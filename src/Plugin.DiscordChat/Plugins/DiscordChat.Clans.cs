using DiscordChatPlugin.Enums;
using DiscordChatPlugin.Localization;
using Oxide.Core.Libraries.Covalence;
using Oxide.Ext.Discord.Libraries.Placeholders;

namespace DiscordChatPlugin.Plugins
{
    public partial class DiscordChat
    {
        private void OnClanChat(IPlayer player, string message, string tag)
        {
            Sends[MessageSource.ClanChat]?.QueueMessage(Lang(LangKeys.Discord.Clans.ClanMessage, GetClanPlaceholders(player, message, tag)));
        }
        
        private void OnAllianceChat(IPlayer player, string message, string tag)
        {
            Sends[MessageSource.AllianceChat]?.QueueMessage(Lang(LangKeys.Discord.Clans.AllianceMessage, GetClanPlaceholders(player, message, tag)));
        }

        public PlaceholderData GetClanPlaceholders(IPlayer player, string message, string tag)
        {
            return GetDefault().AddPlayer(player).Add(PlayerMessagePlaceholder, message).Add(ClanTagPlaceholder, tag);
        }
    }
}