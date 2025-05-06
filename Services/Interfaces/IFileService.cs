namespace FrontendHelper.Services.Interfaces
{
    public interface IFileService
    {
        /// <summary>
        /// Сохраняет файл в подкаталог wwwroot/{subFolder} и возвращает новое имя файла.
        /// </summary>
        Task<string> SaveFileAsync(IFormFile file, string subFolder);
    }
}
