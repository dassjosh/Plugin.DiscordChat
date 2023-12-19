using DiscordChatPlugin.Configuration.Plugins;
using DiscordChatPlugin.Enums;
using DiscordChatPlugin.Plugins;
using Oxide.Core.Libraries.Covalence;
using Oxide.Core.Plugins;
using Oxide.Ext.Discord.Entities;

namespace DiscordChatPlugin.PluginHandlers
{
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
}