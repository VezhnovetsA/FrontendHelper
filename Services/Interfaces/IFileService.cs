namespace FrontendHelper.Services.Interfaces
{
    public interface IFileService
    {

        // будет сохранять файл в папку в  wwwroot/папка_ресрсов и возвращать новое имя файла

        Task<string> SaveFileAsync(IFormFile file, string subFolder);

        // удаляет файл с диска из wwwroot/папка_ресурсов
        void DeleteFile(string fileName, string subFolder);

        // сохраняет новый файл и по необходимости удаляет старый
        Task<string> ReplaceFileAsync(IFormFile newFile, string oldFileName, string subFolder);
    }
}
