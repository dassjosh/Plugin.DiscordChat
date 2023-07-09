using System;
using System.Text;
using DiscordChatPlugin.Enums;
using Oxide.Core.Libraries.Covalence;
using Oxide.Ext.Discord.Entities.Messages;
using Oxide.Ext.Discord.Entities.Users;

namespace DiscordChatPlugin.PluginHandlers
{
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
}