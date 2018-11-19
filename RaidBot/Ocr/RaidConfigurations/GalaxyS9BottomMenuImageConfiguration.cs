namespace T.Ocr.RaidConfigurations
{
    using SixLabors.ImageSharp;
    using SixLabors.ImageSharp.Processing;
    using SixLabors.Primitives;

    public class GalaxyS9BottomMenuImageConfiguration : RaidImageConfiguration
	{
		public GalaxyS9BottomMenuImageConfiguration() : base(1080, 1920) { }

		public override void PreProcessImage<TPixel>(Image<TPixel> image)
		{
			image.Mutate(m => m.Crop(new Rectangle(0, 156, 1440, 2562)));
			base.PreProcessImage(image);
		}
	}
}