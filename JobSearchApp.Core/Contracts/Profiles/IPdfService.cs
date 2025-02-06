using Microsoft.AspNetCore.Http;

namespace JobSearchApp.Core.Contracts.Profiles;

public interface IPdfService
{
    Task UploadPdf(IFormFile formFile, int profileId);
    Task<byte[]> DownloadPdf(int profileId);
    bool CheckIfResumeFolderInitialised(int profileId);
    Task<bool> CheckIfPdfExistsAndEqual(int profileId, IFormFile? formFile = null);
    Task DeletePdf(int profileId);
}