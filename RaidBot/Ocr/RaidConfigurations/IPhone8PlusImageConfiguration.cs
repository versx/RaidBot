﻿namespace T.Ocr.RaidConfigurations
{
    using SixLabors.Primitives;

    public class IPhone8PlusConfiguration : RaidImageConfiguration
    {
        protected override Rectangle GymNamePosition => new Rectangle(220, 135, 860, 90);
        protected override Rectangle PokemonNamePosition => new Rectangle(0, 505, 1080, 140);
        protected override Rectangle RaidTimerPosition => new Rectangle(810, 1170, 180, 50);
        protected override Rectangle EggTimerPosition => new Rectangle(400, 385, 270, 70);
        protected override Rectangle EggLevelPosition => new Rectangle(285, 545, 510, 80);

        public IPhone8PlusConfiguration() : base(1242, 2208) { }
    }
}