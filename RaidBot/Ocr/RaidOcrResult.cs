namespace T.Ocr
{
    using System;
    using System.Threading;

    public class RaidOcrResult
    {
        private static readonly TimeSpan Infinite = Timeout.InfiniteTimeSpan;

        #region Properties

        public int EggLevel { get; set; }

        public TimeSpan EggTimer { get; set; }

        public string Gym { get; set; }

        public string Pokemon { get; set; }

        public int PokemonCp { get; set; }

        public TimeSpan RaidTimer { get; set; }

        public bool IsRaidImage => EggTimer != Infinite || RaidTimer != Infinite;

        public bool IsRaidBoss => RaidTimer != Infinite && !string.IsNullOrEmpty(Pokemon);

        public bool IsSuccess
        {
            get
            {
                if (!IsRaidImage)
                {
                    return false;
                }

                if (IsRaidBoss)
                {
                    return !string.IsNullOrEmpty(Gym) &&
                           !string.IsNullOrEmpty(Pokemon) &&
                            RaidTimer != Infinite;
                }

                return !string.IsNullOrEmpty(Gym) &&
                        EggLevel > 0 &&
                        EggTimer != Infinite;
            }
        }

        #endregion
    }
}