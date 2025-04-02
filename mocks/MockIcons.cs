using FrontendHelper.interfaces;
using FrontendHelper.Models;
namespace FrontendHelper.mocks
{
    public class MockIcons : IAllIcons
    {
        public IEnumerable<Icon> Icons
        {
            get
            {
                return new List<Icon>
                {
                    new Icon { Id = 1, Name = "Vegetable bucket" , Topic = "Vegetables", Img = "/images/basket.png"},
                    new Icon { Id = 2, Name = "Christmas bell" , Topic = "Christmas", Img = "/images/christmas.png"},
                    new Icon { Id = 3, Name = "Christmas bell" , Topic = "Christmas", Img = "/images/christmas-tree.png"},
                    new Icon { Id = 4, Name = "Fruits" , Topic = "Fruits", Img = "/images/fruit.png"},
                    new Icon { Id = 5, Name = "Skate" , Topic = "Sport", Img = "/images/ice-skate.png"},
                    new Icon { Id = 6, Name = "Snowboard" , Topic = "Sport", Img = "/images/snowboard.png"}

                };

            }

        }


        public Icon GetIcon(int iconId)
        {
            throw new NotImplementedException();
        }
    }
}
