using ConVar;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Oxide.Core;
using Oxide.Core.Libraries;
using Oxide.Core.Libraries.Covalence;
using Oxide.Core.Plugins;
using Oxide.Ext.Discord;
using Oxide.Ext.Discord.Attributes;
using Oxide.Ext.Discord.Constants;
using Oxide.Ext.Discord.Entities;
using Oxide.Ext.Discord.Entities.Applications;
using Oxide.Ext.Discord.Entities.Channels;
using Oxide.Ext.Discord.Entities.Gatway;
using Oxide.Ext.Discord.Entities.Gatway.Events;
using Oxide.Ext.Discord.Entities.Guilds;
using Oxide.Ext.Discord.Entities.Messages;
using Oxide.Ext.Discord.Entities.Messages.AllowedMentions;
using Oxide.Ext.Discord.Entities.Permissions;
using Oxide.Ext.Discord.Entities.Users;
using Oxide.Ext.Discord.Extensions;
using Oxide.Ext.Discord.Libraries.Placeholders;
using Oxide.Ext.Discord.Libraries.Subscription;
using Oxide.Ext.Discord.Libraries.Templates;
using Oxide.Ext.Discord.Libraries.Templates.Messages;
using Oxide.Ext.Discord.Libraries.Templates.Messages.Embeds;
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
    [Info("Discord Chat", "MJSU", "2.5.0")]
    [Description("Allows chatting between discord and game server")]
    public partial class DiscordChat : CovalencePlugin
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
                message.ReplyWithGlobalTemplate(_client, this, TemplateKeys.Error.AdminChat.NotLinked, null, GetDefault().Add(message));
                return;
            }
            
            if (!CanPlayerAdminChat(player))
            {
                message.ReplyWithGlobalTemplate(_client, this, TemplateKeys.Error.AdminChat.NoPermission, null, GetDefault().Add(player).Add(message));
                return;
            }
            
            HandleMessage(message.Content, player, player.GetDiscordUser(), MessageSource.AdminChat, message);
        }
        
        public bool IsAdminChatEnabled() => _adminChatSettings.Enabled && Sends.ContainsKey(MessageSource.AdminChat);
        public bool CanPlayerAdminChat(IPlayer player) => _adminChatSettings.Enabled && player.HasPermission(AdminChatPermission);
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
        
        public void GetBetterChatMessage(IPlayer player, string message, out string playerInfo, out string messageInfo)
        {
            string bcMessage = GetBetterChatConsoleMessage(player, message);
            int index = bcMessage.IndexOf(':');
            
            if (index != -1)
            {
                playerInfo = bcMessage.Substring(0, index);
                messageInfo = bcMessage.Substring(index + 2);
            }
            else
            {
                playerInfo = string.Empty;
                messageInfo = bcMessage;
            }
        }
        
        public string GetBetterChatConsoleMessage(IPlayer player, string message)
        {
            return BetterChat.Call<string>("API_GetFormattedMessage", player, message, true);
        }
        #endregion

        #region Plugins\DiscordChat.Clans.cs
        private void OnClanChat(IPlayer player, string message, string tag)
        {
            Sends[MessageSource.ClanChat]?.QueueMessage(Lang(LangKeys.Discord.Clans.ClanMessage, GetClanPlaceholders(player, message, tag)));
        }
        
        private void OnAllianceChat(IPlayer player, string message, string tag)
        {
            Sends[MessageSource.AllianceChat]?.QueueMessage(Lang(LangKeys.Discord.Clans.AllianceMessage, GetClanPlaceholders(player, message, tag)));
        }
        
        public PlaceholderData GetClanPlaceholders(IPlayer player, string message, string tag)
        {
            return GetDefault().Add(player).Add(PlayerMessagePlaceholder, message).Add(ClanTagPlaceholder, tag);
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
                
                _client.Connect(_discordSettings);
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
            
            DiscordGuild guild = null;
            if (ready.Guilds.Count == 1 && !_pluginConfig.GuildId.IsValid())
            {
                guild = ready.Guilds.Values.FirstOrDefault();
            }
            
            if (guild == null)
            {
                guild = ready.Guilds[_pluginConfig.GuildId];
                if (guild == null)
                {
                    PrintError("Failed to find a matching guild for the Discord Server Id. " +
                    "Please make sure your guild Id is correct and the bot is in the discord server.");
                    return;
                }
            }
            
            DiscordApplication app = _client.Bot.Application;
            if (!app.HasApplicationFlag(ApplicationFlags.GatewayMessageContentLimited))
            {
                PrintWarning($"You will need to enable \"Message Content Intent\" for {_client.Bot.BotUser.Username} @ https://discord.com/developers/applications\n by April 2022" +
                $"{Name} will stop function correctly after that date until that is fixed. Once updated please reload {Name}.");
            }
            
            _guildId = guild.Id;
            Puts($"{Title} Ready");
        }
        
        [HookMethod(DiscordExtHooks.OnDiscordGuildCreated)]
        private void OnDiscordGuildCreated(DiscordGuild guild)
        {
            if (guild.Id != _guildId)
            {
                return;
            }
            
            Guild = guild;
            if (_pluginConfig.MessageSettings.DiscordToServer)
            {
                SetupChannel(MessageSource.Server, _pluginConfig.ChannelSettings.ChatChannel, _pluginConfig.MessageSettings.UseBotMessageDisplay, HandleDiscordChatMessage);
            }
            else
            {
                SetupChannel(MessageSource.Server, _pluginConfig.ChannelSettings.ChatChannel, _pluginConfig.MessageSettings.UseBotMessageDisplay);
            }
            
            SetupChannel(MessageSource.PlayerState, _pluginConfig.ChannelSettings.PlayerStateChannel, false);
            SetupChannel(MessageSource.ServerState, _pluginConfig.ChannelSettings.ServerStateChannel, false);
            SetupChannel(MessageSource.AdminChat, _pluginConfig.PluginSupport.AdminChat.ChatChannel, _pluginConfig.MessageSettings.UseBotMessageDisplay, HandleAdminChatDiscordMessage);
            SetupChannel(MessageSource.ClanChat, _pluginConfig.PluginSupport.Clans.ClansChatChannel, _pluginConfig.MessageSettings.UseBotMessageDisplay);
            SetupChannel(MessageSource.AllianceChat, _pluginConfig.PluginSupport.Clans.AllianceChatChannel, _pluginConfig.MessageSettings.UseBotMessageDisplay);
            
            #if RUST
            SetupChannel(MessageSource.Team, _pluginConfig.ChannelSettings.TeamChannel, false);
            SetupChannel(MessageSource.Cards, _pluginConfig.ChannelSettings.CardsChannel, false);
            #endif
            
            if (_pluginConfig.ChannelSettings.ChatChannel.IsValid()
            #if RUST
            || _pluginConfig.ChannelSettings.TeamChannel.IsValid()
            || _pluginConfig.ChannelSettings.CardsChannel.IsValid()
            #endif
            )
            {
                #if RUST
                Subscribe(nameof(OnPlayerChat));
                #else
                Subscribe(nameof(OnUserChat));
                #endif
            }
            
            if (_pluginConfig.ChannelSettings.PlayerStateChannel.IsValid())
            {
                Subscribe(nameof(OnUserConnected));
                Subscribe(nameof(OnUserDisconnected));
            }
            
            if (_pluginConfig.ChannelSettings.ServerStateChannel.IsValid())
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
                    Sends[MessageSource.ServerState]?.SendTemplate(TemplateKeys.Server.Booting, GetDefault());
                }
            });
        }
        
        public void SetupChannel(MessageSource type, Snowflake id, bool wipeNonBotMessages, Action<DiscordMessage> callback = null)
        {
            if (!id.IsValid())
            {
                return;
            }
            
            DiscordChannel channel = Guild.Channels[id];
            if (channel == null)
            {
                PrintWarning($"Channel with ID: '{id}' not found in guild");
                return;
            }
            
            if (callback != null)
            {
                _subscriptions.AddChannelSubscription(this, id, callback);
            }
            
            if (wipeNonBotMessages)
            {
                channel.GetChannelMessages(_client, new ChannelMessagesRequest{Limit = 100}, messages => OnGetChannelMessages(messages, channel));
            }
            
            Sends[type] = new DiscordSendQueue(id, GetTemplateName(type), timer);
            
            Puts($"Setup Channel {type} With ID: {id}");
        }
        
        private void OnGetChannelMessages(List<DiscordMessage> messages, DiscordChannel channel)
        {
            if (messages.Count == 0)
            {
                return;
            }
            
            DiscordMessage[] messagesToDelete = messages
            .Where(m => !CanSendMessage(m.Content, m.Author.Player, m.Author, MessageSource.Server, m))
            .ToArray();
            
            if (messagesToDelete.Length == 0)
            {
                return;
            }
            
            if (messagesToDelete.Length == 1)
            {
                messagesToDelete[0]?.DeleteMessage(_client);
                return;
            }
            
            channel.BulkDeleteMessages(_client, messagesToDelete.Take(100).Select(m => m.Id).ToArray());
        }
        
        public void HandleDiscordChatMessage(DiscordMessage message)
        {
            IPlayer player = message.Author.Player;
            if (Interface.Oxide.CallHook("OnDiscordChatMessage", player, message.Content, message.Author) != null)
            {
                return;
            }
            
            HandleMessage(message.Content, player, message.Author, MessageSource.Discord, message);
            
            if (_pluginConfig.MessageSettings.UseBotMessageDisplay)
            {
                message.DeleteMessage(_client);
            }
        }
        #endregion

        #region Plugins\DiscordChat.Fields.cs
        [PluginReference]
        public Plugin BetterChat;
        
        [DiscordClient]
        #pragma warning disable CS0649
        private DiscordClient _client;
        #pragma warning restore CS0649
        
        private PluginConfig _pluginConfig;
        
        private readonly DiscordSettings _discordSettings = new DiscordSettings
        {
            Intents = GatewayIntents.Guilds | GatewayIntents.GuildMembers | GatewayIntents.GuildMessages | GatewayIntents.MessageContent
        };
        
        private readonly AllowedMention _allowedMention = new AllowedMention
        {
            AllowedTypes = new List<AllowedMentionTypes>(),
            Roles = new List<Snowflake>(),
            Users = new List<Snowflake>()
        };
        
        public DiscordGuild Guild;
        private Snowflake _guildId;
        
        private readonly DiscordSubscriptions _subscriptions = GetLibrary<DiscordSubscriptions>();
        private readonly DiscordPlaceholders _placeholders = GetLibrary<DiscordPlaceholders>();
        private readonly DiscordMessageTemplates _templates = GetLibrary<DiscordMessageTemplates>();
        
        private bool _serverInitCalled;
        
        public readonly Hash<MessageSource, DiscordSendQueue> Sends = new Hash<MessageSource, DiscordSendQueue>();
        private readonly List<IPluginHandler> _plugins = new List<IPluginHandler>();
        
        private const string DiscordSuccess = "43b581";
        private const string DiscordDanger = "f04747";
        private const string DiscordWarning = "FDCB58";
        private const string DiscordBlurple = "5865F2";
        
        public static DiscordChat Instance;
        
        public PluginTimers Timer => timer;
        #endregion

        #region Plugins\DiscordChat.Helpers.cs
        private DateTime GetServerTime()
        {
            return DateTime.Now + TimeSpan.FromHours(_pluginConfig.MessageSettings.ServerTimeOffset);
        }
        
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
            return player.IsConnected ? null : Lang(LangKeys.Server.DiscordTag, player);
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
        private void OnUserConnected(IPlayer player)
        {
            PlaceholderData placeholders = GetDefault().Add(player);
            ProcessPlayerState(LangKeys.Discord.Player.Connected, placeholders);
        }
        
        private void OnUserDisconnected(IPlayer player, string reason)
        {
            PlaceholderData placeholders = GetDefault().Add(player).Add(DisconnectReasonPlaceholder, reason);
            ProcessPlayerState(LangKeys.Discord.Player.Disconnected, placeholders);
        }
        
        public void ProcessPlayerState( string langKey, PlaceholderData data)
        {
            Sends[MessageSource.PlayerState]?.QueueMessage(Lang(langKey, null, data));
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
                _plugins.Add(new AdminChatHandler(_client, this, _pluginConfig.PluginSupport.AdminChat, plugin));
                break;
                
                case "AdminDeepCover":
                _plugins.Add(new AdminDeepCoverHandler(this, plugin));
                break;
                
                case "AntiSpam":
                if (plugin.Version < new VersionNumber(2, 0, 0))
                {
                    PrintError("AntiSpam plugin must be version 2.0.0 or higher");
                    break;
                }
                
                _plugins.Add(new AntiSpamHandler(this, _pluginConfig.PluginSupport.AntiSpam, plugin));
                break;
                
                case "BetterChatMute":
                _plugins.Add(new BetterChatMuteHandler(this, _pluginConfig.PluginSupport.BetterChatMute, plugin));
                break;
                
                case "Clans":
                _plugins.Add(new ClansHandler(this, _pluginConfig.PluginSupport.Clans, plugin));
                break;
                
                case "TranslationAPI":
                _plugins.Add(new TranslationApiHandler(this, _pluginConfig.PluginSupport.ChatTranslator, plugin));
                break;
                
                case "UFilter":
                _plugins.Add(new UFilterHandler(this, _pluginConfig.PluginSupport.UFilter, plugin));
                break;
            }
        }
        
        private void OnPluginUnloaded(Plugin plugin)
        {
            _plugins.RemoveAll(h => h.GetPluginName() == plugin.Name);
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
        public string Lang(string key)
        {
            return lang.GetMessage(key, this);
        }
        
        public string Lang(string key, params object[] args)
        {
            try
            {
                return string.Format(Lang(key), args);
            }
            catch(Exception ex)
            {
                PrintError($"Lang Key '{key}' threw exception\n:{ex}");
                throw;
            }
        }
        
        public string Lang(string key, PlaceholderData data)
        {
            string message = Lang(key);
            if (data != null)
            {
                message = _placeholders.ProcessPlaceholders(message, data);
            }
            
            return message;
        }
        
        protected override void LoadDefaultMessages()
        {
            lang.RegisterMessages(new Dictionary<string, string>
            {
                [LangKeys.Discord.Player.Connected] = ":white_check_mark: ({discordchat.server.time:HH:mm}) **{discordchat.player.name}** has joined.",
                [LangKeys.Discord.Player.Disconnected] = ":x: ({discordchat.server.time:HH:mm}) **{discordchat.player.name}** has disconnected. ({2})",
                [LangKeys.Discord.Chat.Server] = ":desktop: ({discordchat.server.time:HH:mm}) **{discordchat.player.name}**: {discordchat.player.message}",
                [LangKeys.Discord.Chat.LinkedMessage] = ":speech_left: ({discordchat.server.time:HH:mm}) **{discordchat.player.name}**: {discordchat.player.message}",
                [LangKeys.Discord.Chat.UnlinkedMessage] = ":chains: ({discordchat.server.time:HH:mm}) **{user.fullname}**: {discordchat.player.message}",
                [LangKeys.Discord.Team.Message] = ":busts_in_silhouette: ({discordchat.server.time:HH:mm}) **{discordchat.player.name}**: {discordchat.player.message}",
                [LangKeys.Discord.Cards.Message] = ":black_joker: ({discordchat.server.time:HH:mm}) **{discordchat.player.name}**: {discordchat.player.message}",
                [LangKeys.Discord.AdminChat.ServerMessage] = ":mechanic: ({discordchat.server.time:HH:mm}) **{discordchat.player.name}**: {discordchat.player.message}",
                [LangKeys.Discord.AdminChat.DiscordMessage] = ":mechanic: ({discordchat.server.time:HH:mm}) **{discordchat.player.name}**: {discordchat.player.message}",
                [LangKeys.Discord.Clans.ClanMessage] = "({discordchat.server.time:HH:mm}) [Clan] [{discordchat.clan.tag}] **{discordchat.player.message}**: {discordchat.player.message}",
                [LangKeys.Discord.Clans.AllianceMessage] = "({discordchat.server.time:HH:mm}) [Alliance] [{discordchat.clan.tag}] **{discordchat.player.message}**: {discordchat.player.message}",
                [LangKeys.Server.DiscordTag] = "[#5f79d6][Discord][/#]",
                [LangKeys.Server.UnlinkedMessage] = "{discordchat.tag} [#5f79d6]{user.fullname}[/#]: {discordchat.player.message}",
                [LangKeys.Server.LinkedMessage] = "{discordchat.tag} [#5f79d6]{discordchat.player.name}[/#]: {discordchat.player.message}",
                [LangKeys.Server.ClanTag] = "[{0}] "
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
                StringBuilder messageBuilder = DiscordPool.GetStringBuilder(processedMessage);
                
                if (sourceMessage != null)
                {
                    ProcessMentions(sourceMessage, messageBuilder);
                }
                
                ProcessMessage(messageBuilder, player, user, source);
                SendMessage(DiscordPool.FreeStringBuilderToString(messageBuilder), player, user, source, sourceMessage);
            });
        }
        
        public void ProcessMentions(DiscordMessage message, StringBuilder sb)
        {
            if (message.Mentions != null)
            {
                foreach (KeyValuePair<Snowflake, DiscordUser> mention in message.Mentions)
                {
                    sb.Replace($"<@{mention.Key.ToString()}>", $"@{mention.Value.Username}");
                }
                
                foreach (KeyValuePair<Snowflake, DiscordUser> mention in message.Mentions)
                {
                    GuildMember member = Guild.Members[mention.Key];
                    sb.Replace($"<@!{mention.Key.ToString()}>", $"@{member?.Nickname ?? mention.Value.Username}");
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
                DiscordChannel channel = Guild.Channels[id];
                if (channel != null)
                {
                    sb.Replace(value, $"#{channel.Name}");
                }
            }
            
            if (message.MentionRoles != null)
            {
                foreach (Snowflake roleId in message.MentionRoles)
                {
                    DiscordRole role = Guild.Roles[roleId];
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
            for (index = 0; index < _plugins.Count; index++)
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
        
        public string GetPlayerName(IPlayer player)
        {
            StringBuilder name = DiscordPool.GetStringBuilder(player.Name);
            for (int index = 0; index < _plugins.Count; index++)
            {
                _plugins[index].ProcessPlayerName(name, player);
            }
            
            return DiscordPool.FreeStringBuilderToString(name);
        }
        
        public void SendMessage(string message, IPlayer player, DiscordUser user, MessageSource source, DiscordMessage sourceMessage)
        {
            for (int index = 0; index < _plugins.Count; index++)
            {
                if (_plugins[index].SendMessage(message, player, user, source, sourceMessage))
                {
                    return;
                }
            }
        }
        #endregion

        #region Plugins\DiscordChat.Placeholders.cs
        public const string ChatMessagePlaceholder = "discordchat.message";
        public const string PlayerMessagePlaceholder = "discordchat.player.message";
        private const string DisconnectReasonPlaceholder = "discordchat.disconnect.reason";
        private const string ServerTimePlaceholder = "discordchat.server.time";
        private const string ClanTagPlaceholder = "discordchat.clan.tag";
        
        public void RegisterPlaceholders()
        {
            _placeholders.RegisterPlaceholder<string>(this, ChatMessagePlaceholder, ChatMessagePlaceholder, PlaceholderFormatting.Replace);
            _placeholders.RegisterPlaceholder<string>(this, PlayerMessagePlaceholder, PlayerMessagePlaceholder, PlaceholderFormatting.Replace);
            _placeholders.RegisterPlaceholder<string>(this, DisconnectReasonPlaceholder, DisconnectReasonPlaceholder, PlaceholderFormatting.Replace);
            _placeholders.RegisterPlaceholder<string>(this, ClanTagPlaceholder, ClanTagPlaceholder, PlaceholderFormatting.Replace);
            _placeholders.RegisterPlaceholder<IPlayer>(this, "discordchat.player.name", PlayerName);
            _placeholders.RegisterPlaceholder(this, "discordchat.tag", Lang(LangKeys.Server.DiscordTag));
            _placeholders.RegisterPlaceholder<DateTime>(this, ServerTimePlaceholder, ServerTimePlaceholder, ServerTime);
        }
        
        private void PlayerName(StringBuilder builder, PlaceholderState state, IPlayer player) => PlaceholderFormatting.Replace(builder, state, GetPlayerName(player));
        private static void ServerTime(StringBuilder builder, PlaceholderState state, DateTime time) => PlaceholderFormatting.Replace(builder, state, time);
        
        public PlaceholderData GetDefault()
        {
            return _placeholders.CreateData(this).AddGuild(Guild).Add(ServerTimePlaceholder, GetServerTime());
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
            config.ChannelSettings = new ChannelSettings(config.ChannelSettings);
            config.MessageSettings = new MessageSettings(config.MessageSettings);
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
                
                if (_pluginConfig.MessageSettings.EnableServerChatTag)
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
                Sends[MessageSource.ServerState]?.SendTemplate(TemplateKeys.Server.Online, GetDefault());
            }
        }
        
        private void OnServerShutdown()
        {
            Sends[MessageSource.ServerState]?.SendTemplate(TemplateKeys.Server.Shutdown, GetDefault());
        }
        
        private void Unload()
        {
            Instance = null;
        }
        #endregion

        #region Plugins\DiscordChat.Templates.cs
        public void RegisterTemplates()
        {
            DiscordMessageTemplate connected = CreateTemplateEmbed(":white_check_mark: ({discordchat.server.time:HH:mm}) **{discordchat.player.name}** has joined.", DiscordSuccess, new TemplateVersion(1, 0, 0));
            _templates.RegisterGlobalTemplateAsync(this, TemplateKeys.Player.StateChanged, connected, new TemplateVersion(1, 0, 0));
            
            DiscordMessageTemplate online = CreateTemplateEmbed(":green_circle: The server is now online", DiscordSuccess, new TemplateVersion(1, 0, 0));
            _templates.RegisterGlobalTemplateAsync(this, TemplateKeys.Server.Online, online, new TemplateVersion(1, 0, 0));
            
            DiscordMessageTemplate shutdown = CreateTemplateEmbed(":red_circle: The server has shutdown", DiscordDanger, new TemplateVersion(1, 0, 0));
            _templates.RegisterGlobalTemplateAsync(this, TemplateKeys.Server.Shutdown, shutdown, new TemplateVersion(1, 0, 0));
            
            DiscordMessageTemplate booting = CreateTemplateEmbed(":yellow_circle: The server is now booting", DiscordWarning, new TemplateVersion(1, 0, 0));
            _templates.RegisterGlobalTemplateAsync(this, TemplateKeys.Server.Booting, booting, new TemplateVersion(1, 0, 0));
            
            DiscordMessageTemplate serverChat = CreateTemplateEmbed("{discordchat.message}", DiscordBlurple, new TemplateVersion(1, 0, 0));
            _templates.RegisterGlobalTemplateAsync(this, TemplateKeys.Chat.General, serverChat, new TemplateVersion(1, 0, 0));
            
            DiscordMessageTemplate teamChat = CreateTemplateEmbed("{discordchat.message}", DiscordSuccess, new TemplateVersion(1, 0, 0));
            _templates.RegisterGlobalTemplateAsync(this, TemplateKeys.Chat.Teams, teamChat, new TemplateVersion(1, 0, 0));
            
            DiscordMessageTemplate cardsChat = CreateTemplateEmbed("{discordchat.message}", DiscordDanger, new TemplateVersion(1, 0, 0));
            _templates.RegisterGlobalTemplateAsync(this, TemplateKeys.Chat.Cards, cardsChat, new TemplateVersion(1, 0, 0));
            
            DiscordMessageTemplate clanChat = CreateTemplateEmbed("{discordchat.message}", "a1ff46", new TemplateVersion(1, 0, 0));
            _templates.RegisterGlobalTemplateAsync(this, TemplateKeys.Chat.Clans.Clan, clanChat, new TemplateVersion(1, 0, 0));
            
            DiscordMessageTemplate allianceChat = CreateTemplateEmbed("{discordchat.message}", "a1ff46", new TemplateVersion(1, 0, 0));
            _templates.RegisterGlobalTemplateAsync(this, TemplateKeys.Chat.Clans.Alliance, allianceChat, new TemplateVersion(1, 0, 0));
            
            DiscordMessageTemplate errorNotLinked = CreatePrefixedTemplateEmbed("You're not allowed to chat with the server unless you are linked.", DiscordDanger, new TemplateVersion(1, 0, 0));
            _templates.RegisterGlobalTemplateAsync(this, TemplateKeys.Error.NotLinked, errorNotLinked, new TemplateVersion(1, 0, 0));
            
            DiscordMessageTemplate errorAdminChatNotLinked = CreatePrefixedTemplateEmbed("You're not allowed to use admin chat because you have not linked your Discord and game server accounts", DiscordDanger, new TemplateVersion(1, 0, 0));
            _templates.RegisterGlobalTemplateAsync(this, TemplateKeys.Error.AdminChat.NotLinked, errorAdminChatNotLinked, new TemplateVersion(1, 0, 0));
            
            DiscordMessageTemplate errorAdminChatNotPermission = CreatePrefixedTemplateEmbed(":no_entry: You're not allowed to use admin chat channel because you do not have permission.", DiscordDanger, new TemplateVersion(1, 0, 0));
            _templates.RegisterGlobalTemplateAsync(this, TemplateKeys.Error.AdminChat.NoPermission, errorAdminChatNotPermission, new TemplateVersion(1, 0, 0));
        }
        
        public DiscordMessageTemplate CreateTemplateEmbed(string description, string color, TemplateVersion version)
        {
            return new DiscordMessageTemplate
            {
                Embeds = new List<MessageEmbedTemplate>
                {
                    new MessageEmbedTemplate
                    {
                        Description = description,
                        Color = $"#{color}"
                    }
                },
                Version = version
            };
        }
        
        public DiscordMessageTemplate CreatePrefixedTemplateEmbed(string description, string color, TemplateVersion version)
        {
            return new DiscordMessageTemplate
            {
                Embeds = new List<MessageEmbedTemplate>
                {
                    new MessageEmbedTemplate
                    {
                        Description = $"[{{plugin.title}}] {description}",
                        Color = $"#{color}"
                    }
                },
                Version = version
            };
        }
        
        public void SendGlobalTemplateMessage(string templateName, Snowflake channelId, PlaceholderData placeholders = null)
        {
            MessageCreate create = new MessageCreate
            {
                AllowedMention = _allowedMention
            };
            DiscordChannel channel = Guild.Channels[channelId];
            channel?.CreateGlobalTemplateMessage(_client, this, templateName, create, placeholders);
        }
        
        public string GetTemplateName(MessageSource type)
        {
            switch (type)
            {
                case MessageSource.Discord:
                case MessageSource.Server:
                return TemplateKeys.Chat.General;
                case MessageSource.Team:
                return TemplateKeys.Chat.Teams;
                case MessageSource.Cards:
                return TemplateKeys.Chat.Cards;
                case MessageSource.PlayerState:
                return TemplateKeys.Player.StateChanged;
                case MessageSource.AdminChat:
                return TemplateKeys.Chat.AdminChat.Message;
                case MessageSource.ClanChat:
                return TemplateKeys.Chat.Clans.Clan;
                case MessageSource.AllianceChat:
                return TemplateKeys.Chat.Clans.Alliance;
            }
            
            return null;
        }
        #endregion

        #region Configuration\ChannelSettings.cs
        public class ChannelSettings
        {
            [JsonProperty("Chat Channel ID")]
            public Snowflake ChatChannel { get; set; }
            
            #if RUST
            [JsonProperty("Team Channel ID")]
            public Snowflake TeamChannel { get; set; }
            
            [JsonProperty("Cards Channel ID")]
            public Snowflake CardsChannel { get; set; }
            #endif
            
            [JsonProperty("Player State Channel ID")]
            public Snowflake PlayerStateChannel { get; set; }
            
            [JsonProperty("Server State Channel ID")]
            public Snowflake ServerStateChannel { get; set; }
            
            public ChannelSettings(ChannelSettings settings)
            {
                ChatChannel = settings?.ChatChannel ?? default(Snowflake);
                PlayerStateChannel = settings?.PlayerStateChannel ?? default(Snowflake);
                ServerStateChannel = settings?.ServerStateChannel ?? default(Snowflake);
                #if RUST
                TeamChannel = settings?.TeamChannel ?? default(Snowflake);
                CardsChannel = settings?.CardsChannel ?? default(Snowflake);
                #endif
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

        #region Configuration\MessageSettings.cs
        public class MessageSettings
        {
            [JsonProperty("Replace Discord User Message With Bot Message")]
            public bool UseBotMessageDisplay { get; set; }
            
            [JsonProperty("Send Messages From Server Chat To Discord Channel")]
            public bool ServerToDiscord { get; set; }
            
            [JsonProperty("Send Messages From Discord Channel To Server Chat")]
            public bool DiscordToServer { get; set; }
            
            [JsonProperty("Enable Adding Discord Tag To In Game Messages When Sent From Discord")]
            public bool EnableServerChatTag { get; set; } = true;
            
            [JsonProperty("Allow plugins to process Discord to Server Chat Messages")]
            public bool AllowPluginProcessing { get; set; }
            
            [JsonProperty("Discord Message Server Time Offset (Hours)")]
            public float ServerTimeOffset { get; set; }
            
            [JsonProperty("Max Characters Before Sending Message")]
            public int MaxCharactersPerMessage { get; set; }
            
            [JsonProperty("Text Replacements")]
            public Hash<string, string> TextReplacements { get; set; }
            
            [JsonProperty("Unlinked Settings")]
            public UnlinkedSettings UnlinkedSettings { get; set; }
            
            [JsonProperty("Message Filter Settings")]
            public MessageFilterSettings Filter { get; set; }
            
            public MessageSettings(MessageSettings settings)
            {
                UseBotMessageDisplay = settings?.UseBotMessageDisplay ?? true;
                ServerToDiscord = settings?.ServerToDiscord ?? true;
                DiscordToServer = settings?.DiscordToServer ?? true;
                EnableServerChatTag = settings?.EnableServerChatTag ?? true;
                AllowPluginProcessing = settings?.AllowPluginProcessing ?? true;
                ServerTimeOffset = settings?.ServerTimeOffset ?? 0f;
                MaxCharactersPerMessage = settings?.MaxCharactersPerMessage ?? 0;
                if (MaxCharactersPerMessage == 0)
                {
                    MaxCharactersPerMessage = 4000;
                }
                TextReplacements = settings?.TextReplacements ?? new Hash<string, string> { ["TextToBeReplaced"] = "ReplacedText" };
                UnlinkedSettings = new UnlinkedSettings(settings?.UnlinkedSettings);
                Filter = new MessageFilterSettings(settings?.Filter);
            }
        }
        #endregion

        #region Configuration\PluginConfig.cs
        public class PluginConfig
        {
            [JsonProperty(PropertyName = "Discord Bot Token")]
            public string DiscordApiKey { get; set; } = string.Empty;
            
            [JsonProperty(PropertyName = "Discord Server ID (Optional if bot only in 1 guild)")]
            public Snowflake GuildId { get; set; }
            
            [JsonProperty("Channel Settings")]
            public ChannelSettings ChannelSettings { get; set; }
            
            [JsonProperty("Message Settings")]
            public MessageSettings MessageSettings { get; set; }
            
            [JsonProperty("Plugin Support")]
            public PluginSupport PluginSupport { get; set; }
            
            [JsonConverter(typeof(StringEnumConverter))]
            [DefaultValue(DiscordLogLevel.Info)]
            [JsonProperty(PropertyName = "Discord Extension Log Level (Verbose, Debug, Info, Warning, Error, Exception, Off)")]
            public DiscordLogLevel ExtensionDebugging { get; set; } = DiscordLogLevel.Info;
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
        public enum MessageSource
        {
            Discord,
            Server,
            ServerState,
            PlayerState,
            Team,
            Cards,
            AdminChat,
            ClanChat,
            AllianceChat
        }
        #endregion

        #region Helpers\DiscordSendQueue.cs
        public class DiscordSendQueue
        {
            private readonly StringBuilder _message = new StringBuilder();
            private Timer _sendTimer;
            private readonly Snowflake _channelId;
            private readonly string _templateId;
            private readonly Action _callback;
            private readonly PluginTimers _timer;
            
            public DiscordSendQueue(Snowflake channelId, string templateId, PluginTimers timers)
            {
                _channelId = channelId;
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
                DiscordChat.Instance.SendGlobalTemplateMessage(templateId, _channelId, data);
            }
            
            public void Send()
            {
                if (_message.Length > 2000)
                {
                    _message.Length = 2000;
                }
                
                PlaceholderData placeholders = DiscordChat.Instance.GetDefault().Add(DiscordChat.ChatMessagePlaceholder, _message.ToString());
                _message.Length = 0;
                DiscordChat.Instance.SendGlobalTemplateMessage(_templateId, _channelId, placeholders);
                _sendTimer?.Destroy();
                _sendTimer = null;
            }
        }
        #endregion

        #region Localization\LangKeys.cs
        public static class LangKeys
        {
            public const string Root = "V2.";
            
            public static class Discord
            {
                private const string Base = Root + nameof(Discord) + ".";
                
                //public const string NotLinkedError = Base + nameof(NotLinkedError);
                
                public static class Chat
                {
                    private const string Base = Discord.Base + nameof(Chat) + ".";
                    
                    public const string Server = Base + nameof(Server);
                    public const string LinkedMessage = Base + nameof(LinkedMessage);
                    public const string UnlinkedMessage = Base + nameof(UnlinkedMessage);
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
                    
                    public const string Connected = Base + nameof(Connected);
                    public const string Disconnected = Base + nameof(Disconnected);
                }
                
                // public static class OnlineOffline
                // {
                    //     private const string Base = Discord.Base + nameof(OnlineOffline) + ".";
                    //
                    //     public const string OnlineMessage = Base + nameof(OnlineMessage);
                    //     public const string OfflineMessage = Base + nameof(OfflineMessage);
                    //     public const string BootingMessage = Base + nameof(BootingMessage);
                // }
                
                public static class AdminChat
                {
                    private const string Base = Discord.Base + nameof(AdminChat) + ".";
                    
                    public const string ServerMessage = Base + nameof(ServerMessage);
                    public const string DiscordMessage = Base + nameof(DiscordMessage);
                    //public const string Permission = Base + nameof(Permission);
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
                public const string DiscordTag = Base + nameof(DiscordTag);
                public const string ClanTag = Base + nameof(ClanTag);
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
                
                PlaceholderData placeholders = Chat.GetDefault().Add(player).Add(DiscordChat.PlayerMessagePlaceholder, message);
                
                if (sourceMessage != null)
                {
                    if (_settings.ReplaceWithBot)
                    {
                        Chat.Timer.In(.25f, () => { sourceMessage.DeleteMessage(_client); });
                        Chat.Sends[source]?.QueueMessage(Chat.Lang(LangKeys.Discord.AdminChat.DiscordMessage, null, placeholders));
                    }
                    
                    Plugin.Call("SendAdminMessage", player, message);
                }
                else
                {
                    Chat.Sends[source]?.QueueMessage(Chat.Lang(LangKeys.Discord.AdminChat.ServerMessage, null, placeholders));
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
            
            public override bool CanSendMessage(string message, IPlayer player, DiscordUser user, MessageSource source, DiscordMessage sourceMessage)
            {
                return player.Object != null
                && (source == MessageSource.Discord || source == MessageSource.Server)
                && Plugin.Call<bool>("API_IsDeepCovered", player.Object);
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
                
                name.Length = 0;
                name.Append(Plugin.Call<string>("GetClearName", player));
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
                    case MessageSource.ClanChat:
                    case MessageSource.AllianceChat:
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
            private readonly string PluginName;
            
            public BasePluginHandler(DiscordChat chat, Plugin plugin)
            {
                Chat = chat;
                Plugin = plugin;
                PluginName = plugin.Name;
            }
            
            public virtual bool CanSendMessage(string message, IPlayer player, DiscordUser user, MessageSource source, DiscordMessage sourceMessage)
            {
                return true;
            }
            
            public virtual void ProcessPlayerName(StringBuilder name, IPlayer player)
            {
                
            }
            
            public virtual bool HasCallbackMessage()
            {
                return false;
            }
            
            public virtual void ProcessCallbackMessage(string message, IPlayer player, DiscordUser user, MessageSource source, Action<string> callback)
            {
                
            }
            
            public virtual void ProcessMessage(StringBuilder message, IPlayer player, DiscordUser user, MessageSource source)
            {
                
            }
            
            public virtual bool SendMessage(string message, IPlayer player, DiscordUser user, MessageSource source, DiscordMessage sourceMessage)
            {
                return false;
            }
            
            public string GetPluginName()
            {
                return PluginName;
            }
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
                return !_settings.IgnoreMuted || !Plugin.Call<bool>("API_IsMuted", player);
            }
        }
        #endregion

        #region PluginHandlers\ClansHandler.cs
        public class ClansHandler : BasePluginHandler
        {
            private readonly ClansSettings _settings;
            
            public ClansHandler(DiscordChat chat, ClansSettings settings, Plugin plugin) : base(chat, plugin)
            {
                _settings = settings;
            }
            
            public override void ProcessPlayerName(StringBuilder name, IPlayer player)
            {
                if (!_settings.ShowClanTag || player == null)
                {
                    return;
                }
                
                string clanTag = Plugin.Call<string>("GetClanOf", player.Id);
                if (!string.IsNullOrEmpty(clanTag))
                {
                    name.Insert(0, DiscordChat.Instance.Lang(LangKeys.Server.ClanTag, player, clanTag));
                }
            }
        }
        #endregion

        #region PluginHandlers\DiscordChatHandler.cs
        public class DiscordChatHandler : BasePluginHandler
        {
            private readonly MessageSettings _settings;
            private readonly IServer _server;
            
            public DiscordChatHandler(DiscordChat chat, MessageSettings settings, Plugin plugin, IServer server) : base(chat, plugin)
            {
                _settings = settings;
                _server = server;
            }
            
            public override bool CanSendMessage(string message, IPlayer player, DiscordUser user, MessageSource source, DiscordMessage sourceMessage)
            {
                if (sourceMessage != null)
                {
                    if (_settings.Filter.IgnoreMessage(sourceMessage, Chat.Guild.Members[sourceMessage.Author.Id]))
                    {
                        return false;
                    }
                }
                
                switch (source)
                {
                    case MessageSource.Discord:
                    return _settings.DiscordToServer && (!_settings.UnlinkedSettings.AllowedUnlinked || player.IsLinked());
                    
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
                    if (_settings.UseBotMessageDisplay)
                    {
                        if (player.IsLinked())
                        {
                            Chat.Sends[MessageSource.Discord].QueueMessage(Chat.Lang(LangKeys.Discord.Chat.LinkedMessage, GetPlaceholders(message, player, user, sourceMessage)));
                        }
                        else
                        {
                            Chat.Sends[MessageSource.Discord].QueueMessage(Chat.Lang(LangKeys.Discord.Chat.UnlinkedMessage, GetPlaceholders(message, player, user, sourceMessage)));
                        }
                    }
                    
                    if (player.IsLinked())
                    {
                        SendLinkedToServer(player, message);
                    }
                    else
                    {
                        SendUnlinkedToServer(sourceMessage, message);
                    }
                    
                    return false;
                    
                    case MessageSource.Server:
                    Chat.Sends[MessageSource.Discord].QueueMessage(Chat.Lang(LangKeys.Discord.Chat.Server, GetPlaceholders(message, player, user, sourceMessage)));
                    return false;
                    case MessageSource.Team:
                    Chat.Sends[MessageSource.Team].QueueMessage(Chat.Lang(LangKeys.Discord.Team.Message, GetPlaceholders(message, player, user, sourceMessage)));
                    return false;
                    case MessageSource.Cards:
                    Chat.Sends[MessageSource.Cards].QueueMessage(Chat.Lang(LangKeys.Discord.Cards.Message, GetPlaceholders(message, player, user, sourceMessage)));
                    return false;
                    case MessageSource.AdminChat:
                    Chat.Sends[MessageSource.AdminChat].QueueMessage(Chat.Lang(LangKeys.Discord.AdminChat.DiscordMessage, GetPlaceholders(message, player, user, sourceMessage)));
                    return false;
                    case MessageSource.ClanChat:
                    Chat.Sends[MessageSource.ClanChat].QueueMessage(Chat.Lang(LangKeys.Discord.Clans.ClanMessage, GetPlaceholders(message, player, user, sourceMessage)));
                    return false;
                    case MessageSource.AllianceChat:
                    Chat.Sends[MessageSource.AllianceChat].QueueMessage(Chat.Lang(LangKeys.Discord.Clans.AllianceMessage, GetPlaceholders(message, player, user, sourceMessage)));
                    return false;
                }
                
                return true;
            }
            
            public void SendLinkedToServer(IPlayer player, string message)
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
                
                string discordTag = string.Empty;
                if (_settings.EnableServerChatTag)
                {
                    discordTag = Chat.Lang(LangKeys.Server.DiscordTag, player);
                }
                
                message = Chat.Lang(LangKeys.Server.LinkedMessage, player, discordTag, player.Name, message);
                _server.Broadcast(message);
                Chat.Puts(Formatter.ToPlaintext(message));
            }
            
            public void SendUnlinkedToServer(DiscordMessage sourceMessage, string message)
            {
                if (_settings.UseBotMessageDisplay)
                {
                    Chat.Sends[MessageSource.Server]?.QueueMessage(Chat.Lang(LangKeys.Discord.Chat.UnlinkedMessage, Chat.GetDefault().Add(sourceMessage).Add(DiscordChat.PlayerMessagePlaceholder, message)));
                }
                
                string serverMessage = Chat.Lang(LangKeys.Server.UnlinkedMessage, Chat.GetDefault().Add(sourceMessage).AddGuildMember(Chat.Guild.Members[sourceMessage.Author.Id]).Add(DiscordChat.PlayerMessagePlaceholder, message));
                _server.Broadcast(serverMessage);
                Chat.Puts(Formatter.ToPlaintext(serverMessage));
            }
            
            private PlaceholderData GetPlaceholders(string message, IPlayer player, DiscordUser user, DiscordMessage sourceMessage)
            {
                return Chat.GetDefault().Add(player).Add(user).Add(sourceMessage).Add(DiscordChat.PlayerMessagePlaceholder, message);
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
                    Plugin.Call("Translate", message, _settings.DiscordServerLanguage, "auto", new Action<string>(callback.Invoke));
                    return;
                }
                
                callback.Invoke(message);
            }
            
            public bool CanChatTranslatorSource(MessageSource type)
            {
                if (!_settings.Enabled)
                {
                    return false;
                }
                
                switch (type)
                {
                    case MessageSource.Server:
                    return _settings.ServerMessage;
                    
                    case MessageSource.Discord:
                    return _settings.DiscordMessage;
                    
                    case MessageSource.ClanChat:
                    case MessageSource.AllianceChat:
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
                    case MessageSource.ClanChat:
                    case MessageSource.AllianceChat:
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
                
                public const string StateChanged = Base + nameof(StateChanged);
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
            
            public void Disable()
            {
                PlayerName = false;
                ServerMessage = false;
                DiscordMessage = false;
                #if RUST
                TeamMessage = false;
                CardMessages = false;
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
            [JsonProperty("Display Clan Tag")]
            public bool ShowClanTag { get; set; }
            
            [JsonProperty("Clans Chat Channel ID")]
            public Snowflake ClansChatChannel { get; set; }
            
            [JsonProperty("Alliance Chat Channel ID")]
            public Snowflake AllianceChatChannel { get; set; }
            
            public ClansSettings(ClansSettings settings)
            {
                ShowClanTag = settings?.ShowClanTag ?? true;
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
