using ConVar;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Oxide.Core;
using Oxide.Core.Libraries;
using Oxide.Core.Libraries.Covalence;
using Oxide.Core.Plugins;
using Oxide.Ext.Discord.Attributes.Pooling;
using Oxide.Ext.Discord.Clients;
using Oxide.Ext.Discord.Connections;
using Oxide.Ext.Discord.Constants;
using Oxide.Ext.Discord.Entities;
using Oxide.Ext.Discord.Entities.Applications;
using Oxide.Ext.Discord.Entities.Channels;
using Oxide.Ext.Discord.Entities.Gateway;
using Oxide.Ext.Discord.Entities.Gateway.Events;
using Oxide.Ext.Discord.Entities.Guilds;
using Oxide.Ext.Discord.Entities.Messages;
using Oxide.Ext.Discord.Entities.Permissions;
using Oxide.Ext.Discord.Entities.Users;
using Oxide.Ext.Discord.Extensions;
using Oxide.Ext.Discord.Interfaces;
using Oxide.Ext.Discord.Libraries.Placeholders;
using Oxide.Ext.Discord.Libraries.Subscription;
using Oxide.Ext.Discord.Libraries.Templates;
using Oxide.Ext.Discord.Libraries.Templates.Embeds;
using Oxide.Ext.Discord.Libraries.Templates.Messages;
using Oxide.Ext.Discord.Logging;
using Oxide.Ext.Discord.Pooling;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

//DiscordChat created with PluginMerge v(1.0.5.0) by MJSU @ https://github.com/dassjosh/Plugin.Merge
namespace Oxide.Plugins
{
    [Info("Discord Chat", "MJSU", "3.0.0")]
    [Description("Allows chatting between discord and game server")]
    public partial class DiscordChat : CovalencePlugin, IDiscordPlugin
    {
        #region Plugins\DiscordChat.AdminChat.cs
        private AdminChatSettings _adminChatSettings;
        
        private const string AdminChatPermission = "adminchat.use";
        
        //Hook called from AdminChat Plugin
        [HookMethod(nameof(OnAdminChat))]
        public void OnAdminChat(IPlayer player, string message)
        {
            if (IsAdminChatEnabled())
            {
                HandleMessage(message, player, player.GetDiscordUser(), MessageSource.AdminChat, null);
            }
        }
        
        //Message sent in admin chat channel. Process bot replace and sending to server
        public void HandleAdminChatDiscordMessage(DiscordMessage message)
        {
            IPlayer player = message.Author.Player;
            if (player == null)
            {
                message.ReplyWithGlobalTemplate(Client, TemplateKeys.Error.AdminChat.NotLinked, null, GetDefault().AddMessage(message));
                return;
            }
            
            if (!CanPlayerAdminChat(player))
            {
                message.ReplyWithGlobalTemplate(Client, TemplateKeys.Error.AdminChat.NoPermission, null, GetDefault().AddPlayer(player).AddMessage(message));
                return;
            }
            
            HandleMessage(message.Content, player, player.GetDiscordUser(), MessageSource.AdminChat, message);
        }
        
        public bool IsAdminChatEnabled() => _adminChatSettings.Enabled && Sends.ContainsKey(MessageSource.AdminChat);
        public bool CanPlayerAdminChat(IPlayer player) => player != null && _adminChatSettings.Enabled && player.HasPermission(AdminChatPermission);
        #endregion

        #region Plugins\DiscordChat.BetterChat.cs
        public bool SendBetterChatMessage(IPlayer player, string message)
        {
            if (IsPluginLoaded(BetterChat))
            {
                Dictionary<string, object> data = BetterChat.Call<Dictionary<string, object>>("API_GetMessageData", player, message);
                BetterChat.Call("API_SendMessage", data);
                return true;
            }
            
            return false;
        }
        
        // public string GetBetterChatConsoleMessage(IPlayer player, string message)
        // {
            //     return BetterChat.Call<string>("API_GetFormattedMessage", player, message, _true);
        // }
        #endregion

        #region Plugins\DiscordChat.Clans.cs
        private void OnClanChat(IPlayer player, string message)
        {
            Sends[MessageSource.Clan]?.QueueMessage(Lang(LangKeys.Discord.Clans.ClanMessage, GetClanPlaceholders(player, message)));
        }
        
        private void OnAllianceChat(IPlayer player, string message)
        {
            Sends[MessageSource.Alliance]?.QueueMessage(Lang(LangKeys.Discord.Clans.AllianceMessage, GetClanPlaceholders(player, message)));
        }
        
        public PlaceholderData GetClanPlaceholders(IPlayer player, string message)
        {
            return GetDefault().AddPlayer(player).Add(PlaceholderKeys.Data.PlayerMessage, message);
        }
        #endregion

        #region Plugins\DiscordChat.DiscordHooks.cs
        [HookMethod(DiscordExtHooks.OnDiscordClientCreated)]
        private void OnDiscordClientCreated()
        {
            if (!string.IsNullOrEmpty(_pluginConfig.DiscordApiKey))
            {
                RegisterPlaceholders();
                RegisterTemplates();
                Client.Connect(_discordSettings);
            }
        }
        
        [HookMethod(DiscordExtHooks.OnDiscordGatewayReady)]
        private void OnDiscordGatewayReady(GatewayReadyEvent ready)
        {
            if (ready.Guilds.Count == 0)
            {
                PrintError("Your bot was not found in any discord servers. Please invite it to a server and reload the plugin.");
                return;
            }
            
            DiscordApplication app = Client.Bot.Application;
            if (!app.HasApplicationFlag(ApplicationFlags.GatewayMessageContentLimited))
            {
                PrintWarning($"You will need to enable \"Message Content Intent\" for {Client.Bot.BotUser.Username} @ https://discord.com/developers/applications\n by April 2022" +
                $"{Name} will stop function correctly after that date until that is fixed. Once updated please reload {Name}.");
            }
            
            Puts($"{Title} Ready");
        }
        
        [HookMethod(DiscordExtHooks.OnDiscordGuildCreated)]
        private void OnDiscordGuildCreated(DiscordGuild guild)
        {
            if (_pluginConfig.ChatSettings.DiscordToServer)
            {
                SetupChannel(guild, MessageSource.Server, _pluginConfig.ChatSettings.ChatChannel, _pluginConfig.ChatSettings.UseBotToDisplayChat, HandleDiscordChatMessage);
            }
            else
            {
                SetupChannel(guild, MessageSource.Server, _pluginConfig.ChatSettings.ChatChannel, _pluginConfig.ChatSettings.UseBotToDisplayChat);
            }
            
            SetupChannel(guild, MessageSource.Discord, _pluginConfig.ChatSettings.ChatChannel, _pluginConfig.ChatSettings.UseBotToDisplayChat);
            
            SetupChannel(guild, MessageSource.Connecting, _pluginConfig.PlayerStateSettings.PlayerStateChannel, false);
            SetupChannel(guild, MessageSource.Connected, _pluginConfig.PlayerStateSettings.PlayerStateChannel, false);
            SetupChannel(guild, MessageSource.Disconnected, _pluginConfig.PlayerStateSettings.PlayerStateChannel, false);
            SetupChannel(guild, MessageSource.ServerBooting, _pluginConfig.ServerStateSettings.ServerStateChannel, false);
            SetupChannel(guild, MessageSource.ServerOnline, _pluginConfig.ServerStateSettings.ServerStateChannel, false);
            SetupChannel(guild, MessageSource.ServerShutdown, _pluginConfig.ServerStateSettings.ServerStateChannel, false);
            SetupChannel(guild, MessageSource.AdminChat, _pluginConfig.PluginSupport.AdminChat.ChatChannel, _pluginConfig.ChatSettings.UseBotToDisplayChat, HandleAdminChatDiscordMessage);
            SetupChannel(guild, MessageSource.Clan, _pluginConfig.PluginSupport.Clans.ClansChatChannel, _pluginConfig.ChatSettings.UseBotToDisplayChat);
            SetupChannel(guild, MessageSource.Alliance, _pluginConfig.PluginSupport.Clans.AllianceChatChannel, _pluginConfig.ChatSettings.UseBotToDisplayChat);
            
            #if RUST
            SetupChannel(guild, MessageSource.Team, _pluginConfig.ChatSettings.TeamChannel, false);
            SetupChannel(guild, MessageSource.Cards, _pluginConfig.ChatSettings.CardsChannel, false);
            #endif
            
            if (_pluginConfig.ChatSettings.ChatChannel.IsValid()
            #if RUST
            || _pluginConfig.ChatSettings.TeamChannel.IsValid()
            || _pluginConfig.ChatSettings.CardsChannel.IsValid()
            #endif
            )
            {
                #if RUST
                Subscribe(nameof(OnPlayerChat));
                #else
                Subscribe(nameof(OnUserChat));
                #endif
            }
            
            if (_pluginConfig.PlayerStateSettings.PlayerStateChannel.IsValid())
            {
                Subscribe(nameof(OnUserConnected));
                Subscribe(nameof(OnUserDisconnected));
            }
            
            if (_pluginConfig.ServerStateSettings.ServerStateChannel.IsValid())
            {
                Subscribe(nameof(OnServerShutdown));
            }
            
            if (_pluginConfig.PluginSupport.Clans.ClansChatChannel.IsValid())
            {
                Subscribe(nameof(OnClanChat));
            }
            
            if (_pluginConfig.PluginSupport.Clans.AllianceChatChannel.IsValid())
            {
                Subscribe(nameof(OnAllianceChat));
            }
            
            timer.In(1f, () =>
            {
                if (!_serverInitCalled)
                {
                    Sends[MessageSource.ServerBooting]?.SendTemplate(TemplateKeys.Server.Booting, GetDefault());
                }
            });
        }
        
