namespace T.Ocr.RaidConfigurations
{
    using SixLabors.ImageSharp;
    using SixLabors.Primitives;

    public class GalaxyS8NoMenuImageConfiguration : RaidImageConfiguration
    {
        //protected override Rectangle GymNamePosition => new Rectangle(220, 230, 860, 90);
        protected override Rectangle EggTimerPosition => new Rectangle(400, 470, 270, 60);
        protected override Rectangle EggLevelPosition => new Rectangle(285, 625, 510, 90);
        protected override Rectangle PokemonNamePosition => new Rectangle(0, 580, 1080, 140);
        protected override Rectangle RaidTimerPosition => new Rectangle(820, 1255, 180, 40);

        public GalaxyS8NoMenuImageConfiguration() : base(1080, 2076) { }

        public override void PreProcessImage<TPixel>(Image<TPixel> image) { }
    }
}