using JobSearchApp.Data.Models.Profiles;
using Microsoft.AspNetCore.Http;

namespace JobSearchApp.Core.Contracts.Profiles;

public interface IPdfService
{
    Task UploadPdf(IFormFile formFile, CandidateProfile profile);
    Task<byte[]> DownloadPdf(CandidateProfile profile);
    Task<string> GetPdfAsAString(CandidateProfile profile);
    bool CheckIfResumeFolderInitialised(CandidateProfile profile);
    Task<bool> CheckIfPdfExistsAndEqual(CandidateProfile profile, IFormFile? formFile = null);
    Task DeletePdf(CandidateProfile profile);
}