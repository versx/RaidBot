namespace T.Ocr
{
    using System;
    using System.IO;
    
    using SixLabors.ImageSharp;
    using SixLabors.ImageSharp.PixelFormats;

    using T.Ocr.RaidConfigurations;

    public static class OcrExtensions
    {
        public static RaidImageConfiguration GetConfiguration(this Image<Rgba32> image, bool saveDebugImages = false)
        {
            var configuration = new RaidImageConfiguration(1080, 1920);
            if (image.Height == 2220 && image.Width == 1080)
            {
                if (HasBottomMenu(image))
                {
                    if (HasTopMenu(image))
                    {
                        configuration = new BothMenu1080X2220Configuration();
                    }
                    else
                    {
                        configuration = new BottomMenu1080X2220Configuration();
                    }
                }
                else
                {
                    configuration = new WithoutMenu1080X2220Configuration();
                }
            }

            if (image.Height == 2960 && HasBottomMenu(image))
            {
                configuration = new GalaxyS9BottomMenuImageConfiguration();
            }
            else if (image.Height == 2960 && image.Width == 1440)
            {
                configuration = new GalaxyS9PlusConfiguration();
            }

            if (image.Height == 2436 && image.Width == 1125)
            {
                configuration = new IPhoneXImageConfiguration();
            }

            if (image.Height == 2280 && image.Width == 1080)
            {
                configuration = new BothMenu1080X2280Configuration();
            }

            if (image.Height == 2208 && image.Width == 1242)
            {
                configuration = new IPhone8PlusConfiguration();
            }

            if (image.Height == 2076 && image.Width == 1080)
            {
                if (!image.HasTopMenu() && !image.HasBottomMenu())
                {
                    configuration = new GalaxyS8NoMenuImageConfiguration();
                }
            }

            if (image.Height == 2001 && image.Width == 1125)
            {
                if (HasCallMenu(image))
                {
                    configuration = new IPhoneCallImageConfiguration();
                }
            }

            if (image.Height == 2160 && image.Width == 1080)
            {
                if (!HasTopMenu(image) && HasBottomMenu(image))
                {
                    configuration = new BothMenu1080X2160Configuration();
                }
            }

            if (image.Height == 1920 && image.Width == 1080 && HasBottomMenu(image))
            {
                configuration = new BottomMenu1080X1920Configuration();
            }

            if (image.Height == 1600 && image.Width == 739)
            {
                configuration = new WithoutMenu739X1600();
            }

            if (image.Height == 1600 && image.Width == 900 && HasBottomMenu(image))
            {
                configuration = new BottomMenu900X1600Configuration();
            }

            if (image.Height == 1480 && image.Width == 720)
            {
                configuration = new GalaxyNote8ImageConfiguration();
            }

            if (image.Height == 1280 && image.Width == 720)
            {
                var hasTop = HasTopMenu(image);
                var hasBottom = HasBottomMenu(image);
                if (hasBottom)
                {
                    configuration = new BothMenu1280X720Configuration();
                }
                else
                {
                    configuration = new WithoutMenu1280X720Configuration();
                }
            }

            configuration.SaveDebugImages = saveDebugImages;

            return configuration;
        }

        public static bool HasTopMenu(this Image<Rgba32> image)
        {
            // If the whole line has the exact same color it probably is a menu
            var color = image[0, 0];
            for (int x = 1; x < image.Width; x++)
            {
                if (image[x, 0] != color)
                {
                    return false;
                }
            }
            return true;
        }

        public static bool HasBottomMenu(this Image<Rgba32> image)
        {
            // If the whole line has the exact same color it probably is a menu
            var color = image[0, image.Height - 1];
            for (int x = 1; x < image.Width; x++)
            {
                if (image[x, image.Height - 1] != color)
                {
                    return false;
                }
            }
            return true;
        }

        public static bool HasCallMenu(this Image<Rgba32> image)
        {
            var color = image[0, 0];
            for (int x = 1; x < image.Width; x++)
            {
                if (image[x, 0] != color)
                {
                    return false;
                }
            }

            return true;
        }

        public static string CreateTempImageFile<TPixel>(this Image<TPixel> image) where TPixel : struct, IPixel<TPixel>
        {
            var tempImageFile = Path.GetTempFileName() + ".png";
            image.Save(tempImageFile);
            return tempImageFile;
        }
    }
}