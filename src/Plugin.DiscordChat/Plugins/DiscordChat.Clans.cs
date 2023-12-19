using DiscordChatPlugin.Enums;
using DiscordChatPlugin.Localization;
using DiscordChatPlugin.Placeholders;
using Oxide.Core.Libraries.Covalence;
using Oxide.Ext.Discord.Libraries;

namespace DiscordChatPlugin.Plugins
{
    public partial class DiscordChat
    {
        private void OnClanChat(IPlayer player, string message)
        {
            Sends[MessageSource.PluginClan]?.QueueMessage(Lang(LangKeys.Discord.PluginClans.ClanMessage, GetClanPlaceholders(player, message)));
        }
        
        private void OnAllianceChat(IPlayer player, string message)
        {
            Sends[MessageSource.PluginAlliance]?.QueueMessage(Lang(LangKeys.Discord.PluginClans.AllianceMessage, GetClanPlaceholders(player, message)));
        }

        public PlaceholderData GetClanPlaceholders(IPlayer player, string message)
        {
            return GetDefault().AddPlayer(player).Add(PlaceholderDataKeys.PlayerMessage, message);
        }
    }
}