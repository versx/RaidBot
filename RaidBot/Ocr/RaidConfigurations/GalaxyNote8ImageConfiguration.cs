namespace T.Ocr.RaidConfigurations
{
    using SixLabors.Primitives;

    public class GalaxyNote8ImageConfiguration : RaidImageConfiguration
    {
        protected override Rectangle GymNamePosition => new Rectangle(230, 180, 860, 90);
        protected override Rectangle PokemonNamePosition => new Rectangle(0, 505, 1080, 140);
        protected override Rectangle RaidTimerPosition => new Rectangle(770, 1100, 180, 50);
        protected override Rectangle EggTimerPosition => new Rectangle(400, 430, 270, 70);
        protected override Rectangle EggLevelPosition => new Rectangle(285, 570, 510, 80); //545

        public GalaxyNote8ImageConfiguration() : base(720, 1480) { }
    }
}