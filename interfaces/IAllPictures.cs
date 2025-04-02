using FrontendHelper.Models;

namespace FrontendHelper.interfaces
{
    public interface IAllPictures
    {
        IEnumerable<Picture> Pictures { get; }
        Picture GetPicture(int pictureId);
    }
}