        public void SetupChannel(DiscordGuild guild, MessageSource source, Snowflake id, bool wipeNonBotMessages = false, Action<DiscordMessage> callback = null)
        {
            if (!id.IsValid())
            {
                return;
            }
            
            DiscordChannel channel = guild.Channels[id];
            if (channel == null)
            {
                //PrintWarning($"Channel with ID: '{id}' not found in guild");
                return;
            }
            
            if (callback != null)
            {
                _subscriptions.AddChannelSubscription(Client, id, callback);
            }
            
            if (wipeNonBotMessages)
            {
                channel.GetMessages(Client, new ChannelMessagesRequest{Limit = 100}).Then(messages => OnGetChannelMessages(messages, channel));
            }
            
            Sends[source] = new DiscordSendQueue(channel, GetTemplateName(source), timer);;
            Puts($"Setup Channel {source} With ID: {id}");
        }
        
        private void OnGetChannelMessages(List<DiscordMessage> messages, DiscordChannel channel)
        {
            if (messages.Count == 0)
            {
                return;
            }
            
            Snowflake[] messagesToDelete = messages
            .Where(m => !m.Author.IsBot && !CanSendMessage(m.Content, m.Author.Player, m.Author, MessageSource.Server, m))
            .Take(100).Select(m => m.Id)
            .ToArray();
            
            if (messagesToDelete.Length == 0)
            {
                return;
            }
            
            if (messagesToDelete.Length == 1)
            {
                new DiscordMessage { Id = messagesToDelete[0] }.Delete(Client);
                return;
            }
            
            channel.BulkDeleteMessages(Client, messagesToDelete);
        }
        
        public void HandleDiscordChatMessage(DiscordMessage message)
        {
            IPlayer player = message.Author.Player;
            if (Interface.Oxide.CallHook("OnDiscordChatMessage", player, message.Content, message.Author) != null)
            {
                return;
            }
            
            HandleMessage(message.Content, player, message.Author, MessageSource.Discord, message);
            
            if (_pluginConfig.ChatSettings.UseBotToDisplayChat)
            {
                message.Delete(Client);
            }
        }
        #endregion

        #region Plugins\DiscordChat.Fields.cs
        [PluginReference]
        private Plugin BetterChat;
        
        public DiscordClient Client { get; set; }
        
        private PluginConfig _pluginConfig;
        
        private readonly BotConnection _discordSettings = new BotConnection
        {
            Intents = GatewayIntents.Guilds | GatewayIntents.GuildMembers | GatewayIntents.GuildMessages | GatewayIntents.MessageContent
        };
        
        private readonly DiscordSubscriptions _subscriptions = GetLibrary<DiscordSubscriptions>();
        private readonly DiscordPlaceholders _placeholders = GetLibrary<DiscordPlaceholders>();
        private readonly DiscordMessageTemplates _templates = GetLibrary<DiscordMessageTemplates>();
        
        [DiscordPool]
        private DiscordPluginPool _pool;
        
        private bool _serverInitCalled;
        
        public readonly Hash<MessageSource, DiscordSendQueue> Sends = new Hash<MessageSource, DiscordSendQueue>();
        private readonly List<IPluginHandler> _plugins = new List<IPluginHandler>();
        
        public static DiscordChat Instance;
        
        private readonly object _true = true;
        #endregion

        #region Plugins\DiscordChat.Helpers.cs
        public MessageSource GetSourceFromServerChannel(int channel)
        {
            switch (channel)
            {
                case 1:
                return MessageSource.Team;
                case 3:
                return MessageSource.Cards;
            }
            
            return MessageSource.Server;
        }
        
        public bool IsPluginLoaded(Plugin plugin) => plugin != null && plugin.IsLoaded;
        
        public string GetBetterChatTag(IPlayer player)
        {
            return player.IsConnected ? null : _pluginConfig.ChatSettings.DiscordTag;
        }
        
        public new void Subscribe(string hook)
        {
            base.Subscribe(hook);
        }
        
        public new void Unsubscribe(string hook)
        {
            base.Unsubscribe(hook);
        }
        
        public void Puts(string message)
        {
            base.Puts(message);
        }
        #endregion

        #region Plugins\DiscordChat.Hooks.cs
        private void OnUserApproved(string name, string id, string ip)
        {
            IPlayer player = players.FindPlayerById(id) ?? PlayerExt.CreateDummyPlayer(id, name, ip);
            if (_pluginConfig.PlayerStateSettings.ShowAdmins || !player.IsAdmin)
            {
                PlaceholderData placeholders = GetDefault().AddPlayer(player);
                ProcessPlayerState(MessageSource.Connecting, LangKeys.Discord.Player.Connecting, placeholders);
            }
        }
        
        private void OnUserConnected(IPlayer player)
        {
            if (_pluginConfig.PlayerStateSettings.ShowAdmins || !player.IsAdmin)
            {
                PlaceholderData placeholders = GetDefault().AddPlayer(player);
                ProcessPlayerState(MessageSource.Connected, LangKeys.Discord.Player.Connected, placeholders);
            }
        }
        
        private void OnUserDisconnected(IPlayer player, string reason)
        {
            if (_pluginConfig.PlayerStateSettings.ShowAdmins || !player.IsAdmin)
            {
                PlaceholderData placeholders = GetDefault().AddPlayer(player).Add(PlaceholderKeys.Data.DisconnectReason, reason);
                ProcessPlayerState(MessageSource.Disconnected, LangKeys.Discord.Player.Disconnected, placeholders);
            }
        }
        
        public void ProcessPlayerState(MessageSource source, string langKey, PlaceholderData data)
        {
            string message = Lang(langKey, data);
            Sends[source]?.QueueMessage(message);
        }
        
        private void OnPluginLoaded(Plugin plugin)
        {
            if (plugin == null)
            {
                return;
            }
            
            OnPluginUnloaded(plugin);
            
            switch (plugin.Name)
            {
                case "AdminChat":
                AddHandler(new AdminChatHandler(Client, this, _pluginConfig.PluginSupport.AdminChat, plugin));
                break;
                
                case "AdminDeepCover":
                AddHandler(new AdminDeepCoverHandler(this, plugin));
                break;
                
                case "AntiSpam":
                if (plugin.Version < new VersionNumber(2, 0, 0))
                {
                    PrintError("AntiSpam plugin must be version 2.0.0 or higher");
                    break;
                }
                
                AddHandler(new AntiSpamHandler(this, _pluginConfig.PluginSupport.AntiSpam, plugin));
                break;
                
                case "BetterChatMute":
                BetterChatMuteSettings muteSettings = _pluginConfig.PluginSupport.BetterChatMute;
                if (muteSettings.IgnoreMuted)
                {
                    AddHandler(new BetterChatMuteHandler(this, muteSettings, plugin));
                }
                break;
                
                // case "Clans":
                //     AddHandler(new ClansHandler(this, _pluginConfig.PluginSupport.Clans, plugin));
                //     break;
                
                case "TranslationAPI":
                AddHandler(new TranslationApiHandler(this, _pluginConfig.PluginSupport.ChatTranslator, plugin));
                break;
                
                case "UFilter":
                AddHandler(new UFilterHandler(this, _pluginConfig.PluginSupport.UFilter, plugin));
                break;
            }
        }
        
        public void AddHandler(IPluginHandler handler)
        {
            _plugins.Insert(_plugins.Count - 1, handler);
        }
        
        private void OnPluginUnloaded(Plugin plugin)
        {
            if (plugin.Name != Name)
            {
                _plugins.RemoveAll(h => h.GetPluginName() == plugin.Name);
            }
        }
        
        #if RUST
        private void OnPlayerChat(BasePlayer rustPlayer, string message, Chat.ChatChannel chatChannel)
        {
            HandleChat(rustPlayer.IPlayer, message, (int)chatChannel);
        }
        #else
        private void OnUserChat(IPlayer player, string message)
        {
            HandleChat(player, message, 0);
        }
        #endif
        
        public void HandleChat(IPlayer player, string message, int channel)
        {
            DiscordUser user = player.GetDiscordUser();
            MessageSource source = GetSourceFromServerChannel(channel);
            
            if (!Sends.ContainsKey(source))
            {
                return;
            }
            
            HandleMessage(message, player, user, source, null);
        }
        #endregion

        #region Plugins\DiscordChat.Lang.cs
        public string Lang(string key, PlaceholderData data)
        {
            string message = lang.GetMessage(key, this);
            message = _placeholders.ProcessPlaceholders(message, data);
            return message;
        }
        
