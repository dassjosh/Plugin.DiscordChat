using System;
using System.Text;
using DiscordChatPlugin.Enums;
using DiscordChatPlugin.Plugins;
using Oxide.Core.Libraries.Covalence;
using Oxide.Core.Plugins;
using Oxide.Ext.Discord.Entities.Messages;
using Oxide.Ext.Discord.Entities.Users;
using MessageType = DiscordChatPlugin.Enums.MessageType;

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

        public virtual bool CanSendMessage(string message, IPlayer player, DiscordUser user, MessageType type, DiscordMessage sourceMessage) => true;

        public virtual void ProcessPlayerName(StringBuilder name, IPlayer player) { }

        public virtual bool HasCallbackMessage() => false;

        public virtual void ProcessCallbackMessage(string message, IPlayer player, DiscordUser user, MessageType type, Action<string> callback) { }

        public virtual void ProcessMessage(StringBuilder message, IPlayer player, DiscordUser user, MessageType type) { }

        public virtual bool SendMessage(string message, IPlayer player, DiscordUser user, MessageType type, DiscordMessage sourceMessage) => false;

        public string GetPluginName() => _pluginName;
    }
}