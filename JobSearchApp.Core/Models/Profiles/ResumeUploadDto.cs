using Microsoft.AspNetCore.Http;

namespace JobSearchApp.Core.Models.Profiles;

public record ResumeUploadDto
{
    public int CandidateId { get; set; }
    public IFormFile Resume { get; set; }
};