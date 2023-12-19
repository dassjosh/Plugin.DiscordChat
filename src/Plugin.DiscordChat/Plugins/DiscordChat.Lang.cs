using System.Collections.Generic;
using DiscordChatPlugin.Localization;
using DiscordChatPlugin.Placeholders;
using Oxide.Ext.Discord.Libraries;

namespace DiscordChatPlugin.Plugins
{
    public partial class DiscordChat
    {
        public string Lang(string key, PlaceholderData data)
        {
            string message = lang.GetMessage(key, this);
            message = _placeholders.ProcessPlaceholders(message, data);
            return message;
        }
        
        protected override void LoadDefaultMessages()
        {
            lang.RegisterMessages(new Dictionary<string, string>
            {
                [LangKeys.Discord.Player.Connecting] = $":yellow_circle: {DefaultKeys.TimestampNow.ShortTime} **{PlaceholderKeys.PlayerName}** is connecting",
                [LangKeys.Discord.Player.Connected] = $":white_check_mark: {DefaultKeys.TimestampNow.ShortTime} {DefaultKeys.Player.CountryEmoji} **{PlaceholderKeys.PlayerName}** has joined.",
                [LangKeys.Discord.Player.Disconnected] = $":x: {DefaultKeys.TimestampNow.ShortTime} **{PlaceholderKeys.PlayerName}** has disconnected. ({PlaceholderKeys.DisconnectReason})",
                [LangKeys.Discord.Chat.Server] = $":desktop: {DefaultKeys.TimestampNow.ShortTime} **{PlaceholderKeys.PlayerName}**: {PlaceholderKeys.PlayerMessage}",
                [LangKeys.Discord.Chat.LinkedMessage] = $":speech_left: {DefaultKeys.TimestampNow.ShortTime} **{PlaceholderKeys.PlayerName}**: {PlaceholderKeys.PlayerMessage}",
                [LangKeys.Discord.Chat.UnlinkedMessage] = $":chains: {DefaultKeys.TimestampNow.ShortTime} {DefaultKeys.User.Mention}: {PlaceholderKeys.PlayerMessage}",
                [LangKeys.Discord.Chat.PlayerName] = $"{DefaultKeys.Player.NameClan}",
                [LangKeys.Discord.Team.Message] = $":busts_in_silhouette: {DefaultKeys.TimestampNow.ShortTime} **{PlaceholderKeys.PlayerName}**: {PlaceholderKeys.PlayerMessage}",
                [LangKeys.Discord.Cards.Message] = $":black_joker: {DefaultKeys.TimestampNow.ShortTime} **{PlaceholderKeys.PlayerName}**: {PlaceholderKeys.PlayerMessage}",
                [LangKeys.Discord.Clans.Message] = $":shield: {DefaultKeys.TimestampNow.ShortTime} **{PlaceholderKeys.PlayerName}**: {PlaceholderKeys.PlayerMessage}",
                [LangKeys.Discord.AdminChat.ServerMessage] = $":mechanic: {DefaultKeys.TimestampNow.ShortTime} **{PlaceholderKeys.PlayerName}**: {PlaceholderKeys.PlayerMessage}",
                [LangKeys.Discord.AdminChat.DiscordMessage] = $":mechanic: {DefaultKeys.TimestampNow.ShortTime} **{PlaceholderKeys.PlayerName}**: {PlaceholderKeys.PlayerMessage}",
                [LangKeys.Discord.PluginClans.ClanMessage] = $"{DefaultKeys.TimestampNow.ShortTime} [Clan] **{PlaceholderKeys.PlayerName}**: {PlaceholderKeys.PlayerMessage}",
                [LangKeys.Discord.PluginClans.AllianceMessage] = $"{DefaultKeys.TimestampNow.ShortTime} [Alliance] **{PlaceholderKeys.PlayerName}**: {PlaceholderKeys.PlayerMessage}",
                [LangKeys.Server.UnlinkedMessage] = $"{PlaceholderKeys.DiscordTag} [#5f79d6]{DefaultKeys.Member.Name}[/#]: {PlaceholderKeys.PlayerMessage}",
                [LangKeys.Server.LinkedMessage] = $"{PlaceholderKeys.DiscordTag} [#5f79d6]{PlaceholderKeys.PlayerName}[/#]: {PlaceholderKeys.PlayerMessage}"
            }, this);
        }
    }
}