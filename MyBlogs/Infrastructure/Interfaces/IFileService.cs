namespace MyBlogs.Infrastructure.Interfaces
{
    public interface IFileService
    {
        Task<string> UploadAsync(IFormFile file);
        void Delete(string filePath);
        bool IsValidExtension(string fileName);
    }

}
