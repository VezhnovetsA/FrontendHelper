using FrontendHelper.Services.Interfaces;

namespace FrontendHelper.Services
{
    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _environment;

        public FileService(IWebHostEnvironment enviroment)
        {
            _environment = enviroment;
        }




        public async Task<string> SaveFileAsync(IFormFile file, string subFolder)
        {
            if (file == null || file.Length == 0)
                return string.Empty;

            var extension = Path.GetExtension(file.FileName);

            var fileName = $"{Guid.NewGuid()}{extension}";

            var pathToFolder = Path.Combine(_environment.WebRootPath, subFolder);
            if (!Directory.Exists(pathToFolder))
                Directory.CreateDirectory(pathToFolder);

            var pathToFile = Path.Combine(pathToFolder, fileName);
            using var stream = new FileStream(pathToFile, FileMode.Create);
            await file.CopyToAsync(stream);

            return fileName;
        }




        public void DeleteFile(string fileName, string subFolder)
        {
            if (string.IsNullOrEmpty(fileName))
                return;

            var path = Path.Combine(_environment.WebRootPath, subFolder, fileName);
            if (File.Exists(path))
                File.Delete(path);
        }




        public async Task<string> ReplaceFileAsync(IFormFile newFile, string oldFileName, string subFolder)
        {
            DeleteFile(oldFileName, subFolder);
            return await SaveFileAsync(newFile, subFolder);
        }
    }
}
