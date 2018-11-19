namespace T
{
    using System;
    using System.Threading.Tasks;

    using DSharpPlus;
    using DSharpPlus.Entities;
    using DSharpPlus.EventArgs;

    using T.Configuration;
    using T.Diagnostics;
    using T.Ocr;

    class Bot
    {
        #region Variables

        private static readonly IEventLogger _logger = EventLogger.GetLogger();

        private readonly DiscordClient _client;
        private readonly OcrProcessor _ocrProcessor;
        private readonly Config _config;

        #endregion

        #region Constructor

        public Bot()
        {
            _config = Config.Load(Strings.ConfigFileName);
            if (_config == null)
            {
                _logger.Error($"Failed to load config file.");
                return;
            }

            _client = new DiscordClient(new DiscordConfiguration
            {
                AutomaticGuildSync = true,
                AutoReconnect = true,
                EnableCompression = true,
                Token = _config.Token,
                TokenType = TokenType.Bot,
                UseInternalLogHandler = true
            });
            _client.Ready += Client_Ready;
            _client.MessageCreated += Client_MessageCreated;

            _ocrProcessor = new OcrProcessor(_client, _config);
        }

        #endregion

        #region Discord Events

        private async Task Client_Ready(ReadyEventArgs e)
        {
            _logger.Trace($"Bot::Client_Ready [{e.Client.CurrentUser.Username}]");

            //var version = AssemblyUtils.AssemblyVersion;
            //var cleanedVersion = version.Substring(0, version.LastIndexOf('.'));
            await _client.UpdateStatusAsync(new DiscordGame($"Reporting raids..."));

            foreach (var user in _client.Presences)
            {
                _logger.Info($"User: {user.Key}: {user.Value.User.Username}");
            }
        }

        private async Task Client_MessageCreated(MessageCreateEventArgs e)
        {
            if (e.Message.Attachments.Count > 0 && 
                _config.ParseChannelId == e.Channel.Id && 
                _config.Enabled)
            {
                _ocrProcessor.Add(e.Message);
            }

            await Task.CompletedTask;
        }

        #endregion

        #region Public Methods

        public async Task Start()
        {
            _logger.Trace($"Bot::Start");

            await _client.ConnectAsync();
        }

        public async Task Stop()
        {
            _logger.Trace($"Bot::Stop");

            await _client.DisconnectAsync();
        }

        #endregion
    }
}