namespace T.Ocr.RaidConfigurations
{
    using SixLabors.ImageSharp;
    using SixLabors.ImageSharp.PixelFormats;
    using SixLabors.ImageSharp.Processing;
    using SixLabors.Primitives;

    public class IPhoneXImageConfiguration : RaidImageConfiguration
	{
		protected override Rectangle EggTimerPosition => new Rectangle(400, 595, 270, 70);
		protected override Rectangle EggLevelPosition => new Rectangle(285, 755, 510, 80);
		protected override Rectangle PokemonNamePosition => new Rectangle(0, 580, 1080, 180);
        protected override Rectangle PokemonCpPosition => new Rectangle(180, 400, 740, 190);
        protected override Rectangle RaidTimerPosition => new Rectangle(820, 1360, 180, 50);

		public IPhoneXImageConfiguration() : base(1125, 2436, 1080, 2339) { }

		public override void PreProcessImage<TPixel>(Image<TPixel> image)
		{
			image.Mutate(m => m.Brightness(1.09f));
			base.PreProcessImage(image);
		}

        public override Image<Rgba32> PreProcessPokemonCpFragment(Image<Rgba32> imageFragment)
        {
            return imageFragment;
        }
    }
}