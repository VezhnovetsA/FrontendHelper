using FrontendHelper.Models;

namespace FrontendHelper.interfaces
{
    public interface IAllButtons
    {
        IEnumerable<Button> Buttons { get; }
        Button GetButton(int buttonId);
    }
}
