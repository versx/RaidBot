namespace T.Ocr.RaidConfigurations
{
    using SixLabors.Primitives;

    public class GalaxyS9PlusConfiguration : RaidImageConfiguration
    {
        protected override Rectangle GymNamePosition => new Rectangle(220, 110, 860, 100);
        protected override Rectangle PokemonNamePosition => new Rectangle(0, 480, 1080, 140);
        protected override Rectangle RaidTimerPosition => new Rectangle(770, 1125, 180, 50);
        protected override Rectangle EggTimerPosition => new Rectangle(400, 385, 270, 70);
        protected override Rectangle EggLevelPosition => new Rectangle(285, 545, 510, 80);

        public GalaxyS9PlusConfiguration() : base(1440, 2960) { }
    }
}