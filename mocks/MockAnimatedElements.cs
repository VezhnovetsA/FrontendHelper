using FrontendHelper.interfaces;
using FrontendHelper.Models;

namespace FrontendHelper.mocks
{
    public class MockAnimatedElements : IAllAnimatedElements
    {
        public IEnumerable<AnimatedElement> AnimatedElements
        {
            get
            {
                return new List<AnimatedElement>
                {
                    new AnimatedElement { Id = 1, Name = "Balloons" , Topic = "Party", Img = "/images/animated_elements/balloons.gif"},
                    new AnimatedElement { Id = 2, Name = "Christmas bell" , Topic = "Christmas", Img = "/images/animated_elements/german-shepherd.gif"},
                    new AnimatedElement{ Id = 3, Name = "Plant" , Topic = "Nature", Img = "/images/animated_elements/planting.gif"},
                    

                };

            }

        }

        AnimatedElement IAllAnimatedElements.GetAnimatedElement(int animatedElementId)
        {
            throw new NotImplementedException();
        }
    }
}
