namespace T.Configuration
{
    using System;
    using System.IO;

    using Newtonsoft.Json;

    public class Config
    {
        [JsonProperty("enabled")]
        public bool Enabled { get; set; }

        [JsonProperty("token")]
        public string Token { get; set; }

        [JsonProperty("parseChannelId")]
        public ulong ParseChannelId { get; set; }

        [JsonProperty("reportChannelId")]
        public ulong ReportChannelId { get; set; }

        [JsonProperty("webhook")]
        public string Webhook { get; set; }

        [JsonProperty("defaultRaidBoss")]
        public int DefaultRaidBoss { get; set; }

        [JsonProperty("defaultRaidLevel")]
        public int DefaultRaidLevel { get; set; }

        [JsonProperty("debug")]
        public bool Debug { get; set; }

        public Config()
        {
            Enabled = true;
        }

        public void Save(string filePath)
        {
            var data = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(filePath, data);
        }

        public static Config Load(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("Config not loaded because file not found.", filePath);
            }

            var data = File.ReadAllText(filePath);
            return JsonConvert.DeserializeObject<Config>(data);
        }
    }
}