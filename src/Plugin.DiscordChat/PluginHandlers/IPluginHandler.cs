using System;
using System.Text;
using DiscordChatPlugin.Enums;
using Oxide.Core.Libraries.Covalence;
using Oxide.Ext.Discord.Entities.Messages;
using Oxide.Ext.Discord.Entities.Users;
using MessageType = DiscordChatPlugin.Enums.MessageType;

namespace DiscordChatPlugin.PluginHandlers
{
    public interface IPluginHandler
    {
        bool CanSendMessage(string message, IPlayer player, DiscordUser user, MessageType type, DiscordMessage sourceMessage);
        void ProcessPlayerName(StringBuilder name, IPlayer player);
        bool HasCallbackMessage();
        void ProcessCallbackMessage(string message, IPlayer player, DiscordUser user, MessageType type, Action<string> callback);
        void ProcessMessage(StringBuilder message, IPlayer player, DiscordUser user, MessageType type);
        bool SendMessage(string message, IPlayer player, DiscordUser user, MessageType type, DiscordMessage sourceMessage);
        string GetPluginName();
    }
}