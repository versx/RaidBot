namespace T.Ocr.RaidConfigurations
{
    using SixLabors.ImageSharp;
    using SixLabors.Primitives;

    public class IPhoneCallImageConfiguration : RaidImageConfiguration
    {
        protected override Rectangle EggTimerPosition => new Rectangle(400, 445, 300, 75);
        protected override Rectangle EggLevelPosition => new Rectangle(300, 615, 525, 80);
        protected override Rectangle PokemonNamePosition => new Rectangle(0, 525, 1080, 150);
        protected override Rectangle RaidTimerPosition => new Rectangle(830, 1225, 200, 50);
        protected override Rectangle GymNamePosition => new Rectangle(220, 190, 1080, 70);

        public IPhoneCallImageConfiguration() : base(1125, 2001) { }

        public override void PreProcessImage<TPixel>(Image<TPixel> image) { }
    }
}