using JobSearchApp.Core.Models.Profiles;
using JobSearchApp.Data.Models.Profiles.Common;

namespace JobSearchApp.Core.Contracts.Profiles;

public interface IProfileService
{
    Task<List<GetCandidateProfileDto>> GetAllCandidatesProfiles(CandidateFilterParameters filter);
    Task<GetRecruiterProfileDto> GetRecruiterProfile(int recruiterId);
    Task<GetCandidateProfileDto> GetCandidateProfile(int profileId);
    Task<GetCandidateProfileDto> GetCandidateProfileByUserId(int userId);
    Task<GetRecruiterProfileDto> GetRecruiterProfileByUserId(int userId);
    Task CreateProfile(ProfileCreateDto profile);
    Task<GetCandidateProfileDto> UpdateCandidateProfile(CandidateProfileUpdateDto profileDto);
    Task<GetRecruiterProfileDto> UpdateRecruiterProfile(RecruiterProfileUpdateDto profileDto);
    Task UploadResume(ResumeUploadDto resumeDto);
    Task<byte[]?> DownloadResume(int candidateId);
    Task DeleteProfile<T>(int id) where T : Profile<T>;
    Task DeleteProfileByUserId(int userId);
    Task<string> GenerateProfileDescription(int userId, AiDescriptionRequest request);
    Task ActivateDeactivateProfile<T>(int id) where T : Profile<T>;
}