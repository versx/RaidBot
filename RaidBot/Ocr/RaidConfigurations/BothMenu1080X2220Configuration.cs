namespace T.Ocr.RaidConfigurations
{
    using SixLabors.ImageSharp;
    using SixLabors.Primitives;

    public class BothMenu1080X2220Configuration : RaidImageConfiguration
	{
		protected override Rectangle GymNamePosition => new Rectangle(220, 230, 860, 90);
		protected override Rectangle EggTimerPosition => new Rectangle(400, 500, 270, 70);
		protected override Rectangle EggLevelPosition => new Rectangle(285, 660, 510, 80);
		protected override Rectangle PokemonNamePosition => new Rectangle(0, 590, 1080, 150);
		protected override Rectangle RaidTimerPosition => new Rectangle(820, 1265, 180, 50);

		public BothMenu1080X2220Configuration() : base(1080, 2220) { }

		public override void PreProcessImage<TPixel>(Image<TPixel> image) { }
	}

    public class BothMenu1080X2160Configuration : RaidImageConfiguration
    {
        protected override Rectangle GymNamePosition => new Rectangle(220, 230, 860, 90);
        protected override Rectangle EggTimerPosition => new Rectangle(400, 490, 270, 60);
        protected override Rectangle EggLevelPosition => new Rectangle(285, 670, 510, 90);
        protected override Rectangle PokemonNamePosition => new Rectangle(0, 580, 1080, 140);
        protected override Rectangle RaidTimerPosition => new Rectangle(820, 1255, 180, 40);

        public BothMenu1080X2160Configuration() : base(1080, 2160) { }

        public override void PreProcessImage<TPixel>(Image<TPixel> image) { }
    }

    public class BothMenu1080X2280Configuration : RaidImageConfiguration
    {
        protected override Rectangle GymNamePosition => new Rectangle(220, 275, 860, 90);
        protected override Rectangle EggTimerPosition => new Rectangle(400, 545, 270, 70);
        protected override Rectangle EggLevelPosition => new Rectangle(285, 705, 510, 80);
        protected override Rectangle PokemonNamePosition => new Rectangle(0, 535, 1080, 150);
        protected override Rectangle RaidTimerPosition => new Rectangle(820, 1305, 180, 50);

        public BothMenu1080X2280Configuration() : base(1080, 2280) { }

        public override void PreProcessImage<TPixel>(Image<TPixel> image) { }
    }
}