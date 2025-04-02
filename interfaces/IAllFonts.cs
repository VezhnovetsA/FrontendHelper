using FrontendHelper.Models;
namespace FrontendHelper.interfaces
{
    public interface IAllFonts
    {
        IEnumerable<Font> Fonts { get; }
        Font GetFont(int fontId);
    }
}
