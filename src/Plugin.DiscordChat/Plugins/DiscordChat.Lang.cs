using System.Collections.Generic;
using DiscordChatPlugin.Localization;
using DiscordChatPlugin.Placeholders;
using Oxide.Ext.Discord.Libraries.Placeholders;

namespace DiscordChatPlugin.Plugins
{
    public partial class DiscordChat
    {
        public string Lang(string key, PlaceholderData data)
        {
            string message = lang.GetMessage(key, this);
            if (data != null)
            {
                message = _placeholders.ProcessPlaceholders(message, data);
            }
            
            return message;
        }
        
        protected override void LoadDefaultMessages()
        {
            lang.RegisterMessages(new Dictionary<string, string>
            {
                [LangKeys.Discord.Player.Connecting] = $":yellow_circle: {{timestamp.now.shorttime}} **{{{PlaceholderKeys.PlayerName}}}** is connecting",
                [LangKeys.Discord.Player.Connected] = $":white_check_mark: {{timestamp.now.shorttime}} **{{{PlaceholderKeys.PlayerName}}}** {{player.country.emoji}} has joined.",
                [LangKeys.Discord.Player.Disconnected] = $":x: {{timestamp.now.shorttime}} **{{{PlaceholderKeys.PlayerName}}}** has disconnected. ({{{PlaceholderKeys.DisconnectReason}}})",
                [LangKeys.Discord.Chat.Server] = $":desktop: {{timestamp.now.shorttime}} **{{{PlaceholderKeys.PlayerName}}}**: {{{PlaceholderKeys.PlayerMessage}}}",
                [LangKeys.Discord.Chat.LinkedMessage] = $":speech_left: {{timestamp.now.shorttime}} **{{{PlaceholderKeys.PlayerName}}}**: {{{PlaceholderKeys.PlayerMessage}}}",
                [LangKeys.Discord.Chat.UnlinkedMessage] = $":chains: {{timestamp.now.shorttime}} {{user.mention}}: {{{PlaceholderKeys.PlayerMessage}}}",
                [LangKeys.Discord.Chat.PlayerName] = "{player.name:clan}",
                [LangKeys.Discord.Team.Message] = $":busts_in_silhouette: {{timestamp.now.shorttime}} **{{{PlaceholderKeys.PlayerName}}}**: {{{PlaceholderKeys.PlayerMessage}}}",
                [LangKeys.Discord.Cards.Message] = $":black_joker: {{timestamp.now.shorttime}} **{{{PlaceholderKeys.PlayerName}}}**: {{{PlaceholderKeys.PlayerMessage}}}",
                [LangKeys.Discord.AdminChat.ServerMessage] = $":mechanic: {{timestamp.now.shorttime}} **{{{PlaceholderKeys.PlayerName}}}**: {{{PlaceholderKeys.PlayerMessage}}}",
                [LangKeys.Discord.AdminChat.DiscordMessage] = $":mechanic: {{timestamp.now.shorttime}} **{{{PlaceholderKeys.PlayerName}}}**: {{{PlaceholderKeys.PlayerMessage}}}",
                [LangKeys.Discord.Clans.ClanMessage] = $"{{timestamp.now.shorttime}} [Clan] **{{{PlaceholderKeys.PlayerName}}}**: {{{PlaceholderKeys.PlayerMessage}}}",
                [LangKeys.Discord.Clans.AllianceMessage] = $"{{timestamp.now.shorttime}} [Alliance] **{{{PlaceholderKeys.PlayerName}}}**: {{{PlaceholderKeys.PlayerMessage}}}",
                [LangKeys.Server.UnlinkedMessage] = $"{{{PlaceholderKeys.DiscordTag}}} [#5f79d6]{{user.fullname}}[/#]: {{{PlaceholderKeys.PlayerMessage}}}",
                [LangKeys.Server.LinkedMessage] = $"{{{PlaceholderKeys.DiscordTag}}} [#5f79d6]{{{PlaceholderKeys.PlayerName}}}[/#]: {{{PlaceholderKeys.PlayerMessage}}}"
            }, this);
        }
    }
}