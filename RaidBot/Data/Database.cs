namespace T.Data
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using T.Diagnostics;

    using Newtonsoft.Json;

    class Database
    {
        private const string PokemonFileName = "pokemon.json";

        private static readonly IEventLogger _logger = EventLogger.GetLogger();

        #region Singleton

        private static Database _instance;
        public static Database Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new Database();
                }

                return _instance;
            }
        }

        #endregion

        public Dictionary<int, string> Pokemon { get; }

        public Database()
        {
            if (!File.Exists(PokemonFileName))
            {
                throw new FileNotFoundException($"{PokemonFileName} file not found.", PokemonFileName);
            }

            var data = File.ReadAllText(PokemonFileName);
            if (data == null)
            {
                _logger.Error("Pokemon database is null.");
            }
            else
            {
                Pokemon = JsonConvert.DeserializeObject<Dictionary<int, string>>(data);
            }
        }

        public int PokemonIdFromName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return 0;

            var pkmn = Pokemon.FirstOrDefault(x => string.Compare(x.Value, name, true) == 0);

            if (pkmn.Key > 0)
                return pkmn.Key;

            foreach (var p in Pokemon)
                if (p.Value.ToLower().Contains(name.ToLower()))
                    return p.Key;

            if (!int.TryParse(name, out var pokeId))
                return 0;

            if (Pokemon.ContainsKey(pokeId))
                return pokeId;

            return 0;
        }
    }
}