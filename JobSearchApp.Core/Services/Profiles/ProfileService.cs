using System.Net;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using JobSearchApp.Core.Contracts.Profiles;
using JobSearchApp.Core.Exceptions;
using JobSearchApp.Core.MessageContracts;
using JobSearchApp.Core.Models.Profiles;
using JobSearchApp.Data;
using JobSearchApp.Data.Models.Profiles;
using JobSearchApp.Data.Models.Profiles.Common;
using MassTransit;
using MassTransit.Testing;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Serilog;
using ZiggyCreatures.Caching.Fusion;
using Role = JobSearchApp.Data.Enums.Role;

namespace JobSearchApp.Core.Services.Profiles;

public class ProfileService(
    AppDbContext db,
    IMapper mapper,
    IChatClient chatClient,
    ILogger logger,
    IPdfService pdfService,
    IFusionCache hybridCache,
    IPublishEndpoint publishEndpoint)
    : IProfileService
{
    private readonly ILogger _logger = logger.ForContext<ProfileService>();

    public async Task<List<GetCandidateProfileDto>> GetAllCandidatesProfiles(CandidateFilterParameters filter)
    {
        var profileQuery = db.CandidateProfile.AsQueryable();

        profileQuery = ApplyFilters();

        var profileEntities = await profileQuery
            .Include(p => p.ProfileSkills)
            .ThenInclude(ps => ps.Skill)
            .Include(p => p.LocationProfiles)
            .ThenInclude(pl => pl.Location)
            .OrderBy(p => p.Id)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ProjectTo<GetCandidateProfileDto>(mapper.ConfigurationProvider)
            .ToListAsync();

        return profileEntities;

        IQueryable<CandidateProfile> ApplyFilters()
        {
            if (filter.SearchTerm is not null)
            {
                profileQuery = profileQuery.Where(p => p.Name.ToLower().Contains(filter.SearchTerm.ToLower())
                                                       || p.Surname.ToLower().Contains(filter.SearchTerm.ToLower())
                                                       || p.PositionTitle.ToLower()
                                                           .Contains(filter.SearchTerm.ToLower())
                                                       || p.Description.ToLower()
                                                           .Contains(filter.SearchTerm.ToLower()));
            }

            if (filter.Skill is not null)
            {
                profileQuery = profileQuery
                    .Include(p => p.ProfileSkills)!.ThenInclude(ps => ps.Skill)
                    .Where(p => p.ProfileSkills.Any(ps => filter.Skill == ps.Skill.Id));
            }

            if (filter.Location is not null)
            {
                profileQuery = profileQuery
                    .Include(p => p.LocationProfiles)!.ThenInclude(lp => lp.Location)
                    .Where(p => p.LocationProfiles.Any(lp => filter.Location == lp.Location.Id));
            }

            if (filter.Experience is not null)
            {
                profileQuery = profileQuery.Where(p => p.WorkExperience == filter.Experience);
            }

            if (filter.AttendanceMode is not null)
            {
                profileQuery = profileQuery.Where(p => p.Attendance == filter.AttendanceMode);
            }

            return profileQuery;
        }
    }

    public async Task<GetRecruiterProfileDto> GetRecruiterProfile(int recruiterId)
    {
        var profileEntity = await db.RecruiterProfile
            .Include(x => x.Company)
            .FirstOrDefaultAsync(x => x.Id == recruiterId);

        if (profileEntity == null)
        {
            throw new ExceptionWithStatusCode("Profile not found", HttpStatusCode.BadRequest);
        }

        var profile = mapper.Map<GetRecruiterProfileDto>(profileEntity);

        return profile;
    }

    public async Task<GetCandidateProfileDto> GetCandidateProfile(int profileId)
    {
        var profileEntity = await db.CandidateProfile
            .Where(p => p.Id == profileId)
            .ProjectTo<GetCandidateProfileDto>(mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();

        if (profileEntity == null)
        {
            throw new ExceptionWithStatusCode("Profile not found", HttpStatusCode.BadRequest);
        }

        return profileEntity;
    }

    public async Task<GetCandidateProfileDto> GetCandidateProfileByUserId(int userId)
    {
        var profileEntity = await db.CandidateProfile
            .Where(p => p.UserId == userId)
            .ProjectTo<GetCandidateProfileDto>(mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();

        if (profileEntity == null)
        {
            throw new ExceptionWithStatusCode("Profile not found", HttpStatusCode.BadRequest);
        }

        return profileEntity;
    }

    public async Task<GetRecruiterProfileDto> GetRecruiterProfileByUserId(int userId)
    {
        var profileEntity =
            await db.RecruiterProfile
                .Where(rp => rp.UserId == userId)
                .ProjectTo<GetRecruiterProfileDto>(mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();

        if (profileEntity == null)
        {
            throw new ExceptionWithStatusCode("Profile not found", HttpStatusCode.BadRequest);
        }

        return profileEntity;
    }

    public async Task CreateProfile(ProfileCreateDto profile)
    {
        if (profile.Role == Role.Candidate)
        {
            var profileEntity = new CandidateProfile().MapCreateToCandidateProfile(profile);
            db.CandidateProfile.Add(profileEntity);
        }
        else
        {
            var profileEntity = new RecruiterProfile().MapCreateToRecruiterProfile(profile);
            db.RecruiterProfile.Add(profileEntity);
        }

        await db.SaveChangesAsync();
    }

    public async Task<GetCandidateProfileDto> UpdateCandidateProfile(CandidateProfileUpdateDto profileDto)
    {
        var profile = await db.CandidateProfile
            .Include(x => x.ProfileSkills)
            .Include(x => x.LocationProfiles)
            .FirstOrDefaultAsync(x => x.Id == profileDto.Id);
        if (profile == null)
        {
            throw new ExceptionWithStatusCode("Profile that you trying to update, not exist",
                HttpStatusCode.BadRequest);
        }

        mapper.Map(profileDto, profile);

        if (profileDto.PdfResume is not null)
        {
            await UploadPdf(profile, profileDto.PdfResume);
        }

        var entityEntry = db.Update(profile);
        await db.SaveChangesAsync();

        await publishEndpoint.Publish(new CandidateProfileUpdatedEvent
        {
            ProfileId = entityEntry.Entity.Id
        });
        await hybridCache.RemoveByTagAsync("recommended_vacancies");

        var result = mapper.Map<GetCandidateProfileDto>(entityEntry.Entity);
        return result;
    }

    public async Task<GetRecruiterProfileDto> UpdateRecruiterProfile(RecruiterProfileUpdateDto profileDto)
    {
        var profile = await db.RecruiterProfile.FindAsync(profileDto.Id);
        if (profile == null)
        {
            throw new ExceptionWithStatusCode("Profile that you trying to update, not exist",
                HttpStatusCode.BadRequest);
        }

        mapper.Map(profileDto, profile);

        var entity = db.Update(profile);
        await db.SaveChangesAsync();

        var result = mapper.Map<GetRecruiterProfileDto>(entity.Entity);

        return result;
    }

    public async Task UploadResume(ResumeUploadDto resumeDto)
    {
        var candidateProfile = await db.CandidateProfile.FindAsync(resumeDto.CandidateId);
        if (candidateProfile is null)
        {
            throw new ExceptionWithStatusCode("Candidate profile not found", HttpStatusCode.BadRequest);
        }

        await UploadPdf(candidateProfile, resumeDto.Resume);
    }

    public async Task<byte[]?> DownloadResume(int candidateId)
    {
        var candidateProfile = await db.CandidateProfile.FindAsync(candidateId);
        if (candidateProfile == null)
        {
            throw new ExceptionWithStatusCode("Candidate profile not found", HttpStatusCode.BadRequest);
        }

        if (pdfService.CheckIfResumeFolderInitialised(candidateProfile))
        {
            var result = await pdfService.DownloadPdf(candidateProfile);
            return result;
        }

        return null;
    }

    public async Task DeleteProfile<T>(int id) where T : Profile<T>
    {
        var profile = await db.Set<T>().FindAsync(id);
        if (profile == null)
        {
            throw new ExceptionWithStatusCode("Profile not found", HttpStatusCode.BadRequest);
        }

        db.Set<T>().Remove(profile);
        await db.SaveChangesAsync();
    }

    public async Task DeleteProfileByUserId(int userId)
    {
        var candidateProfile = await db.CandidateProfile.FirstOrDefaultAsync(cp => cp.UserId == userId);
        if (candidateProfile != null)
        {
            db.CandidateProfile.Remove(candidateProfile);
        }
        else
        {
            var recruiterProfile = await db.RecruiterProfile.FirstOrDefaultAsync(rp => rp.UserId == userId);
            if (recruiterProfile != null)
            {
                db.RecruiterProfile.Remove(recruiterProfile);
            }
        }
    }

    public async Task<string> GenerateProfileDescription(int userId, AiDescriptionRequest request)
    {
        var userProfile = await db.CandidateProfile
            .Where(cp => cp.UserId == userId)
            .Include(candidateProfile => candidateProfile.ProfileSkills)
            .ThenInclude(profileSkills => profileSkills.Skill)
            .FirstOrDefaultAsync();
        if (userProfile is null)
        {
            throw new ExceptionWithStatusCode("Profile not found", HttpStatusCode.BadRequest);
        }

        var description = string.IsNullOrEmpty(request.Description) ? userProfile.Description : request.Description;

        var prompt = $"""
                      I need to generate big and detailed description for my profile. 
                      I will give you info about me, short description and my pdf resume if it exists(if pdf or/and description does not contain info for my description as candidate then ignore it).
                      You will take that info and generate a rich interesting  description for me.
                      If something empty or not exist, just ignore it.
                      !!!IMPORTANT!!!
                      Give me only this description in Markdown format.Dont need to add any comments or explanations.
                      !!!
                      Here is info about me:
                      Position title: {request.PositionTitle}
                      Work experience: {request.Experience.ToString()}
                      My Skills: {string.Join(", ", userProfile.ProfileSkills.Select(ps => ps.Skill.Name))}
                      Description: {description}
                      """;
        var message = new ChatMessage(ChatRole.User, prompt);
        try
        {
            var fileContent = await pdfService.DownloadPdf(userProfile);
            message.Contents.Add(new DataContent(fileContent, "application/pdf"));
        }
        catch (Exception e)
        {
            _logger.Error(e, "Error while downloading pdf for ai description, skip...");
        }

        var response = await chatClient.GetResponseAsync(message);
        return response.Text;
    }

    public async Task ActivateDeactivateProfile<T>(int id) where T : Profile<T>
    {
        var profile = await db.Set<T>().FindAsync(id);
        if (profile == null)
        {
            throw new ExceptionWithStatusCode("Profile not found", HttpStatusCode.BadRequest);
        }

        profile.IsActive = !profile.IsActive;
        db.Update(profile);
        await db.SaveChangesAsync();
    }

    private async Task UploadPdf(CandidateProfile profile, IFormFile formFile)
    {
        if (Path.GetExtension(formFile.FileName) != ".pdf")
        {
            throw new ExceptionWithStatusCode("File must be pdf", HttpStatusCode.BadRequest);
        }

        if (!await pdfService.CheckIfPdfExistsAndEqual(profile, formFile))
        {
            await pdfService.UploadPdf(formFile, profile);
        }
    }
}