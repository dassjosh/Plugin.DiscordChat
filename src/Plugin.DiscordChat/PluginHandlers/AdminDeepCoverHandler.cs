using DiscordChatPlugin.Enums;
using DiscordChatPlugin.Plugins;
using Oxide.Core.Libraries.Covalence;
using Oxide.Core.Plugins;
using Oxide.Ext.Discord.Entities.Messages;
using Oxide.Ext.Discord.Entities.Users;
using MessageType = DiscordChatPlugin.Enums.MessageType;

namespace DiscordChatPlugin.PluginHandlers
{
    public class AdminDeepCoverHandler : BasePluginHandler
    {
        public AdminDeepCoverHandler(DiscordChat chat, Plugin plugin) : base(chat, plugin) { }

        public override bool CanSendMessage(string message, IPlayer player, DiscordUser user, MessageType type, DiscordMessage sourceMessage)
        {
            return player?.Object != null
                   && (type == MessageType.Discord || type == MessageType.Server)
                   && !Plugin.Call<bool>("API_IsDeepCovered", player.Object);
        }
    }
}