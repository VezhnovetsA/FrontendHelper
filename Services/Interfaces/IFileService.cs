namespace FrontendHelper.Services.Interfaces
{
    public interface IFileService
    {

        /// будет сохранять файл в папку в  wwwroot/папка_ресрсов и возвращать новое имя файла

        Task<string> SaveFileAsync(IFormFile file, string subFolder);
    }
}