        protected override void LoadDefaultMessages()
        {
            lang.RegisterMessages(new Dictionary<string, string>
            {
                [LangKeys.Discord.Player.Connecting] = $":yellow_circle: {{timestamp.now.shorttime}} **{{{PlaceholderKeys.PlayerName}}}** is connecting",
                [LangKeys.Discord.Player.Connected] = $":white_check_mark: {{timestamp.now.shorttime}} **{{{PlaceholderKeys.PlayerName}}}** {{player.country.emoji}} has joined.",
                [LangKeys.Discord.Player.Disconnected] = $":x: {{timestamp.now.shorttime}} **{{{PlaceholderKeys.PlayerName}}}** has disconnected. ({{{PlaceholderKeys.DisconnectReason}}})",
                [LangKeys.Discord.Chat.Server] = $":desktop: {{timestamp.now.shorttime}} **{{{PlaceholderKeys.PlayerName}}}**: {{{PlaceholderKeys.PlayerMessage}}}",
                [LangKeys.Discord.Chat.LinkedMessage] = $":speech_left: {{timestamp.now.shorttime}} **{{{PlaceholderKeys.PlayerName}}}**: {{{PlaceholderKeys.PlayerMessage}}}",
                [LangKeys.Discord.Chat.UnlinkedMessage] = $":chains: {{timestamp.now.shorttime}} {{user.mention}}: {{{PlaceholderKeys.PlayerMessage}}}",
                [LangKeys.Discord.Chat.PlayerName] = "{player.name:clan}",
                [LangKeys.Discord.Team.Message] = $":busts_in_silhouette: {{timestamp.now.shorttime}} **{{{PlaceholderKeys.PlayerName}}}**: {{{PlaceholderKeys.PlayerMessage}}}",
                [LangKeys.Discord.Cards.Message] = $":black_joker: {{timestamp.now.shorttime}} **{{{PlaceholderKeys.PlayerName}}}**: {{{PlaceholderKeys.PlayerMessage}}}",
                [LangKeys.Discord.AdminChat.ServerMessage] = $":mechanic: {{timestamp.now.shorttime}} **{{{PlaceholderKeys.PlayerName}}}**: {{{PlaceholderKeys.PlayerMessage}}}",
                [LangKeys.Discord.AdminChat.DiscordMessage] = $":mechanic: {{timestamp.now.shorttime}} **{{{PlaceholderKeys.PlayerName}}}**: {{{PlaceholderKeys.PlayerMessage}}}",
                [LangKeys.Discord.Clans.ClanMessage] = $"{{timestamp.now.shorttime}} [Clan] **{{{PlaceholderKeys.PlayerName}}}**: {{{PlaceholderKeys.PlayerMessage}}}",
                [LangKeys.Discord.Clans.AllianceMessage] = $"{{timestamp.now.shorttime}} [Alliance] **{{{PlaceholderKeys.PlayerName}}}**: {{{PlaceholderKeys.PlayerMessage}}}",
                [LangKeys.Server.UnlinkedMessage] = $"{{{PlaceholderKeys.DiscordTag}}} [#5f79d6]{{user.fullname}}[/#]: {{{PlaceholderKeys.PlayerMessage}}}",
                [LangKeys.Server.LinkedMessage] = $"{{{PlaceholderKeys.DiscordTag}}} [#5f79d6]{{{PlaceholderKeys.PlayerName}}}[/#]: {{{PlaceholderKeys.PlayerMessage}}}"
            }, this);
        }
        #endregion

