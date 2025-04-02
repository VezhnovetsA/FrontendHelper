using FrontendHelper.Models;

namespace FrontendHelper.interfaces
{
    public interface IAllIcons
    {
        IEnumerable<Icon> Icons { get; }
        Icon GetIcon(int iconId);
    }
}
