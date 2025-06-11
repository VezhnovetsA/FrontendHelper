namespace FrontendHelper.Services.Interfaces
{
    public interface IFileService
    {

        Task<string> SaveFileAsync(IFormFile file, string subFolder);

        void DeleteFile(string fileName, string subFolder);

        Task<string> ReplaceFileAsync(IFormFile newFile, string oldFileName, string subFolder);
    }
}
