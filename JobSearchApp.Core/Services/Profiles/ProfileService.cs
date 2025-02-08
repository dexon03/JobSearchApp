using System.Net;
using AutoMapper;
using JobSearchApp.Core.Contracts.Profiles;
using JobSearchApp.Core.Exceptions;
using JobSearchApp.Core.Models.Profiles;
using JobSearchApp.Core.Models.Vacancies;
using JobSearchApp.Data;
using JobSearchApp.Data.Models.Profiles;
using JobSearchApp.Data.Models.Profiles.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Role = JobSearchApp.Data.Enums.Role;

namespace JobSearchApp.Core.Services.Profiles;

public class ProfileService(
    AppDbContext db,
    IMapper mapper,
    HybridCache hybridCache,
    IPdfService pdfService)
    : IProfileService
{
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
            .Select(profile => new GetCandidateProfileDto
            {
                Id = profile.Id,
                Name = profile.Name,
                Surname = profile.Surname,
                Email = profile.Email ?? string.Empty,
                PhoneNumber = profile.PhoneNumber ?? string.Empty,
                DateBirth = profile.DateBirth,
                Description = profile.Description ?? string.Empty,
                ImageUrl = profile.ImageUrl ?? string.Empty,
                PositionTitle = profile.PositionTitle ?? string.Empty,
                IsActive = profile.IsActive,
                WorkExperience = profile.WorkExperience,
                DesiredSalary = profile.DesiredSalary,
                Attendance = profile.Attendance,
                Skills = profile.ProfileSkills
                    .Select(ps => new SkillDto
                    {
                        Id = ps.Skill.Id,
                        Name = ps.Skill.Name
                    }),
                Locations = profile.LocationProfiles
                    .Select(pl => new LocationDto
                    {
                        Id = pl.Location.Id,
                        City = pl.Location.City,
                        Country = pl.Location.Country
                    }),
            })
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
        var profileEntity = await db.RecruiterProfile.FindAsync(recruiterId);
        if (profileEntity == null)
        {
            throw new ExceptionWithStatusCode("Profile not found", HttpStatusCode.BadRequest);
        }

        var profile = new GetRecruiterProfileDto
        {
            Id = profileEntity.Id,
            Name = profileEntity.Name,
            Surname = profileEntity.Surname,
            Email = profileEntity.Email ?? String.Empty,
            PhoneNumber = profileEntity.PhoneNumber ?? String.Empty,
            DateBirth = profileEntity.DateBirth,
            Description = profileEntity.Description ?? String.Empty,
            ImageUrl = profileEntity.ImageUrl ?? String.Empty,
            PositionTitle = profileEntity.PositionTitle ?? String.Empty,
            IsActive = profileEntity.IsActive,
            UserId = profileEntity.UserId
        };

        return profile;
    }

    public async Task<GetCandidateProfileDto> GetCandidateProfile(int profileId)
    {
        var profileEntity = await db.CandidateProfile
            .Where(p => p.Id == profileId)
            .Include(p => p.ProfileSkills)
            .ThenInclude(ps => ps.Skill)
            .Include(p => p.LocationProfiles)
            .ThenInclude(pl => pl.Location)
            .Select(profile => new GetCandidateProfileDto
            {
                Id = profile.Id,
                Name = profile.Name,
                Surname = profile.Surname,
                Email = profile.Email ?? string.Empty,
                PhoneNumber = profile.PhoneNumber ?? string.Empty,
                DateBirth = profile.DateBirth,
                Description = profile.Description ?? string.Empty,
                ImageUrl = profile.ImageUrl ?? string.Empty,
                PositionTitle = profile.PositionTitle ?? string.Empty,
                IsActive = profile.IsActive,
                WorkExperience = profile.WorkExperience,
                DesiredSalary = profile.DesiredSalary,
                Attendance = profile.Attendance,
                UserId = profile.UserId,

                Skills = profile.ProfileSkills
                    .Select(ps => new SkillDto
                    {
                        Id = ps.Skill.Id,
                        Name = ps.Skill.Name
                    })
                    .ToList(),

                Locations = profile.LocationProfiles
                    .Select(pl => new LocationDto
                    {
                        Id = pl.Location.Id,
                        City = pl.Location.City,
                        Country = pl.Location.Country
                    })
                    .ToList()
            })
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
            .Include(p => p.ProfileSkills)
            .ThenInclude(ps => ps.Skill)
            .Include(p => p.LocationProfiles)
            .ThenInclude(pl => pl.Location)
            .Select(profile => new GetCandidateProfileDto
            {
                Id = profile.Id,
                Name = profile.Name,
                Surname = profile.Surname,
                Email = profile.Email ?? string.Empty,
                PhoneNumber = profile.PhoneNumber ?? string.Empty,
                DateBirth = profile.DateBirth,
                Description = profile.Description ?? string.Empty,
                ImageUrl = profile.ImageUrl ?? string.Empty,
                PositionTitle = profile.PositionTitle ?? string.Empty,
                IsActive = profile.IsActive,
                WorkExperience = profile.WorkExperience,
                DesiredSalary = profile.DesiredSalary,
                Attendance = profile.Attendance,
                UserId = userId,
                Skills = profile.ProfileSkills
                    .Select(ps => new SkillDto
                    {
                        Id = ps.Skill.Id,
                        Name = ps.Skill.Name
                    })
                    .ToList(),

                Locations = profile.LocationProfiles
                    .Select(pl => new LocationDto
                    {
                        Id = pl.Location.Id,
                        City = pl.Location.City,
                        Country = pl.Location.Country
                    })
                    .ToList()
            })
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
                .AsSplitQuery()
                .Include(rp => rp.Company)
                .Select(t => new GetRecruiterProfileDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    Surname = t.Surname,
                    Email = t.Email,
                    PhoneNumber = t.PhoneNumber,
                    DateBirth = t.DateBirth,
                    Description = t.Description,
                    ImageUrl = t.ImageUrl,
                    PositionTitle = t.PositionTitle,
                    IsActive = t.IsActive,
                    Company = t.Company,
                    UserId = t.UserId,
                }).FirstOrDefaultAsync();

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

    public async Task<CandidateProfile> UpdateCandidateProfile(CandidateProfileUpdateDto profileDto)
    {
        var profile = await GetProfile(profileDto);

        if (profileDto.PdfResume is not null)
        {
            await UploadPdf(profile, profileDto.PdfResume);
        }

        db.Update(profile);
        await db.SaveChangesAsync();

        return profile;
    }

    public async Task<RecruiterProfile> UpdateRecruiterProfile(RecruiterProfileUpdateDto profileDto)
    {
        var profile = await GetProfile(profileDto);

        db.Update(profile);
        await db.SaveChangesAsync();

        return profile;
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

    public async Task DeleteProfileByUserId<T>(int userId) where T : Profile<T>
    {
        var profile = db.Set<T>().FirstOrDefault(p => p.UserId == userId);
        if (profile is not null)
        {
            db.Set<T>().Remove(profile);
            await db.SaveChangesAsync();
            if (typeof(T) == typeof(CandidateProfile))
            {
                await pdfService.DeletePdf((profile as CandidateProfile)!);
            }
        }
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

    private async Task<T> GetProfile<T>(ProfileUpdateDto<T> profileDto) where T : Profile<T>
    {
        var profileEntity = await db.Set<T>().FindAsync(profileDto.Id);
        if (profileEntity == null)
        {
            throw new ExceptionWithStatusCode("Profile that you trying to update, not exist",
                HttpStatusCode.BadRequest);
        }

        mapper.Map(profileDto, profileEntity);
        return profileEntity;
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