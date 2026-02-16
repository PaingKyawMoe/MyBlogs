using MyBlogs.Infrastructure.Interfaces;

namespace MyBlogs.Infrastructure
{
    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _env;
        private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png" };

        public FileService(IWebHostEnvironment env)
        {
            _env = env;
        }

        public bool IsValidExtension(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLower();
            return _allowedExtensions.Contains(extension);
        }

        public async Task<string> UploadAsync(IFormFile file)
        {
            var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
            var folder = Path.Combine(_env.WebRootPath, "images");

            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            var path = Path.Combine(folder, fileName);

            await using var stream = new FileStream(path, FileMode.Create);
            await file.CopyToAsync(stream);

            return "/images/" + fileName;
        }

        public void Delete(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) return;

            var fullPath = Path.Combine(_env.WebRootPath, filePath.TrimStart('/'));

            if (File.Exists(fullPath))
                File.Delete(fullPath);
        }
    }

}
