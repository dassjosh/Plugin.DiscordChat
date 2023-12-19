using System;
using System.Text;
using DiscordChatPlugin.Enums;
using DiscordChatPlugin.Plugins;
using Oxide.Core.Libraries.Covalence;
using Oxide.Core.Plugins;
using Oxide.Ext.Discord.Entities;
using Oxide.Ext.Discord.Libraries;

namespace DiscordChatPlugin.PluginHandlers
{
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

        public virtual bool SendMessage(string message, IPlayer player, DiscordUser user, MessageSource source, DiscordMessage sourceMessage, PlaceholderData data) => false;

        public string GetPluginName() => _pluginName;
    }
}