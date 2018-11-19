namespace T.Ocr
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;

    using DSharpPlus;
    using DSharpPlus.Entities;
    using DSharpPlus.Interactivity;

    using Newtonsoft.Json;

    using T.Configuration;
    using T.Data;
    using T.Diagnostics;
    using T.Extensions;

    public class OcrProcessor : QueueProcessor<DiscordMessage>
    {
        #region Variables

        private static readonly IEventLogger _logger = EventLogger.GetLogger();
        private readonly DiscordClient _client;
        private readonly Config _config;
        private readonly IOcrService _ocrService;

        #endregion

        #region Constructor

        public OcrProcessor(DiscordClient client, Config config)
        {
            _client = client;
            _config = config;
            _ocrService = new OcrService
            {
                SaveDebugImages = config.Debug
            };
        }

        #endregion

        #region Protected Overrides

        protected override async Task<bool> ProcessEvent(DiscordMessage item)
        {
            _logger.Trace($"OcrProcessor::ProcessEvent [Item={item.Id}]");

            await HandleOcrAsync(item);

            return true;
        }

        protected override void QueueLengthChanged(int length)
        {
            _logger.Trace($"OcrProcessor::QueueLengthChanged [Length={length}]");
        }

        public override void Dispose()
        {
            _client?.Dispose();
        }

        #endregion

        #region Ocr Handlers

        private async Task HandleOcrAsync(DiscordMessage message)
        {
            _logger.Trace($"OcrProcessor::HandleOcrAsync [Message={message.Id}]");

            using (var httpClient = new HttpClient())
            {
                foreach (var attachment in message.Attachments)
                {
                    if (!await httpClient.IsImageUrlAsync(attachment.Url))
                        continue;

                    var tempImageFile = string.Empty;
                    try
                    {
                        tempImageFile = Path.GetTempFileName() + "." + attachment.Url.Split('.').Last();
                        await httpClient.DownloadAsync(new Uri(attachment.Url), tempImageFile);

                        var response = await _ocrService.AddRaidAsync(tempImageFile, false);
                        if (response == null || !response.IsSuccess)// || !response.IsRaidImage)
                        {
                            _logger.Error($"Failed to parse ocr, invalid raid image.");
                            continue;
                        }

                        if (string.IsNullOrEmpty(response.Gym))
                        {
                            await message.RespondAsync($"{message.Author.Mention} Failed to parse gym name '{response.Gym}'.");
                            //TODO: Ask for gym name.
                            continue;
                        }

                        if (response.IsRaidBoss)
                        {
                            var pokeId = FindPokemon(response.Pokemon);
                            if (pokeId == 0)
                            {
                                pokeId = await AskForPokemon(message);
                                if (pokeId == 0)
                                {
                                    pokeId = _config.DefaultRaidBoss;
                                    await message.RespondAsync($"{message.Author.Mention} Failed to parse raid boss name, assuming {Database.Instance.Pokemon[pokeId]}.");
                                }
                            }

                            if (await HandleRaidOcr(response, message, pokeId))
                            {
                                var pokemonName = Database.Instance.Pokemon[pokeId];
                                await message.RespondAsync($"{message.Author.Mention} Reported {pokemonName} raid at {response.Gym} that's ending in {response.RaidTimer.ToReadableString()}.");
                                //(Different plus total left = 45) - DateTime.Now
                                //var now = DateTime.Now;
                                //var endTime = now.AddMinutes(response.RaidTimer.Minutes);
                                //var startTime = endTime.Subtract(new TimeSpan(0, 45, 0));
                                //var spawnTime = endTime.Subtract(new TimeSpan(1, 45, 0));
                            }
                        }
                        else
                        {
                            var level = response.EggLevel;
                            if (level == 0)
                            {
                                level = await AskForEggLevel(message);
                                if (level == 0)
                                {
                                    level = _config.DefaultRaidLevel;
                                    await message.RespondAsync($"{message.Author.Mention} Failed to parse egg level, assuming level 5.");
                                }
                            }

                            if (await HandleEggOcr(response, message, level))
                            {
                                await message.RespondAsync($"{message.Author.Mention} Reported level {response.EggLevel} raid egg at {response.Gym} that's starting in {response.EggTimer.ToReadableString()}.");
                            }
                        }

                        //await message.DeleteAsync();
                    }
                    catch (Exception ex)
                    {
                        await message.RespondAsync($"{message.Author.Mention} Error: {ex.ToString()}");

                        var dir = Path.Combine(Directory.GetCurrentDirectory(), Strings.FailedOcrFolder);
                        if (!Directory.Exists(dir))
                        {
                            Directory.CreateDirectory(dir);
                        }

                        File.Copy(tempImageFile, Path.Combine(dir, Path.GetFileName(tempImageFile)));
                    }
                }
            }
        }

        private async Task<bool> HandleEggOcr(RaidOcrResult ocrEgg, DiscordMessage message, int level)
        {
            _logger.Trace($"OcrProcessor::HandleEggOcr [OcrEgg={ocrEgg.EggLevel}, Message={message.Id}, Gym={ocrEgg.Gym}, Level={level}]");

            var starts = DateTime.Now.Subtract(ocrEgg.EggTimer);
            var eb = new DiscordEmbedBuilder
            {
                Title = ocrEgg.Gym,
                Color = DiscordColor.Yellow, //TODO: Get color from raid level.
                ThumbnailUrl = string.Format(Strings.EggImage, level),
                //ImageUrl = string.Format(Strings.GoogleMapsStaticImage, gym.Latitude, gym.Longitude),
                //Url = string.Format(Strings.GoogleMaps, gym.Latitude, gym.Longitude),
                Description =
                    $"A level {ocrEgg.EggLevel} egg will hatch **{starts.ToLongTimeString()} ({starts.GetTimeRemaining().ToReadableStringNoSeconds()})**\r\n" +
                    //$"{gym.Details.Description}\r\n" +
                    //$"```{raid.Gym.Latitude},{raid.Gym.Longitude}```\r\n\r\n" +
                    //$"**Address:** " + (!string.IsNullOrEmpty(loc.Address) ? loc.Address : $"{raid.Gym.Latitude}, {raid.Gym.Longitude}") + "\r\n\r\n" +
                    $"**Submitted by:** {message.Author.Username}"
            };
            eb.Author = new DiscordEmbedBuilder.EmbedAuthor
            {
                Name = $"Level {level} Egg",
                IconUrl = string.Format(Strings.EggImage, level)
            };
            eb.Footer = new DiscordEmbedBuilder.EmbedFooter
            {
                Text = $"versx | {DateTime.Now}",
                IconUrl = message?.Channel?.Guild?.IconUrl ?? string.Empty
            };

            try
            {
                var whObj = GetWebHookData(_config.Webhook);
                var wh = await _client.GetWebhookWithTokenAsync(whObj.Id, whObj.Token);
                await wh.ExecuteAsync(string.Empty, _client.CurrentUser.Username, _client.CurrentUser.AvatarUrl ?? _client.CurrentUser.DefaultAvatarUrl, false, new DiscordEmbed[] { eb });

                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }

            return false;
        }

        private async Task<bool> HandleRaidOcr(RaidOcrResult ocrRaid, DiscordMessage message, int pokeId)
        {
            _logger.Trace($"OcrProcessor::HandleRaidOcr [OcrRaid={ocrRaid.Pokemon}, Message={message.Id}, Gym={ocrRaid.Gym}, PokemonId={pokeId}]");

            var db = Database.Instance;
            if (!db.Pokemon.ContainsKey(pokeId))
            {
                _logger.Error($"Failed to lookup Raid Pokemon '{pokeId}' in database.");
                return false;
            }

            var pkmn = db.Pokemon[pokeId];

            //var types = pkmn.Types.Count > 1 ? pkmn.Types[0].Type + "/" + pkmn.Types[1].Type : pkmn.Types[0].Type;
            //var typesText = $"**Types:** {string.Join("/", types)}\r\n";

            //var weaknesses = new List<string>();
            //foreach (var type in types.Split('/'))
            //{
            //    foreach (var weakness in PokemonExtensions.GetWeaknesses(type))
            //    {
            //        var emojiId = message.Channel.Guild.GetEmojiId($"types_{weakness.ToLower()}");
            //        var emojiName = emojiId > 0 ? $"<:{weakness.ToLower()}:{emojiId}>" : weakness;
            //        if (!weaknesses.Contains(emojiName))
            //        {
            //            weaknesses.Add(emojiName);
            //        }
            //    }
            //}

            //var counters = string.Empty;
            //if (weaknesses.Count > 0)
            //{
            //    counters += $"**Weaknesses:** {string.Join(" ", weaknesses)}\r\n";
            //}

            //var perfectRange = db.GetPokemonCpRange(pokeId, 20);
            //var boostedRange = db.GetPokemonCpRange(pokeId, 25);
            var ends = DateTime.Now.Subtract(ocrRaid.RaidTimer);
            var eb = new DiscordEmbedBuilder
            {
                Title = ocrRaid.Gym,
                Color = DiscordColor.Red, //TODO: Get color from raid level
                ThumbnailUrl = string.Format(Strings.PokemonImage, pokeId, 0),
            //    //ImageUrl = string.Format(Strings.GoogleMapsStaticImage, raid.Gym.Latitude, raid.Gym.Longitude),
            //    //Url = string.Format(Strings.GoogleMaps, raid.Gym.Latitude, raid.Gym.Longitude),
                Description =
                    $"**Ends:** {ends.ToLongTimeString()} ({ends.GetTimeRemaining().ToReadableStringNoSeconds()})\r\n" +
                    //$"**Perfect CP:** {perfectRange.Best} / :white_sun_rain_cloud: {boostedRange.Best}\r\n" +
                    //counters +
                    //$"{raid.Gym.Details.Description}\r\n" +
                    //$"```{raid.Gym.Latitude},{raid.Gym.Longitude}```\r\n\r\n" +
                    //$"**Address:** " + (!string.IsNullOrEmpty(loc.Address) ? loc.Address : $"{raid.Gym.Latitude}, {raid.Gym.Longitude}") + "\r\n\r\n" +
                    $"**Submitted by:** {message.Author.Username}"
            };
            eb.Author = new DiscordEmbedBuilder.EmbedAuthor
            {
                Name = $"{pkmn} Raid",
                IconUrl = string.Format(Strings.PokemonImage, pokeId, 0)
            };
            eb.Footer = new DiscordEmbedBuilder.EmbedFooter
            {
                Text = $"versx | {DateTime.Now}",
                IconUrl = message?.Channel?.Guild?.IconUrl ?? string.Empty
            };

            try
            {
                var whObj = GetWebHookData(_config.Webhook);
                var wh = await _client.GetWebhookWithTokenAsync(whObj.Id, whObj.Token);
                await wh.ExecuteAsync(string.Empty, _client.CurrentUser.Username, _client.CurrentUser.AvatarUrl ?? _client.CurrentUser.DefaultAvatarUrl, false, new DiscordEmbed[] { eb });

                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }

            return false;
        }

        #endregion

        #region Interactive Helpers

        private int FindPokemon(string pokemon)
        {
            _logger.Debug($"Name Before: {pokemon}");

            var db = Database.Instance;
            var pkmn = db.Pokemon.FirstOrDefault(x => string.Compare(x.Value, pokemon, true) == 0).Value;
            if (pkmn == null)
            {
                pkmn = db.Pokemon.FirstOrDefault(x => LevenshteinDistance.Compute(x.Value, pokemon) <= 2).Value;
            }

            _logger.Debug($"Name After: {pkmn}");

            var pokeId = db.PokemonIdFromName(pkmn ?? string.Empty);
            return pokeId;
        }

        private async Task<int> AskForPokemon(DiscordMessage message)
        {
            _logger.Trace($"OcrProcessor::AskForPokemon [Message={message.Id}]");

            await message.RespondAsync($"{message.Author.Mention} Failed to parse Pokemon name, please enter the name or id manually below:");
            var pokeId = 0;

            var interactivity = _client.GetInteractivityModule();
            var msg = await interactivity.WaitForMessageAsync(x => !string.IsNullOrEmpty(x.Content) /*&& Db.IsValidRaidBoss(Database.Instance.PokemonIdFromName(x.Content))*/ && x.Author.Id == message.Author.Id, TimeSpan.FromSeconds(90));
            if (msg != null)
            {
                pokeId = Database.Instance.PokemonIdFromName(msg.Message.Content);
                if (pokeId == 0)
                {
                    await message.RespondAsync($"{message.Author.Mention} Failed to find Pokemon by name or id, raid was not submitted.");
                }
            }

            return pokeId;
        }

        private async Task<int> AskForEggLevel(DiscordMessage message)
        {
            _logger.Trace($"OcrProcessor::AskForEggLevel [Message={message.Id}]");

            await message.RespondAsync($"{message.Author.Mention} Failed to find level for raid egg, enter it manually below: (1-5)");
            var level = 0;
            var interactivity = _client.GetInteractivityModule();
            var msg = await interactivity.WaitForMessageAsync(x => !string.IsNullOrEmpty(x.Content) && Convert.ToInt32(x.Content) > 0 && Convert.ToInt32(x.Content) < 6 && x.Author.Id == message.Author.Id);
            if (msg != null)
            {
                if (!int.TryParse(msg.Message.Content, out level))
                {
                    await message.RespondAsync($"{message.Author.Mention} You've entered an invalid value for the raid egg level, raid egg was not submitted.");
                }
            }

            return level;
        }

        #endregion

        public static WebHookObject GetWebHookData(string webHook)
        {
            /**Example:
             * {
             *   "name": "", 
             *   "channel_id": "", 
             *   "token": "", 
             *   "avatar": null, 
             *   "guild_id": "", 
             *   "id": ""
             * }
             */

            using (var wc = new WebClient())
            {
                wc.Proxy = null;
                var json = wc.DownloadString(webHook);
                var data = JsonConvert.DeserializeObject<WebHookObject>(json);
                return data;
            }
        }
    }

    public class WebHookObject
    {
        [JsonProperty("id")]
        public ulong Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("guild_id")]
        public ulong GuildId { get; set; }

        [JsonProperty("channel_id")]
        public ulong ChannelId { get; set; }

        [JsonProperty("token")]
        public string Token { get; set; }

        [JsonProperty("avatar")]
        public string Avatar { get; set; }
    }
}