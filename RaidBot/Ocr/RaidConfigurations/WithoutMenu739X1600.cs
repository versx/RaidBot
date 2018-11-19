namespace T.Ocr.RaidConfigurations
{
    using SixLabors.ImageSharp;
    using SixLabors.ImageSharp.Processing;

    public class WithoutMenu739X1600 : IPhoneXImageConfiguration
	{
		public override void PreProcessImage<TPixel>(Image<TPixel> image)
		{
			image.Mutate(m => m.Resize(1080, 2339));
			base.PreProcessImage(image);
		}
	}
}