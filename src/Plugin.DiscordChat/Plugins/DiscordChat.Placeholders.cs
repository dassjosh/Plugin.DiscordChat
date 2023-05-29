using System;
using System.Text;
using DiscordChatPlugin.Localization;
using Oxide.Core.Libraries.Covalence;
using Oxide.Ext.Discord.Libraries.Placeholders;

namespace DiscordChatPlugin.Plugins
{
    public partial class DiscordChat
    {
        public const string ChatMessagePlaceholder = "discordchat.message";
        public const string PlayerMessagePlaceholder = "discordchat.player.message";
        private const string DisconnectReasonPlaceholder = "discordchat.disconnect.reason";
        private const string ServerTimePlaceholder = "discordchat.server.time";
        private const string ClanTagPlaceholder = "discordchat.clan.tag";

        public void RegisterPlaceholders()
        {
            _placeholders.RegisterPlaceholder<string>(this, ChatMessagePlaceholder, ChatMessagePlaceholder, PlaceholderFormatting.Replace);
            _placeholders.RegisterPlaceholder<string>(this, PlayerMessagePlaceholder, PlayerMessagePlaceholder, PlaceholderFormatting.Replace);
            _placeholders.RegisterPlaceholder<string>(this, DisconnectReasonPlaceholder, DisconnectReasonPlaceholder, PlaceholderFormatting.Replace);
            _placeholders.RegisterPlaceholder<string>(this, ClanTagPlaceholder, ClanTagPlaceholder, PlaceholderFormatting.Replace);
            _placeholders.RegisterPlaceholder<IPlayer>(this, "discordchat.player.name", PlayerName);
            _placeholders.RegisterPlaceholder(this, "discordchat.tag", Lang(LangKeys.Server.DiscordTag));
            _placeholders.RegisterPlaceholder<DateTime>(this, ServerTimePlaceholder, ServerTimePlaceholder, ServerTime);
        }
        
        private void PlayerName(StringBuilder builder, PlaceholderState state, IPlayer player) => PlaceholderFormatting.Replace(builder, state, GetPlayerName(player));
        private static void ServerTime(StringBuilder builder, PlaceholderState state, DateTime time) => PlaceholderFormatting.Replace(builder, state, time);
        
        public PlaceholderData GetDefault()
        {
            return _placeholders.CreateData(this).AddGuild(Guild).Add(ServerTimePlaceholder, GetServerTime());
        }
    }
}