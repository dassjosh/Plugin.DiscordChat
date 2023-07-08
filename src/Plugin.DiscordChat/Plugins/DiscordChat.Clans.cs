using DiscordChatPlugin.Enums;
using DiscordChatPlugin.Localization;
using DiscordChatPlugin.Placeholders;
using Oxide.Core.Libraries.Covalence;
using Oxide.Ext.Discord.Libraries.Placeholders;

namespace DiscordChatPlugin.Plugins
{
    public partial class DiscordChat
    {
        private void OnClanChat(IPlayer player, string message)
        {
            Sends[MessageType.Clan]?.QueueMessage(Lang(LangKeys.Discord.Clans.ClanMessage, GetClanPlaceholders(player, message)));
        }
        
        private void OnAllianceChat(IPlayer player, string message)
        {
            Sends[MessageType.Alliance]?.QueueMessage(Lang(LangKeys.Discord.Clans.AllianceMessage, GetClanPlaceholders(player, message)));
        }

        public PlaceholderData GetClanPlaceholders(IPlayer player, string message)
        {
            return GetDefault().AddPlayer(player).Add(PlaceholderKeys.Data.PlayerMessage, message);
        }
    }
}