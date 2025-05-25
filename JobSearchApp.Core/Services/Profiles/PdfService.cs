using System.Diagnostics.CodeAnalysis;
using JobSearchApp.Core.Contracts.Profiles;
using JobSearchApp.Data.Models.Profiles;
using Microsoft.AspNetCore.Http;

namespace JobSearchApp.Core.Services.Profiles;

[ExcludeFromCodeCoverage]
public class PdfService : IPdfService
{
    private readonly string _fileStoragePath = Path.Combine(Directory.GetCurrentDirectory(), "PdfResumesFiles");
    
    public async Task UploadPdf(IFormFile formFile, CandidateProfile profile)
    {
        var filePath = Path.Combine(_fileStoragePath, GetPdfFileIdentifier(profile));
        if (!Directory.Exists(filePath))
        {
            Directory.CreateDirectory(filePath);
        }
        DirectoryInfo di = new DirectoryInfo(filePath);
        foreach (FileInfo file in di.GetFiles())
        {
            file.Delete(); 
        }
        await using var fileStream = new FileStream(Path.Combine(filePath, formFile.FileName), FileMode.Create);
        await formFile.CopyToAsync(fileStream);
    }

    public async Task<string> GetPdfAsAString(CandidateProfile profile)
    {
        var filePath = Path.Combine(_fileStoragePath, GetPdfFileIdentifier(profile));
        if (!Directory.Exists(filePath))
        {
            return string.Empty;
        }
        var file = Directory.GetFiles(filePath).FirstOrDefault();
        if (file == null)
        {
            return string.Empty;
        }
        
        return await File.ReadAllTextAsync(file);
    }

    public bool CheckIfResumeFolderInitialised(CandidateProfile profile)
    {
        var filePath = Path.Combine(_fileStoragePath, GetPdfFileIdentifier(profile));

        if (Directory.Exists(filePath))
        {
            return true;
        }

        return false;
    }

    public async Task<bool> CheckIfPdfExistsAndEqual(CandidateProfile profile, IFormFile? formFile = null)
    {
        var filePath = Path.Combine(_fileStoragePath, GetPdfFileIdentifier(profile));

        if (!Directory.Exists(filePath))
        {
            return false;
        }

        if (formFile != null)
        {
            filePath = Path.Combine(filePath, formFile.FileName);

            if (File.Exists(filePath) && await AreFilesEqualAsync(filePath, formFile))
            {
                return true;
            }
        }

        return false;
    }

    private async Task<bool> AreFilesEqualAsync(string filePath, IFormFile formFile)
    {
        byte[] existingFileBytes = await File.ReadAllBytesAsync(filePath);
    
        return existingFileBytes.Length == formFile.Length;
    }

    public Task DeletePdf(CandidateProfile profile)
    {
        var filePath = Path.Combine(_fileStoragePath, GetPdfFileIdentifier(profile));
        if (Directory.Exists(filePath))
        {
            Directory.Delete(filePath, true);
        }
        return Task.CompletedTask;
    }

    public Task<byte[]> DownloadPdf(CandidateProfile profile)
    {
        var filePath = Path.Combine(_fileStoragePath, GetPdfFileIdentifier(profile));
        if (!Directory.Exists(filePath))
        {
            throw new FileNotFoundException("File not found");
        }
        var file = Directory.GetFiles(filePath).FirstOrDefault();
        if (file == null)
        {
            throw new FileNotFoundException();
        }
        
        return File.ReadAllBytesAsync(file);
    }

    private string GetPdfFileIdentifier(CandidateProfile profile)
    {
        return $"{profile.Id}-{profile.Email}";
    }
}