        #region Plugins\DiscordChat.MessageHandling.cs
        private readonly Regex _channelMention = new Regex(@"(<#\d+>)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        
        public void HandleMessage(string content, IPlayer player, DiscordUser user, MessageSource source, DiscordMessage sourceMessage)
        {
            if (!CanSendMessage(content, player, user, source, sourceMessage))
            {
                return;
            }
            
            ProcessCallbackMessages(content, player, user, source, processedMessage =>
            {
                StringBuilder messageBuilder = _pool.GetStringBuilder(processedMessage);
                
                if (sourceMessage != null)
                {
                    ProcessMentions(sourceMessage, messageBuilder);
                }
                
                ProcessMessage(messageBuilder, player, user, source);
                SendMessage(_pool.FreeStringBuilderToString(messageBuilder), player, user, source, sourceMessage);
            });
        }
        
        public void ProcessMentions(DiscordMessage message, StringBuilder sb)
        {
            DiscordGuild guild = Client.Bot.GetGuild(message.GuildId);
            if (message.Mentions != null)
            {
                foreach (KeyValuePair<Snowflake, DiscordUser> mention in message.Mentions)
                {
                    sb.Replace($"<@{mention.Key.ToString()}>", $"@{mention.Value.DisplayName}");
                }
                
                foreach (KeyValuePair<Snowflake, DiscordUser> mention in message.Mentions)
                {
                    GuildMember member = guild.Members[mention.Key];
                    sb.Replace($"<@!{mention.Key.ToString()}>", $"@{member?.Nickname ?? mention.Value.DisplayName}");
                }
            }
            
            if (message.MentionsChannels != null)
            {
                foreach (KeyValuePair<Snowflake, ChannelMention> mention in message.MentionsChannels)
                {
                    sb.Replace($"<#{mention.Key.ToString()}>", $"#{mention.Value.Name}");
                }
            }
            
            foreach (Match match in _channelMention.Matches(message.Content))
            {
                string value = match.Value;
                Snowflake id = new Snowflake(value.Substring(2, value.Length - 3));
                DiscordChannel channel = guild.Channels[id];
                if (channel != null)
                {
                    sb.Replace(value, $"#{channel.Name}");
                }
            }
            
            if (message.MentionRoles != null)
            {
                foreach (Snowflake roleId in message.MentionRoles)
                {
                    DiscordRole role = guild.Roles[roleId];
                    sb.Replace($"<@&{roleId.ToString()}>", $"@{role.Name ?? roleId}");
                }
            }
        }
        
        public bool CanSendMessage(string message, IPlayer player, DiscordUser user, MessageSource source, DiscordMessage sourceMessage)
        {
            for (int index = 0; index < _plugins.Count; index++)
            {
                if (!_plugins[index].CanSendMessage(message, player, user, source, sourceMessage))
                {
                    return false;
                }
            }
            
            return true;
        }
        
        public void ProcessCallbackMessages(string message, IPlayer player, DiscordUser user, MessageSource source, Action<string> completed, int index = 0)
        {
            for (; index < _plugins.Count; index++)
            {
                IPluginHandler handler = _plugins[index];
                if (handler.HasCallbackMessage())
                {
                    handler.ProcessCallbackMessage(message, player, user, source, callbackMessage =>
                    {
                        ProcessCallbackMessages(callbackMessage, player, user, source, completed, index + 1);
                    });
                    return;
                }
            }
            
            completed.Invoke(message);
        }
        
        public void ProcessMessage(StringBuilder message, IPlayer player, DiscordUser user, MessageSource source)
        {
            for (int index = 0; index < _plugins.Count; index++)
            {
                _plugins[index].ProcessMessage(message, player, user, source);
            }
        }
        
        public void SendMessage(string message, IPlayer player, DiscordUser user, MessageSource source, DiscordMessage sourceMessage)
        {
            for (int index = 0; index < _plugins.Count; index++)
            {
                IPluginHandler plugin = _plugins[index];
                if (plugin.SendMessage(message, player, user, source, sourceMessage))
                {
                    return;
                }
            }
        }
        #endregion

        #region Plugins\DiscordChat.Placeholders.cs
        public void RegisterPlaceholders()
        {
            _placeholders.RegisterPlaceholder<string>(this, PlaceholderKeys.Message, PlaceholderKeys.Data.Message);
            _placeholders.RegisterPlaceholder<string>(this, PlaceholderKeys.PlayerMessage, PlaceholderKeys.Data.PlayerMessage);
            _placeholders.RegisterPlaceholder<string>(this, PlaceholderKeys.DisconnectReason, PlaceholderKeys.Data.DisconnectReason);
            _placeholders.RegisterPlaceholder<IPlayer, string>(this, PlaceholderKeys.PlayerName, GetPlayerName);
            _placeholders.RegisterPlaceholder(this, PlaceholderKeys.DiscordTag, _pluginConfig.ChatSettings.DiscordTag);
        }
        
        public string GetPlayerName(IPlayer player)
        {
            string name = Lang(LangKeys.Discord.Chat.PlayerName, GetDefault().AddPlayer(player));
            StringBuilder sb = _pool.GetStringBuilder(name);
            for (int index = 0; index < _plugins.Count; index++)
            {
                _plugins[index].ProcessPlayerName(sb, player);
            }
            
            return _pool.FreeStringBuilderToString(sb);
        }
        
        public PlaceholderData GetDefault()
        {
            return _placeholders.CreateData(this);
        }
        #endregion

        #region Plugins\DiscordChat.Setup.cs
        private void Init()
        {
            Instance = this;
            
            _discordSettings.ApiToken = _pluginConfig.DiscordApiKey;
            _discordSettings.LogLevel = _pluginConfig.ExtensionDebugging;
            
            _adminChatSettings = _pluginConfig.PluginSupport.AdminChat;
            
            #if RUST
            Unsubscribe(nameof(OnPlayerChat));
            #else
            Unsubscribe(nameof(OnUserChat));
            #endif
            
            Unsubscribe(nameof(OnUserConnected));
            Unsubscribe(nameof(OnUserDisconnected));
            Unsubscribe(nameof(OnServerShutdown));
            Unsubscribe(nameof(OnClanChat));
            Unsubscribe(nameof(OnAllianceChat));
            
            _plugins.Add(new DiscordChatHandler(this, _pluginConfig.ChatSettings, this, server));
        }
        
        protected override void LoadDefaultConfig()
        {
            PrintWarning("Loading Default Config");
        }
        
        protected override void LoadConfig()
        {
            base.LoadConfig();
            _pluginConfig = AdditionalConfig(Config.ReadObject<PluginConfig>());
            Config.WriteObject(_pluginConfig);
        }
        
        private PluginConfig AdditionalConfig(PluginConfig config)
        {
            config.ChatSettings = new ChatSettings(config.ChatSettings);
            config.PlayerStateSettings = new PlayerStateSettings(config.PlayerStateSettings);
            config.ServerStateSettings = new ServerStateSettings(config.ServerStateSettings);
            config.PluginSupport = new PluginSupport(config.PluginSupport);
            return config;
        }
        
        private void OnServerInitialized(bool startup)
        {
            _serverInitCalled = true;
            if (IsPluginLoaded(BetterChat))
            {
                if (BetterChat.Version < new VersionNumber(5, 2, 7))
                {
                    PrintWarning("Please update your version of BetterChat to version >= 5.2.7");
                }
                
                if (!string.IsNullOrEmpty(_pluginConfig.ChatSettings.DiscordTag))
                {
                    BetterChat.Call("API_RegisterThirdPartyTitle", this, new Func<IPlayer, string>(GetBetterChatTag));
                }
            }
            
            if (string.IsNullOrEmpty(_pluginConfig.DiscordApiKey))
            {
                PrintWarning("Please set the Discord Bot Token and reload the plugin");
                return;
            }
            
            OnPluginLoaded(plugins.Find("AdminChat"));
            OnPluginLoaded(plugins.Find("AdminDeepCover"));
            OnPluginLoaded(plugins.Find("AntiSpam"));
            OnPluginLoaded(plugins.Find("BetterChatMute"));
            OnPluginLoaded(plugins.Find("Clans"));
            OnPluginLoaded(plugins.Find("TranslationAPI"));
            OnPluginLoaded(plugins.Find("UFilter"));
            
            if (startup)
            {
                Sends[MessageSource.ServerOnline]?.SendTemplate(TemplateKeys.Server.Online, GetDefault());
            }
        }
        
        private void OnServerShutdown()
        {
            Sends[MessageSource.ServerShutdown]?.SendTemplate(TemplateKeys.Server.Shutdown, GetDefault());
        }
        
        private void Unload()
        {
            Instance = null;
        }
        #endregion

        #region Plugins\DiscordChat.Templates.cs
        public void RegisterTemplates()
        {
            DiscordMessageTemplate connecting = CreateTemplateEmbed($"{{{PlaceholderKeys.Message}}}",  DiscordColor.Warning);
            _templates.RegisterGlobalTemplateAsync(this, TemplateKeys.Player.Connecting, connecting, new TemplateVersion(1, 0, 0), new TemplateVersion(1, 0, 0));
            
            DiscordMessageTemplate connected = CreateTemplateEmbed($"{{{PlaceholderKeys.Message}}}",  DiscordColor.Success);
            _templates.RegisterGlobalTemplateAsync(this, TemplateKeys.Player.Connected, connected, new TemplateVersion(1, 0, 0), new TemplateVersion(1, 0, 0));
            
            DiscordMessageTemplate disconnected = CreateTemplateEmbed($"{{{PlaceholderKeys.Message}}}",  DiscordColor.Danger);
            _templates.RegisterGlobalTemplateAsync(this, TemplateKeys.Player.Disconnected, disconnected, new TemplateVersion(1, 0, 0), new TemplateVersion(1, 0, 0));
            
            DiscordMessageTemplate online = CreateTemplateEmbed(":green_circle: The server is now online", DiscordColor.Success);
            _templates.RegisterGlobalTemplateAsync(this, TemplateKeys.Server.Online, online, new TemplateVersion(1, 0, 0), new TemplateVersion(1, 0, 0));
            
            DiscordMessageTemplate shutdown = CreateTemplateEmbed(":red_circle: The server has shutdown", DiscordColor.Danger);
            _templates.RegisterGlobalTemplateAsync(this, TemplateKeys.Server.Shutdown, shutdown, new TemplateVersion(1, 0, 0), new TemplateVersion(1, 0, 0));
            
            DiscordMessageTemplate booting = CreateTemplateEmbed(":yellow_circle: The server is now booting", DiscordColor.Warning);
            _templates.RegisterGlobalTemplateAsync(this, TemplateKeys.Server.Booting, booting, new TemplateVersion(1, 0, 0), new TemplateVersion(1, 0, 0));
            
            DiscordMessageTemplate serverChat = CreateTemplateEmbed($"{{{PlaceholderKeys.Message}}}", DiscordColor.Blurple);
            _templates.RegisterGlobalTemplateAsync(this, TemplateKeys.Chat.General, serverChat, new TemplateVersion(1, 0, 0), new TemplateVersion(1, 0, 0));
            
            DiscordMessageTemplate teamChat = CreateTemplateEmbed($"{{{PlaceholderKeys.Message}}}", DiscordColor.Success);
            _templates.RegisterGlobalTemplateAsync(this, TemplateKeys.Chat.Teams, teamChat, new TemplateVersion(1, 0, 0), new TemplateVersion(1, 0, 0));
            
            DiscordMessageTemplate cardsChat = CreateTemplateEmbed($"{{{PlaceholderKeys.Message}}}", DiscordColor.Danger);
            _templates.RegisterGlobalTemplateAsync(this, TemplateKeys.Chat.Cards, cardsChat, new TemplateVersion(1, 0, 0), new TemplateVersion(1, 0, 0));
            
            DiscordMessageTemplate clanChat = CreateTemplateEmbed($"{{{PlaceholderKeys.Message}}}", new DiscordColor("a1ff46"));
            _templates.RegisterGlobalTemplateAsync(this, TemplateKeys.Chat.Clans.Clan, clanChat, new TemplateVersion(1, 0, 0), new TemplateVersion(1, 0, 0));
            
            DiscordMessageTemplate allianceChat = CreateTemplateEmbed($"{{{PlaceholderKeys.Message}}}",  new DiscordColor("80cc38"));
            _templates.RegisterGlobalTemplateAsync(this, TemplateKeys.Chat.Clans.Alliance, allianceChat, new TemplateVersion(1, 0, 0), new TemplateVersion(1, 0, 0));
            
            DiscordMessageTemplate errorNotLinked = CreatePrefixedTemplateEmbed("You're not allowed to chat with the server unless you are linked.", DiscordColor.Danger);
            _templates.RegisterGlobalTemplateAsync(this, TemplateKeys.Error.NotLinked, errorNotLinked, new TemplateVersion(1, 0, 0), new TemplateVersion(1, 0, 0));
            
            DiscordMessageTemplate errorAdminChatNotLinked = CreatePrefixedTemplateEmbed("You're not allowed to use admin chat because you have not linked your Discord and game server accounts", DiscordColor.Danger);
            _templates.RegisterGlobalTemplateAsync(this, TemplateKeys.Error.AdminChat.NotLinked, errorAdminChatNotLinked, new TemplateVersion(1, 0, 0), new TemplateVersion(1, 0, 0));
            
            DiscordMessageTemplate errorAdminChatNotPermission = CreatePrefixedTemplateEmbed(":no_entry: You're not allowed to use admin chat channel because you do not have permission.", DiscordColor.Danger);
            _templates.RegisterGlobalTemplateAsync(this, TemplateKeys.Error.AdminChat.NoPermission, errorAdminChatNotPermission, new TemplateVersion(1, 0, 0), new TemplateVersion(1, 0, 0));
        }
        
        public DiscordMessageTemplate CreateTemplateEmbed(string description, DiscordColor color)
        {
            return new DiscordMessageTemplate
            {
                Embeds = new List<DiscordEmbedTemplate>
                {
                    new DiscordEmbedTemplate
                    {
                        Description = description,
                        Color = color.ToHex()
                    }
                },
            };
        }
        
        public DiscordMessageTemplate CreatePrefixedTemplateEmbed(string description, DiscordColor color)
        {
            return new DiscordMessageTemplate
            {
                Embeds = new List<DiscordEmbedTemplate>
                {
                    new DiscordEmbedTemplate
                    {
                        Description = $"[{{plugin.title}}] {description}",
                        Color = color.ToHex()
                    }
                },
            };
        }
        
        public void SendGlobalTemplateMessage(string templateName, DiscordChannel channel, PlaceholderData placeholders = null)
        {
            MessageCreate create = new MessageCreate
            {
                AllowedMentions = AllowedMentions.None
            };
            channel?.CreateGlobalTemplateMessage(Client, templateName, create, placeholders);
        }
        
        public string GetTemplateName(MessageSource source)
        {
            switch (source)
            {
                case MessageSource.Discord:
                case MessageSource.Server:
                return TemplateKeys.Chat.General;
                case MessageSource.Team:
                return TemplateKeys.Chat.Teams;
                case MessageSource.Cards:
                return TemplateKeys.Chat.Cards;
                case MessageSource.Connecting:
                return TemplateKeys.Player.Connecting;
                case MessageSource.Connected:
                return TemplateKeys.Player.Connected;
                case MessageSource.Disconnected:
                return TemplateKeys.Player.Disconnected;
                case MessageSource.AdminChat:
                return TemplateKeys.Chat.AdminChat.Message;
                case MessageSource.Clan:
                return TemplateKeys.Chat.Clans.Clan;
                case MessageSource.Alliance:
                return TemplateKeys.Chat.Clans.Alliance;
            }
            
            return null;
        }
        #endregion

        #region Configuration\ChatSettings.cs
        public class ChatSettings
        {
            [JsonProperty("Chat Channel ID")]
            public Snowflake ChatChannel { get; set; }
            
            #if RUST
            [JsonProperty("Team Channel ID")]
            public Snowflake TeamChannel { get; set; }
            
            [JsonProperty("Cards Channel ID")]
            public Snowflake CardsChannel { get; set; }
            #endif
            
            [JsonProperty("Replace Discord User Message With Bot Message")]
            public bool UseBotToDisplayChat { get; set; }
            
            [JsonProperty("Send Messages From Server Chat To Discord Channel")]
            public bool ServerToDiscord { get; set; }
            
            [JsonProperty("Send Messages From Discord Channel To Server Chat")]
            public bool DiscordToServer { get; set; }
            
            [JsonProperty("Add Discord Tag To In Game Messages When Sent From Discord")]
            public string DiscordTag { get; set; }
            
            [JsonProperty("Allow plugins to process Discord to Server Chat Messages")]
            public bool AllowPluginProcessing { get; set; }
            
            [JsonProperty("Text Replacements")]
            public Hash<string, string> TextReplacements { get; set; }
            
            [JsonProperty("Unlinked Settings")]
            public UnlinkedSettings UnlinkedSettings { get; set; }
            
            [JsonProperty("Message Filter Settings")]
            public MessageFilterSettings Filter { get; set; }
            
            public ChatSettings(ChatSettings settings)
            {
                ChatChannel = settings?.ChatChannel ?? default(Snowflake);
                #if RUST
                TeamChannel = settings?.TeamChannel ?? default(Snowflake);
                CardsChannel = settings?.CardsChannel ?? default(Snowflake);
                #endif
                UseBotToDisplayChat = settings?.UseBotToDisplayChat ?? true;
                ServerToDiscord = settings?.ServerToDiscord ?? true;
                DiscordToServer = settings?.DiscordToServer ?? true;
                DiscordTag = settings?.DiscordTag ?? "[#5f79d6][Discord][/#]";
                AllowPluginProcessing = settings?.AllowPluginProcessing ?? true;
                TextReplacements = settings?.TextReplacements ?? new Hash<string, string> { ["TextToBeReplaced"] = "ReplacedText" };
                UnlinkedSettings = new UnlinkedSettings(settings?.UnlinkedSettings);
                Filter = new MessageFilterSettings(settings?.Filter);
            }
        }
        #endregion

        #region Configuration\MessageFilterSettings.cs
        public class MessageFilterSettings
        {
            [JsonProperty("Ignore messages from users in this list (Discord ID)")]
            public List<Snowflake> IgnoreUsers { get; set; }
            
            [JsonProperty("Ignore messages from users in this role (Role ID)")]
            public List<Snowflake> IgnoreRoles { get; set; }
            
            [JsonProperty("Ignored Prefixes")]
            public List<string> IgnoredPrefixes { get; set; }
            
            public MessageFilterSettings(MessageFilterSettings settings)
            {
                IgnoreUsers = settings?.IgnoreUsers ?? new List<Snowflake>();
                IgnoreRoles = settings?.IgnoreRoles ?? new List<Snowflake>();
                IgnoredPrefixes = settings?.IgnoredPrefixes ?? new List<string>();
            }
            
            public bool IgnoreMessage(DiscordMessage message, GuildMember member)
            {
                return IsIgnoredUser(message.Author, member) || IsIgnoredPrefix(message.Content);
            }
            
            public bool IsIgnoredUser(DiscordUser user, GuildMember member)
            {
                if (user.IsBot)
                {
                    return true;
                }
                
                if (IgnoreUsers.Contains(user.Id))
                {
                    return true;
                }
                
                return member != null && IsRoleIgnoredMember(member);
            }
            
            public bool IsRoleIgnoredMember(GuildMember member)
            {
                for (int index = 0; index < IgnoreRoles.Count; index++)
                {
                    Snowflake role = IgnoreRoles[index];
                    if (member.Roles.Contains(role))
                    {
                        return true;
                    }
                }
                
                return false;
            }
            
            public bool IsIgnoredPrefix(string content)
            {
                for (int index = 0; index < IgnoredPrefixes.Count; index++)
                {
                    string prefix = IgnoredPrefixes[index];
                    if (content.StartsWith(prefix))
                    {
                        return true;
                    }
                }
                
                return false;
            }
        }
        #endregion

        #region Configuration\PlayerStateSettings.cs
        public class PlayerStateSettings
        {
            [JsonProperty("Player State Channel ID")]
            public Snowflake PlayerStateChannel { get; set; }
            
            [JsonProperty("Show Admins")]
            public bool ShowAdmins { get; set; }
            
            public PlayerStateSettings(PlayerStateSettings settings)
            {
                PlayerStateChannel = settings?.PlayerStateChannel ?? default(Snowflake);
                ShowAdmins = settings?.ShowAdmins ?? true;
            }
        }
        #endregion

        #region Configuration\PluginConfig.cs
        public class PluginConfig
        {
            [JsonProperty(PropertyName = "Discord Bot Token")]
            public string DiscordApiKey { get; set; } = string.Empty;
            
            [JsonProperty("Chat Settings")]
            public ChatSettings ChatSettings { get; set; }
            
            [JsonProperty("Player State Settings")]
            public PlayerStateSettings PlayerStateSettings { get; set; }
            
            [JsonProperty("Server State Settings")]
            public ServerStateSettings ServerStateSettings { get; set; }
            
            [JsonProperty("Plugin Support")]
            public PluginSupport PluginSupport { get; set; }
            
            [JsonConverter(typeof(StringEnumConverter))]
            [DefaultValue(DiscordLogLevel.Info)]
            [JsonProperty(PropertyName = "Discord Extension Log Level (Verbose, Debug, Info, Warning, Error, Exception, Off)")]
            public DiscordLogLevel ExtensionDebugging { get; set; } = DiscordLogLevel.Info;
        }
        #endregion

        #region Configuration\ServerStateSettings.cs
        public class ServerStateSettings
        {
            [JsonProperty("Server State Channel ID")]
            public Snowflake ServerStateChannel { get; set; }
            
            public ServerStateSettings(ServerStateSettings settings)
            {
                ServerStateChannel = settings?.ServerStateChannel ?? default(Snowflake);
            }
        }
        #endregion

        #region Configuration\UnlinkedSettings.cs
        public class UnlinkedSettings
        {
            [JsonProperty("Allow Unlinked Players To Chat With Server")]
            public bool AllowedUnlinked { get; set; }
            
            #if RUST
            [JsonProperty("Steam Icon ID")]
            public ulong SteamIcon { get; set; }
            #endif
            
            public UnlinkedSettings(UnlinkedSettings settings)
            {
                AllowedUnlinked = settings?.AllowedUnlinked ?? true;
                #if RUST
                SteamIcon = settings?.SteamIcon ?? 76561199144296099;
                #endif
            }
        }
        #endregion

        #region Enums\MessageSource.cs
        public enum MessageSource : byte
        {
            Connecting,
            Connected,
            Disconnected,
            ServerBooting,
            ServerOnline,
            ServerShutdown,
            Server,
            Discord,
            Team,
            Cards,
            Clan,
            Alliance,
            AdminChat
        }
        #endregion

        #region Helpers\DiscordSendQueue.cs
        public class DiscordSendQueue
        {
            private readonly StringBuilder _message = new StringBuilder();
            private Timer _sendTimer;
            private readonly DiscordChannel _channel;
            private readonly string _templateId;
            private readonly Action _callback;
            private readonly PluginTimers _timer;
            
            public DiscordSendQueue(DiscordChannel channel, string templateId, PluginTimers timers)
            {
                _channel = channel;
                _templateId = templateId;
                _callback = Send;
                _timer = timers;
            }
            
            public void QueueMessage(string message)
            {
                if (string.IsNullOrWhiteSpace(message))
                {
                    return;
                }
                
                if (_message.Length + message.Length > 2000)
                {
                    Send();
                }
                
                if (_sendTimer == null)
                {
                    _sendTimer = _timer.In(1f, _callback);
                }
                
                _message.AppendLine(message);
            }
            
            public void SendTemplate(string templateId, PlaceholderData data)
            {
                DiscordChat.Instance.SendGlobalTemplateMessage(templateId, _channel, data);
            }
            
            public void Send()
            {
                if (_message.Length > 2000)
                {
                    _message.Length = 2000;
                }
                
                PlaceholderData placeholders = DiscordChat.Instance.GetDefault().Add(PlaceholderKeys.Data.Message, _message.ToString());
                _message.Length = 0;
                DiscordChat.Instance.SendGlobalTemplateMessage(_templateId, _channel, placeholders);
                _sendTimer?.Destroy();
                _sendTimer = null;
            }
        }
        #endregion

        #region Localization\LangKeys.cs
        public static class LangKeys
        {
            public const string Root = "V3.";
            
            public static class Discord
            {
                private const string Base = Root + nameof(Discord) + ".";
                
                public static class Chat
                {
                    private const string Base = Discord.Base + nameof(Chat) + ".";
                    
                    public const string Server = Base + nameof(Server);
                    public const string LinkedMessage = Base + nameof(LinkedMessage);
                    public const string UnlinkedMessage = Base + nameof(UnlinkedMessage);
                    public const string PlayerName = Base + nameof(PlayerName);
                }
                
                public static class Team
                {
                    private const string Base = Discord.Base + nameof(Team) + ".";
                    
                    public const string Message = Base + nameof(Message);
                }
                
                public static class Cards
                {
                    private const string Base = Discord.Base + nameof(Cards) + ".";
                    
                    public const string Message = Base + nameof(Message);
                }
                
                public static class Player
                {
                    private const string Base = Discord.Base + nameof(Player) + ".";
                    
                    public const string Connecting = Base + nameof(Connecting);
                    public const string Connected = Base + nameof(Connected);
                    public const string Disconnected = Base + nameof(Disconnected);
                }
                
                public static class AdminChat
                {
                    private const string Base = Discord.Base + nameof(AdminChat) + ".";
                    
                    public const string ServerMessage = Base + nameof(ServerMessage);
                    public const string DiscordMessage = Base + nameof(DiscordMessage);
                }
                
                public static class Clans
                {
                    private const string Base = Discord.Base + nameof(Clans) + ".";
                    
                    public const string ClanMessage = Base + nameof(ClanMessage);
                    public const string AllianceMessage = Base + nameof(AllianceMessage);
                }
            }
            
            public static class Server
            {
                private const string Base = Root + nameof(Server) + ".";
                
                public const string LinkedMessage = Base + nameof(LinkedMessage);
                public const string UnlinkedMessage = Base + nameof(UnlinkedMessage);
            }
        }
        #endregion

        #region Placeholders\PlaceholderKeys.cs
        public class PlaceholderKeys
        {
            public const string Message = "discordchat.message";
            public const string PlayerMessage = "discordchat.player.message";
            public const string DisconnectReason = "discordchat.disconnect.reason";
            public const string PlayerName = "discordchat.player.name";
            public const string DiscordTag = "discordchat.discord.tag";
            
            public class Data
            {
                public const string Message = "message";
                public const string PlayerMessage = "player.message";
                public const string DisconnectReason = "reason";
            }
        }
        #endregion

        #region PluginHandlers\AdminChatHandler.cs
        public class AdminChatHandler : BasePluginHandler
        {
            private readonly AdminChatSettings _settings;
            private readonly DiscordClient _client;
            
            public AdminChatHandler(DiscordClient client, DiscordChat chat, AdminChatSettings settings, Plugin plugin) : base(chat, plugin)
            {
                _client = client;
                _settings = settings;
            }
            
            public override bool CanSendMessage(string message, IPlayer player, DiscordUser user, MessageSource source, DiscordMessage sourceMessage)
            {
                return source == MessageSource.AdminChat ? !_settings.Enabled : !IsAdminChatMessage(player, message);
            }
            
            public override bool SendMessage(string message, IPlayer player, DiscordUser user, MessageSource source, DiscordMessage sourceMessage)
            {
                if (source != MessageSource.AdminChat)
                {
                    return false;
                }
                
                PlaceholderData placeholders = Chat.GetDefault().AddPlayer(player).Add(PlaceholderKeys.Data.PlayerMessage, message);
                
                if (sourceMessage != null)
                {
                    if (_settings.ReplaceWithBot)
                    {
                        sourceMessage.Delete(_client);
                        Chat.Sends[source]?.QueueMessage(Chat.Lang(LangKeys.Discord.AdminChat.DiscordMessage, placeholders));
                    }
                    
                    Plugin.Call("SendAdminMessage", player, message);
                }
                else
                {
                    Chat.Sends[source]?.QueueMessage(Chat.Lang(LangKeys.Discord.AdminChat.ServerMessage, placeholders));
                }
                
                return true;
            }
            
            private bool IsAdminChatMessage(IPlayer player, string message) => Chat.CanPlayerAdminChat(player) && (message.StartsWith(_settings.AdminChatPrefix) || Plugin.Call<bool>("HasAdminChatEnabled", player));
        }
        #endregion

        #region PluginHandlers\AdminDeepCoverHandler.cs
        public class AdminDeepCoverHandler : BasePluginHandler
        {
            public AdminDeepCoverHandler(DiscordChat chat, Plugin plugin) : base(chat, plugin) { }
            
            public override bool CanSendMessage(string message, IPlayer player, DiscordUser user, MessageSource type, DiscordMessage sourceMessage)
            {
                return player?.Object != null
                && (type == MessageSource.Discord || type == MessageSource.Server)
                && !Plugin.Call<bool>("API_IsDeepCovered", player.Object);
            }
        }
        #endregion

        #region PluginHandlers\AntiSpamHandler.cs
        public class AntiSpamHandler : BasePluginHandler
        {
            private readonly AntiSpamSettings _settings;
            
            public AntiSpamHandler(DiscordChat chat, AntiSpamSettings settings, Plugin plugin) : base(chat, plugin)
            {
                _settings = settings;
            }
            
            public override void ProcessPlayerName(StringBuilder name, IPlayer player)
            {
                if (!_settings.PlayerName || player == null)
                {
                    return;
                }
                
                string builtName = name.ToString();
                builtName = Plugin.Call<string>("GetSpamFreeText", builtName);
                builtName = Plugin.Call<string>("GetImpersonationFreeText", builtName);
                name.Length = 0;
                name.Append(builtName);
            }
            
            public override void ProcessMessage(StringBuilder message, IPlayer player, DiscordUser user, MessageSource source)
            {
                if (CanFilterMessage(source))
                {
                    string clearMessage = Plugin.Call<string>("GetSpamFreeText", message.ToString());
                    message.Length = 0;
                    message.Append(clearMessage);
                }
            }
            
            private bool CanFilterMessage(MessageSource source)
            {
                switch (source)
                {
                    case MessageSource.Discord:
                    return _settings.DiscordMessage;
                    case MessageSource.Server:
                    return _settings.ServerMessage;
                    case MessageSource.Team:
                    return _settings.TeamMessage;
                    case MessageSource.Cards:
                    return _settings.CardMessages;
                    case MessageSource.Clan:
                    case MessageSource.Alliance:
                    return _settings.PluginMessage;
                }
                
                return false;
            }
        }
        #endregion

        #region PluginHandlers\BasePluginHandler.cs
        public abstract class BasePluginHandler : IPluginHandler
        {
            protected readonly DiscordChat Chat;
            protected readonly Plugin Plugin;
            private readonly string _pluginName;
            
            protected BasePluginHandler(DiscordChat chat, Plugin plugin)
            {
                Chat = chat;
                Plugin = plugin;
                _pluginName = plugin.Name;
            }
            
            public virtual bool CanSendMessage(string message, IPlayer player, DiscordUser user, MessageSource source, DiscordMessage sourceMessage) => true;
            
            public virtual void ProcessPlayerName(StringBuilder name, IPlayer player) { }
            
            public virtual bool HasCallbackMessage() => false;
            
            public virtual void ProcessCallbackMessage(string message, IPlayer player, DiscordUser user, MessageSource source, Action<string> callback) { }
            
            public virtual void ProcessMessage(StringBuilder message, IPlayer player, DiscordUser user, MessageSource source) { }
            
            public virtual bool SendMessage(string message, IPlayer player, DiscordUser user, MessageSource source, DiscordMessage sourceMessage) => false;
            
            public string GetPluginName() => _pluginName;
        }
        #endregion

        #region PluginHandlers\BetterChatMuteHandler.cs
        public class BetterChatMuteHandler : BasePluginHandler
        {
            private readonly BetterChatMuteSettings _settings;
            
            public BetterChatMuteHandler(DiscordChat chat, BetterChatMuteSettings settings, Plugin plugin) : base(chat, plugin)
            {
                _settings = settings;
            }
            
            public override bool CanSendMessage(string message, IPlayer player, DiscordUser user, MessageSource source, DiscordMessage sourceMessage)
            {
                return player != null && !Plugin.Call<bool>("API_IsMuted", player);
            }
        }
        #endregion

        #region PluginHandlers\DiscordChatHandler.cs
        public class DiscordChatHandler : BasePluginHandler
        {
            private readonly ChatSettings _settings;
            private readonly IServer _server;
            private readonly object[] _unlinkedArgs = new object[2];
            
            public DiscordChatHandler(DiscordChat chat, ChatSettings settings, Plugin plugin, IServer server) : base(chat, plugin)
            {
                _settings = settings;
                _server = server;
                _unlinkedArgs[0] = settings.UnlinkedSettings.SteamIcon;
            }
            
            public override bool CanSendMessage(string message, IPlayer player, DiscordUser user, MessageSource source, DiscordMessage sourceMessage)
            {
                if (sourceMessage != null)
                {
                    if (_settings.Filter.IgnoreMessage(sourceMessage, Chat.Client.Bot.GetGuild(sourceMessage.GuildId).Members[sourceMessage.Author.Id]))
                    {
                        return false;
                    }
                }
                
                switch (source)
                {
                    case MessageSource.Discord:
                    return _settings.DiscordToServer && (_settings.UnlinkedSettings.AllowedUnlinked || (player != null && player.IsLinked()));
                    
                    case MessageSource.Server:
                    return _settings.ServerToDiscord;
                }
                
                return true;
            }
            
            public override bool SendMessage(string message, IPlayer player, DiscordUser user, MessageSource source, DiscordMessage sourceMessage)
            {
                switch (source)
                {
                    case MessageSource.Discord:
                    if (_settings.UseBotToDisplayChat)
                    {
                        if (player.IsLinked())
                        {
                            Chat.Sends[MessageSource.Discord]?.QueueMessage(Chat.Lang(LangKeys.Discord.Chat.LinkedMessage, GetPlaceholders(message, player, user, sourceMessage)));
                        }
                        else
                        {
                            Chat.Sends[MessageSource.Discord]?.QueueMessage(Chat.Lang(LangKeys.Discord.Chat.UnlinkedMessage, GetPlaceholders(message, player, user, sourceMessage)));
                        }
                    }
                    
                    if (player.IsLinked())
                    {
                        SendLinkedToServer(player, message, sourceMessage);
                    }
                    else
                    {
                        SendUnlinkedToServer(sourceMessage, message);
                    }
                    
                    return true;
                    
                    case MessageSource.Server:
                    Chat.Sends[MessageSource.Discord]?.QueueMessage(Chat.Lang(LangKeys.Discord.Chat.Server, GetPlaceholders(message, player, user, sourceMessage)));
                    return true;
                    case MessageSource.Team:
                    Chat.Sends[MessageSource.Team]?.QueueMessage(Chat.Lang(LangKeys.Discord.Team.Message, GetPlaceholders(message, player, user, sourceMessage)));
                    return true;
                    case MessageSource.Cards:
                    Chat.Sends[MessageSource.Cards]?.QueueMessage(Chat.Lang(LangKeys.Discord.Cards.Message, GetPlaceholders(message, player, user, sourceMessage)));
                    return true;
                    case MessageSource.AdminChat:
                    Chat.Sends[MessageSource.AdminChat]?.QueueMessage(Chat.Lang(LangKeys.Discord.AdminChat.DiscordMessage, GetPlaceholders(message, player, user, sourceMessage)));
                    return true;
                    case MessageSource.Clan:
                    Chat.Sends[MessageSource.Clan]?.QueueMessage(Chat.Lang(LangKeys.Discord.Clans.ClanMessage, GetPlaceholders(message, player, user, sourceMessage)));
                    return true;
                    case MessageSource.Alliance:
                    Chat.Sends[MessageSource.Alliance]?.QueueMessage(Chat.Lang(LangKeys.Discord.Clans.AllianceMessage, GetPlaceholders(message, player, user, sourceMessage)));
                    return true;
                }
                
                return false;
            }
            
            public void SendLinkedToServer(IPlayer player, string message, DiscordMessage source)
            {
                if (_settings.AllowPluginProcessing)
                {
                    bool playerReturn = false;
                    #if RUST
                    //Let other chat plugins process first
                    if (player.Object != null)
                    {
                        Chat.Unsubscribe("OnPlayerChat");
                        playerReturn = Interface.Call("OnPlayerChat", player.Object, message, ConVar.Chat.ChatChannel.Global) != null;
                        Chat.Subscribe("OnPlayerChat");
                    }
                    #endif
                    
                    //Let other chat plugins process first
                    Chat.Unsubscribe("OnUserChat");
                    bool userReturn = Interface.Call("OnUserChat", player, message) != null;
                    Chat.Subscribe("OnUserChat");
                    
                    if (playerReturn || userReturn)
                    {
                        return;
                    }
                    
                    if (Chat.SendBetterChatMessage(player, message))
                    {
                        return;
                    }
                }
                
                PlaceholderData data = Chat.GetDefault().AddPlayer(player).AddMessage(source).Add(PlaceholderKeys.Data.PlayerMessage, message);
                message = Chat.Lang(LangKeys.Server.LinkedMessage, data);
                _server.Broadcast(message);
                Chat.Puts(Formatter.ToPlaintext(message));
            }
            
            public void SendUnlinkedToServer(DiscordMessage sourceMessage, string message)
            {
                if (_settings.UseBotToDisplayChat)
                {
                    Chat.Sends[MessageSource.Server]?.QueueMessage(Chat.Lang(LangKeys.Discord.Chat.UnlinkedMessage, Chat.GetDefault().AddMessage(sourceMessage).Add(PlaceholderKeys.Data.PlayerMessage, message)));
                }
                
                string serverMessage = Chat.Lang(LangKeys.Server.UnlinkedMessage, Chat.GetDefault().AddMessage(sourceMessage).AddGuildMember(Chat.Client.Bot.GetGuild(sourceMessage.GuildId).Members[sourceMessage.Author.Id]).Add(PlaceholderKeys.Data.PlayerMessage, message));
                #if RUST
                _unlinkedArgs[1] = serverMessage;
                ConsoleNetwork.BroadcastToAllClients("chat.add", _unlinkedArgs);
                #else
                _server.Broadcast(serverMessage);
                #endif
                
                Chat.Puts(Formatter.ToPlaintext(serverMessage));
            }
            
            private PlaceholderData GetPlaceholders(string message, IPlayer player, DiscordUser user, DiscordMessage sourceMessage)
            {
                return Chat.GetDefault().AddPlayer(player).AddUser(user).AddMessage(sourceMessage).Add(PlaceholderKeys.Data.PlayerMessage, message);
            }
            
            public override void ProcessMessage(StringBuilder message, IPlayer player, DiscordUser user, MessageSource source)
            {
                foreach (KeyValuePair<string,string> replacement in _settings.TextReplacements)
                {
                    message.Replace(replacement.Key, replacement.Value);
                }
            }
        }
        #endregion

        #region PluginHandlers\IPluginHandler.cs
        public interface IPluginHandler
        {
            bool CanSendMessage(string message, IPlayer player, DiscordUser user, MessageSource source, DiscordMessage sourceMessage);
            void ProcessPlayerName(StringBuilder name, IPlayer player);
            bool HasCallbackMessage();
            void ProcessCallbackMessage(string message, IPlayer player, DiscordUser user, MessageSource source, Action<string> callback);
            void ProcessMessage(StringBuilder message, IPlayer player, DiscordUser user, MessageSource source);
            bool SendMessage(string message, IPlayer player, DiscordUser user, MessageSource source, DiscordMessage sourceMessage);
            string GetPluginName();
        }
        #endregion

        #region PluginHandlers\TranslationApiHandler.cs
        public class TranslationApiHandler : BasePluginHandler
        {
            private readonly ChatTranslatorSettings _settings;
            
            public TranslationApiHandler(DiscordChat chat, ChatTranslatorSettings settings, Plugin plugin) : base(chat, plugin)
            {
                _settings = settings;
            }
            
            public override bool HasCallbackMessage() => true;
            
            public override void ProcessCallbackMessage(string message, IPlayer player, DiscordUser user, MessageSource source, Action<string> callback)
            {
                if (CanChatTranslatorSource(source))
                {
                    Plugin.Call("Translate", message, _settings.DiscordServerLanguage, "auto", callback);
                    return;
                }
                
                callback.Invoke(message);
            }
            
            public bool CanChatTranslatorSource(MessageSource source)
            {
                if (!_settings.Enabled)
                {
                    return false;
                }
                
                switch (source)
                {
                    case MessageSource.Server:
                    return _settings.ServerMessage;
                    
                    case MessageSource.Discord:
                    return _settings.DiscordMessage;
                    
                    case MessageSource.Clan:
                    case MessageSource.Alliance:
                    return _settings.PluginMessage;
                    
                    #if RUST
                    case MessageSource.Team:
                    return _settings.TeamMessage;
                    
                    case MessageSource.Cards:
                    return _settings.CardMessages;
                    #endif
                }
                
                return false;
            }
        }
        #endregion

        #region PluginHandlers\UFilterHandler.cs
        public class UFilterHandler : BasePluginHandler
        {
            private readonly UFilterSettings _settings;
            
            public UFilterHandler(DiscordChat chat, UFilterSettings settings, Plugin plugin) : base(chat, plugin)
            {
                _settings = settings;
            }
            
            public override void ProcessPlayerName(StringBuilder name, IPlayer player)
            {
                if (_settings.PlayerNames)
                {
                    UFilterText(name);
                }
            }
            
            public override void ProcessMessage(StringBuilder message, IPlayer player, DiscordUser user, MessageSource source)
            {
                if (CanFilterMessage(source))
                {
                    UFilterText(message);
                }
            }
            
            private bool CanFilterMessage(MessageSource source)
            {
                switch (source)
                {
                    case MessageSource.Discord:
                    return _settings.DiscordMessages;
                    case MessageSource.Server:
                    return _settings.ServerMessage;
                    case MessageSource.Team:
                    return _settings.TeamMessage;
                    case MessageSource.Cards:
                    return _settings.CardMessage;
                    case MessageSource.Clan:
                    case MessageSource.Alliance:
                    return _settings.PluginMessages;
                }
                
                return false;
            }
            
            private void UFilterText(StringBuilder text)
            {
                string[] profanities = Plugin.Call<string[]>("Profanities", text.ToString());
                for (int index = 0; index < profanities.Length; index++)
                {
                    string profanity = profanities[index];
                    text.Replace(profanity, new string(_settings.ReplacementCharacter, profanity.Length));
                }
            }
        }
        #endregion

        #region Templates\TemplateKeys.cs
        public static class TemplateKeys
        {
            public static class Player
            {
                private const string Base = nameof(Player) + ".";
                
                public const string Connecting = Base + nameof(Connecting);
                public const string Connected = Base + nameof(Connected);
                public const string Disconnected = Base + nameof(Disconnected);
            }
            
            public static class Server
            {
                private const string Base = nameof(Server) + ".";
                
                public const string Online = Base + nameof(Online);
                public const string Shutdown = Base + nameof(Shutdown);
                public const string Booting = Base + nameof(Booting);
            }
            
            public static class Chat
            {
                private const string Base = nameof(Chat) + ".";
                
                public const string General = Base + nameof(General);
                public const string Teams = Base + nameof(Teams);
                public const string Cards = Base + nameof(Cards);
                
                public static class Clans
                {
                    private const string Base = Chat.Base + nameof(Clans) + ".";
                    
                    public const string Clan = Base + nameof(Clan);
                    public const string Alliance = Base + nameof(Alliance);
                }
                
                public static class AdminChat
                {
                    private const string Base = Chat.Base + nameof(AdminChat) + ".";
                    
                    public const string Message = Base + nameof(Message);
                }
            }
            
            public static class Error
            {
                private const string Base = nameof(Error) + ".";
                
                public const string NotLinked = Base + nameof(NotLinked);
                
                public static class AdminChat
                {
                    private const string Base = Error.Base + nameof(AdminChat) + ".";
                    
                    public const string NotLinked = Base + nameof(NotLinked);
                    public const string NoPermission = Base + nameof(NoPermission);
                }
            }
        }
        #endregion

        #region Configuration\Plugins\AdminChatSettings.cs
        public class AdminChatSettings
        {
            [JsonProperty("Enable AdminChat Plugin Support")]
            public bool Enabled { get; set; }
            
            [JsonProperty("Chat Channel ID")]
            public Snowflake ChatChannel { get; set; }
            
            [JsonProperty("Chat Prefix")]
            public string AdminChatPrefix { get; set; }
            
            [JsonProperty("Replace Discord Message With Bot")]
            public bool ReplaceWithBot { get; set; }
            
            public AdminChatSettings(AdminChatSettings settings)
            {
                Enabled = settings?.Enabled ?? false;
                ChatChannel = settings?.ChatChannel ?? default(Snowflake);
                AdminChatPrefix = settings?.AdminChatPrefix ?? "@";
                ReplaceWithBot = settings?.ReplaceWithBot ?? true;
            }
        }
        #endregion

        #region Configuration\Plugins\AntiSpamSettings.cs
        public class AntiSpamSettings
        {
            [JsonProperty("Use AntiSpam On Player Names")]
            public bool PlayerName { get; set; }
            
            [JsonProperty("Use AntiSpam On Server Messages")]
            public bool ServerMessage { get; set; }
            
            [JsonProperty("Use AntiSpam On Chat Messages")]
            public bool DiscordMessage { get; set; }
            
            [JsonProperty("Use AntiSpam On Plugin Messages")]
            public bool PluginMessage { get; set; }
            
            #if RUST
            [JsonProperty("Use AntiSpam On Team Messages")]
            public bool TeamMessage { get; set; }
            
            [JsonProperty("Use AntiSpam On Card Messages")]
            public bool CardMessages { get; set; }
            #endif
            
            public AntiSpamSettings(AntiSpamSettings settings)
            {
                PlayerName = settings?.PlayerName ?? false;
                ServerMessage = settings?.ServerMessage ?? false;
                DiscordMessage = settings?.DiscordMessage ?? false;
                PluginMessage = settings?.PluginMessage ?? false;
                #if RUST
                TeamMessage = settings?.TeamMessage ?? false;
                CardMessages = settings?.CardMessages ?? false;
                #endif
            }
        }
        #endregion

        #region Configuration\Plugins\BetterChatMuteSettings.cs
        public class BetterChatMuteSettings
        {
            [JsonProperty("Ignore Muted Players")]
            public bool IgnoreMuted { get; set; }
            
            public BetterChatMuteSettings(BetterChatMuteSettings settings)
            {
                IgnoreMuted = settings?.IgnoreMuted ?? true;
            }
        }
        #endregion

        #region Configuration\Plugins\ChatTranslatorSettings.cs
        public class ChatTranslatorSettings
        {
            [JsonProperty("Enable Chat Translator")]
            public bool Enabled { get; set; }
            
            [JsonProperty("Use ChatTranslator On Server Messages")]
            public bool ServerMessage { get; set; }
            
            [JsonProperty("Use ChatTranslator On Chat Messages")]
            public bool DiscordMessage { get; set; }
            
            [JsonProperty("Use ChatTranslator On Plugin Messages")]
            public bool PluginMessage { get; set; }
            
            #if RUST
            [JsonProperty("Use ChatTranslator On Team Messages")]
            public bool TeamMessage { get; set; }
            
            [JsonProperty("Use ChatTranslator On Card Messages")]
            public bool CardMessages { get; set; }
            #endif
            
            [JsonProperty("Discord Server Chat Language")]
            public string DiscordServerLanguage { get; set; }
            
            public ChatTranslatorSettings(ChatTranslatorSettings settings)
            {
                Enabled = settings?.Enabled ?? false;
                ServerMessage = settings?.ServerMessage ?? false;
                DiscordMessage = settings?.DiscordMessage ?? false;
                #if RUST
                TeamMessage = settings?.TeamMessage ?? false;
                CardMessages = settings?.CardMessages ?? false;
                #endif
                DiscordServerLanguage = settings?.DiscordServerLanguage ?? Interface.Oxide.GetLibrary<Lang>().GetServerLanguage();
            }
        }
        #endregion

        #region Configuration\Plugins\ClansSettings.cs
        public class ClansSettings
        {
            [JsonProperty("Clans Chat Channel ID")]
            public Snowflake ClansChatChannel { get; set; }
            
            [JsonProperty("Alliance Chat Channel ID")]
            public Snowflake AllianceChatChannel { get; set; }
            
            public ClansSettings(ClansSettings settings)
            {
                ClansChatChannel = settings?.ClansChatChannel ?? default(Snowflake);
                AllianceChatChannel = settings?.AllianceChatChannel ?? default(Snowflake);
            }
        }
        #endregion

        #region Configuration\Plugins\PluginSupport.cs
        public class PluginSupport
        {
            [JsonProperty("AdminChat Settings")]
            public AdminChatSettings AdminChat { get; set; }
            
            [JsonProperty("AntiSpam Settings")]
            public AntiSpamSettings AntiSpam { get; set; }
            
            [JsonProperty("BetterChatMute Settings")]
            public BetterChatMuteSettings BetterChatMute { get; set; }
            
            [JsonProperty("ChatTranslator Settings")]
            public ChatTranslatorSettings ChatTranslator { get; set; }
            
            [JsonProperty("Clan Settings")]
            public ClansSettings Clans { get; set; }
            
            [JsonProperty("UFilter Settings")]
            public UFilterSettings UFilter { get; set; }
            
            public PluginSupport(PluginSupport settings)
            {
                AdminChat = new AdminChatSettings(settings?.AdminChat);
                BetterChatMute = new BetterChatMuteSettings(settings?.BetterChatMute);
                ChatTranslator = new ChatTranslatorSettings(settings?.ChatTranslator);
                Clans = new ClansSettings(settings?.Clans);
                AntiSpam = new AntiSpamSettings(settings?.AntiSpam);
                UFilter = new UFilterSettings(settings?.UFilter);
            }
        }
        #endregion

        #region Configuration\Plugins\UFilterSettings.cs
        public class UFilterSettings
        {
            [JsonProperty("Use UFilter On Player Names")]
            public bool PlayerNames { get; set; }
            
            [JsonProperty("Use UFilter On Server Messages")]
            public bool ServerMessage { get; set; }
            
            [JsonProperty("Use UFilter On Discord Messages")]
            public bool DiscordMessages { get; set; }
            
            [JsonProperty("Use UFilter On Plugin Messages")]
            public bool PluginMessages { get; set; }
            
            #if RUST
            [JsonProperty("Use UFilter On Team Messages")]
            public bool TeamMessage { get; set; }
            
            [JsonProperty("Use UFilter On Card Messages")]
            public bool CardMessage { get; set; }
            #endif
            
            [JsonProperty("Replacement Character")]
            public char ReplacementCharacter { get; set; }
            
            public UFilterSettings(UFilterSettings settings)
            {
                PlayerNames = settings?.PlayerNames ?? false;
                ServerMessage = settings?.ServerMessage ?? false;
                DiscordMessages = settings?.DiscordMessages ?? false;
                PluginMessages = settings?.PluginMessages ?? false;
                #if RUST
                TeamMessage = settings?.TeamMessage ?? false;
                CardMessage = settings?.CardMessage ?? false;
                #endif
                
                ReplacementCharacter = settings?.ReplacementCharacter ?? '＊';
            }
        }
        #endregion

    }

}
