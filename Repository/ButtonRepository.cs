using FrontendHelper.Data;
using FrontendHelper.interfaces;
using FrontendHelper.Models;

namespace FrontendHelper.Repository
{
    public class ButtonRepository : IAllButtons
    {
        private readonly ApplicationDbContext context;

        public ButtonRepository(ApplicationDbContext context)
        {
            this.context = context;
        }
        public IEnumerable<Button> Buttons => context.Buttons;

        public Button GetButton(int buttonId) => context.Buttons.FirstOrDefault(p => p.Id == buttonId);
    }
}
