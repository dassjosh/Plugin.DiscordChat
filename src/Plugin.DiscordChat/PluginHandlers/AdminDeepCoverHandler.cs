using DiscordChatPlugin.Enums;
using DiscordChatPlugin.Plugins;
using Oxide.Core.Libraries.Covalence;
using Oxide.Core.Plugins;
using Oxide.Ext.Discord.Entities;

namespace DiscordChatPlugin.PluginHandlers
{
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
}