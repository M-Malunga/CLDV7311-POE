using Microsoft.AspNetCore.Http;

namespace ST10296771_CLDV7311_POE.Services
{
    public interface IBlobStorageService
    {
        Task<string> UploadImageAsync(IFormFile file, string fileName);
        Task<bool> DeleteImageAsync(string fileName);
        Task<string> GetImageUrlAsync(string fileName);
        bool IsValidImage(IFormFile file);
    }
}