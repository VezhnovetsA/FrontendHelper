using FrontendHelper.Services.Interfaces;

namespace FrontendHelper.Services
{
    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _enviroment;

        public FileService(IWebHostEnvironment enviroment)
        {
            _enviroment = enviroment;
        }

        public async Task<string> SaveFileAsync(IFormFile file, string subFolder)
        {
            if (file == null || file.Length == 0)
                return string.Empty;

            var extension = Path.GetExtension(file.FileName);

            // рандом имя для папки
            var fileName = $"{Guid.NewGuid()}{extension}";

            // строка пути к папке wwwroot/папка_с_ресурсами:
            var pathToFolder = Path.Combine(_enviroment.WebRootPath, subFolder);
            if (!Directory.Exists(pathToFolder))
                Directory.CreateDirectory(pathToFolder);

            var pathToFile = Path.Combine(pathToFolder, fileName);
            using var stream = new FileStream(pathToFile, FileMode.Create);
            await file.CopyToAsync(stream);

            return fileName;
        }
    }
}
