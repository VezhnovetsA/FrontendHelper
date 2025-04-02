using FrontendHelper.Data;
using FrontendHelper.interfaces;
using FrontendHelper.Models;

namespace FrontendHelper.Repository
{
    public class PictureRepository : IAllPictures
    {
        private readonly ApplicationDbContext context;

        public PictureRepository(ApplicationDbContext context)
        {
            this.context = context;
        }
        public IEnumerable<Picture> Pictures => context.Pictures;

        public Picture GetPicture(int pictureId) => context.Pictures.FirstOrDefault(p => p.Id == pictureId);
    }
}
