namespace T.Ocr.RaidConfigurations
{
    using SixLabors.ImageSharp;
    using SixLabors.Primitives;

    public class BothMenu1280X720Configuration : RaidImageConfiguration
    {
        protected override Rectangle GymNamePosition => new Rectangle(130, 75, 720, 90);
        protected override Rectangle PokemonNamePosition => new Rectangle(0, 480, 720, 140);
        protected override Rectangle RaidTimerPosition => new Rectangle(720, 1150, 180, 50);
        protected override Rectangle EggTimerPosition => new Rectangle(270, 235, 180, 60);
        protected override Rectangle EggLevelPosition => new Rectangle(150, 335, 420, 60);

        public BothMenu1280X720Configuration() : base(720, 1280) { }

        public override void PreProcessImage<TPixel>(Image<TPixel> image) { }
    }
}