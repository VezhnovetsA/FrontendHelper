using FrontendHelper.Data;
using FrontendHelper.Models;
using FrontendHelper.interfaces;

namespace FrontendHelper.Repository
{
    public class AnimatedElementRepository : IAllAnimatedElements
    {
        private readonly ApplicationDbContext context;

        public AnimatedElementRepository(ApplicationDbContext context)
        {
            this.context = context;
        }
        public IEnumerable<AnimatedElement> AnimatedElements => context.AnimatedElements;

        public AnimatedElement GetAnimatedElement(int animatedElementId) => context.AnimatedElements.FirstOrDefault(p => p.Id == animatedElementId);
    }
}
