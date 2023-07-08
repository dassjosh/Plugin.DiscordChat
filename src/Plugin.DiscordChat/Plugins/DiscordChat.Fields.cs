using System.Collections.Generic;
using DiscordChatPlugin.Configuration;
using DiscordChatPlugin.Enums;
using DiscordChatPlugin.Helpers;
using DiscordChatPlugin.PluginHandlers;
using Oxide.Core.Plugins;
using Oxide.Ext.Discord.Attributes.Pooling;
using Oxide.Ext.Discord.Clients;
using Oxide.Ext.Discord.Connections;
using Oxide.Ext.Discord.Entities;
using Oxide.Ext.Discord.Entities.Gateway;
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
        private readonly Hash<string, string> _flagCache = new Hash<string, string>();
        private readonly List<IPluginHandler> _plugins = new List<IPluginHandler>();

        public static DiscordChat Instance;

        private readonly object _true = true;
    }
}