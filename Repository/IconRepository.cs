using FrontendHelper.Data;
using FrontendHelper.interfaces;
using FrontendHelper.Models;
using Microsoft.EntityFrameworkCore;

namespace FrontendHelper.Repository
{
    public class IconRepository : IAllIcons
    {
        private readonly ApplicationDbContext context;

        public IconRepository(ApplicationDbContext context)
        {
            this.context = context;
        }
        public IEnumerable<Icon> Icons => context.Icons; 

        public Icon GetIcon(int iconId) => context.Icons.FirstOrDefault(p => p.Id == iconId);

        //public Icon GetIconById(int id)
        //{
        //    return context.Icons.FirstOrDefault(i => i.Id == id);
        //}

    }
}
