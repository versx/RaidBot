namespace T.Ocr.RaidConfigurations
{
    using SixLabors.ImageSharp;
    using SixLabors.Primitives;

    public class BottomMenu1080X2220Configuration : RaidImageConfiguration
	{
        protected override Rectangle EggTimerPosition => new Rectangle(400, 470, 270, 70);
        protected override Rectangle EggLevelPosition => new Rectangle(285, 625, 510, 80);
        protected override Rectangle PokemonNamePosition => new Rectangle(0, 520, 1080, 150);
		protected override Rectangle RaidTimerPosition => new Rectangle(820, 1230, 180, 50);

		public BottomMenu1080X2220Configuration() : base(1080, 2220) { }

		public override void PreProcessImage<TPixel>(Image<TPixel> image) {}
	}
}