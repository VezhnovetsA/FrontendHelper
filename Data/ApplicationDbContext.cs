using FrontendHelper.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FrontendHelper.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Icon> Icons { get; set; }
        public DbSet<Picture> Pictures { get; set; }
        public DbSet<AnimatedElement> AnimatedElements { get; set; }
        public DbSet<Button> Buttons { get; set; }
        public DbSet<Font> Fonts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
        }
    }
}
