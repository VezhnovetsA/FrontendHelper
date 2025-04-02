using FrontendHelper.Data;
using FrontendHelper.Models;

namespace FrontendHelper.Repository
{
    public static class DbInitializer
    {
        public static void Initialize(ApplicationDbContext context)
        {
            if (!context.Icons.Any())
            {
                var icons = new[]
                {
                    new Icon {Name = "Vegetable bucket" , Topic = "Vegetables", Img = "/images/basket.png"},
                    new Icon {Name = "Christmas bell" , Topic = "Christmas", Img = "/images/christmas.png"},
                    new Icon { Name = "Christmas bell" , Topic = "Christmas", Img = "/images/christmas-tree.png"},
                    new Icon { Name = "Fruits" , Topic = "Fruits", Img = "/images/fruit.png"},
                    new Icon { Name = "Skate" , Topic = "Sport", Img = "/images/ice-skate.png"},
                    new Icon { Name = "Snowboard" , Topic = "Sport", Img = "/images/snowboard.png"}
            };

                context.Icons.AddRange(icons);
            }


            if (!context.Pictures.Any())
            {
                var pictures = new[]
                {
                    new Picture { Name = "Bananas" , Topic = "Wallpapers", Img = "/images/bananas.jpg"},
                    new Picture { Name = "Castle" , Topic = "Castle", Img = "/images/castle.jpg"},
                    new Picture { Name = "Futuristic car" , Topic = "Future", Img = "/images/futuristic-car.jpg"},
                    new Picture { Name = "River" , Topic = "Nature", Img = "/images/river.jpg"},
                    new Picture { Name = "Watermelon lake" , Topic = "Watermelons", Img = "/images/watermelons.jpg"},
                    new Picture { Name = "Bird in cave" , Topic = "Birds", Img = "/images/white-bird.jpg"}
            };

                context.Pictures.AddRange(pictures);
            }


            if (!context.AnimatedElements.Any())
            {
                var animatedElements = new[]
                {
                    new AnimatedElement { Name = "Balloons" , Topic = "Party", Img = "/images/animated_elements/balloons.gif"},
                    new AnimatedElement { Name = "Christmas bell" , Topic = "Christmas", Img = "/images/animated_elements/german-shepherd.gif"},
                    new AnimatedElement{ Name = "Plant" , Topic = "Nature", Img = "/images/animated_elements/planting.gif"},
            };

                context.AnimatedElements.AddRange(animatedElements);
            }

            //if (!context.Buttons.Any())
            //{
            //    var buttons = new[]
            //    {
            //        new Button { Name = "Effect" , ButtonCode = "/buttons/effectbutton.html"},
            //        new Button { Name = "Gradient" , ButtonCode = "/buttons/gradientbutton.html"},
            //        new Button { Name = "Load" , ButtonCode = "/buttons/loadbutton.html"},
            //        new Button { Name = "Shadow" , ButtonCode = "/buttons/shadowbutton.html"},
            //        new Button { Name = "Simple" , ButtonCode = "/buttons/simplebutton.html"},
            //        new Button { Name = "Transparent" , ButtonCode = "/buttons/transparentbutton.html"},
            //};

            //    context.Buttons.AddRange(buttons);
            //}


            if (!context.Buttons.Any())
            {

                var buttonFilesPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/css/buttons");

                var buttons = new[]
                {
                new Button
                {
                    Name = "Effect",
                    ButtonCode = File.ReadAllText(Path.Combine(buttonFilesPath, "effectbutton.html"))
                },
                new Button
                {
                    Name = "Gradient",
                    ButtonCode = File.ReadAllText(Path.Combine(buttonFilesPath, "gradientbutton.html"))
                },
                new Button
                {
                    Name = "Load",
                    ButtonCode = File.ReadAllText(Path.Combine(buttonFilesPath, "loadbutton.html"))
                },
                new Button
                {
                    Name = "Shadow",
                    ButtonCode = File.ReadAllText(Path.Combine(buttonFilesPath, "shadowbutton.html"))
                },
                new Button
                {
                    Name = "Simple",
                    ButtonCode = File.ReadAllText(Path.Combine(buttonFilesPath, "simplebutton.html"))
                },
                new Button
                {
                    Name = "Transparent",
                    ButtonCode = File.ReadAllText(Path.Combine(buttonFilesPath, "transparentbutton.html"))
                }
            };

                context.Buttons.AddRange(buttons);
                context.SaveChanges();
            }



            if (!context.Fonts.Any())
            {
                var fonts = new[]
                {
                    //new Font { Name = "Merriweather" , FontFamily = "Merriweather" , Link ="https://fonts.googleapis.com/css2?family=Merriweather:ital,wght@0,300;0,400;0,700;0,900;1,300;1,400;1,700;1,900&display=swap"},
                    new Font { Name = "Pacifico" , FontFamily = "Pacifico" , Link = "https://fonts.googleapis.com/css2?family=Pacifico&display=swap"},
                    new Font { Name = "Caveat" , FontFamily = "Caveat" , Link = "https://fonts.googleapis.com/css2?family=Caveat:wght@400..700&display=swap"},
                    new Font { Name = "Comfortaa" , FontFamily = "Comfortaa" , Link = "https://fonts.googleapis.com/css2?family=Comfortaa:wght@300..700&display=swap" },
                    //new Font { Name = "Simple" , FontFamily = "/buttons/simplebutton.html" , Link = ""},
                    //new Font { Name = "Transparent" , FontFamily = "/buttons/transparentbutton.html" , Link = ""},
            };

                context.Fonts.AddRange(fonts);
            }

            context.SaveChanges();
        }


        //private static string LoadButtonTemplate(string path)
        //{
        //    if (System.IO.File.Exists(path))
        //    {
        //        return System.IO.File.ReadAllText(path);
        //    }
        //    return string.Empty;
        //}
    }
}
