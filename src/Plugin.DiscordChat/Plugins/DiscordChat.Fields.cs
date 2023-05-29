using System.Collections.Generic;
using DiscordChatPlugin.Configuration;
using DiscordChatPlugin.Enums;
using DiscordChatPlugin.Helpers;
using DiscordChatPlugin.PluginHandlers;
using Oxide.Core.Plugins;
using Oxide.Ext.Discord;
using Oxide.Ext.Discord.Entities;
using Oxide.Ext.Discord.Entities.Gateway;
using Oxide.Ext.Discord.Entities.Guilds;
using Oxide.Ext.Discord.Entities.Messages.AllowedMentions;
using Oxide.Ext.Discord.Libraries.Placeholders;
using Oxide.Ext.Discord.Libraries.Subscription;
using Oxide.Ext.Discord.Libraries.Templates.Messages;
using Oxide.Ext.Discord.Pooling;
using Oxide.Plugins;

namespace DiscordChatPlugin.Plugins
{
    public partial class DiscordChat
    {
        [PluginReference]
        public Plugin BetterChat;
        
        public DiscordClient Client { get; set; }

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
        private DiscordPluginPool _pool;

        private bool _serverInitCalled;
        
        public readonly Hash<MessageSource, DiscordSendQueue> Sends = new Hash<MessageSource, DiscordSendQueue>();
        private readonly List<IPluginHandler> _plugins = new List<IPluginHandler>();

        private const string DiscordSuccess = "43b581";
        private const string DiscordDanger = "f04747";
        private const string DiscordWarning = "FDCB58";
        private const string DiscordBlurple = "5865F2";
        
        public static DiscordChat Instance;

        public PluginTimers Timer => timer;
    }
}