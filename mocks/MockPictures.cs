using FrontendHelper.interfaces;
using FrontendHelper.Models;

namespace FrontendHelper.mocks
{
    public class MockPictures : IAllPictures
    {
        public IEnumerable<Picture> Pictures
        {
            get
            {
                return new List<Picture>
                {
                    new Picture { Id = 1, Name = "Bananas" , Topic = "Wallpapers", Img = "/images/bananas.jpg"},
                    new Picture { Id = 2, Name = "Castle" , Topic = "Castle", Img = "/images/castle.jpg"},
                    new Picture { Id = 3, Name = "Futuristic car" , Topic = "Future", Img = "/images/futuristic-car.jpg"},
                    new Picture { Id = 4, Name = "River" , Topic = "Nature", Img = "/images/river.jpg"},
                    new Picture { Id = 5, Name = "Watermelon lake" , Topic = "Watermelons", Img = "/images/watermelons.jpg"},
                    new Picture { Id = 6, Name = "Bird in cave" , Topic = "Birds", Img = "/images/white-bird.jpg"}

                };

            }

        }

        public Picture GetPicture(int pictureId)
        {
            throw new NotImplementedException();
        }
    }
}
