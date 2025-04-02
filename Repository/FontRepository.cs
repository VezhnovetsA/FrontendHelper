using FrontendHelper.Data;
using FrontendHelper.interfaces;
using FrontendHelper.Models;

namespace FrontendHelper.Repository
{
    public class FontRepository : IAllFonts
    {
        private readonly ApplicationDbContext context;

        public FontRepository(ApplicationDbContext context)
        {
            this.context = context;
        }
        public IEnumerable<Font> Fonts => context.Fonts;

        public Font GetFont(int fontId) => context.Fonts.FirstOrDefault(p => p.Id == fontId);
    }
}
