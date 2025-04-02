using FrontendHelper.Models;

namespace FrontendHelper.interfaces
{
    public interface IAllAnimatedElements
    {
        IEnumerable<AnimatedElement> AnimatedElements { get; }
        AnimatedElement GetAnimatedElement(int animatedElementId);
    }
}
