using System;
using System.Collections.Generic;
using DiscordChatPlugin.Localization;
using Oxide.Ext.Discord.Libraries.Placeholders;

namespace DiscordChatPlugin.Plugins
{
    public partial class DiscordChat
    {
        public string Lang(string key)
        {
            return lang.GetMessage(key, this);
        }

        public string Lang(string key, params object[] args)
        {
            try
            {
                return string.Format(Lang(key), args);
            }
            catch(Exception ex)
            {
                PrintError($"Lang Key '{key}' threw exception\n:{ex}");
                throw;
            }
        }
        
        public string Lang(string key, PlaceholderData data)
        {
            string message = Lang(key);
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
                [LangKeys.Discord.Player.Connected] = ":white_check_mark: ({discordchat.server.time:HH:mm}) **{discordchat.player.name}** has joined.",
                [LangKeys.Discord.Player.Disconnected] = ":x: ({discordchat.server.time:HH:mm}) **{discordchat.player.name}** has disconnected. ({2})",
                [LangKeys.Discord.Chat.Server] = ":desktop: ({discordchat.server.time:HH:mm}) **{discordchat.player.name}**: {discordchat.player.message}",
                [LangKeys.Discord.Chat.LinkedMessage] = ":speech_left: ({discordchat.server.time:HH:mm}) **{discordchat.player.name}**: {discordchat.player.message}",
                [LangKeys.Discord.Chat.UnlinkedMessage] = ":chains: ({discordchat.server.time:HH:mm}) **{user.fullname}**: {discordchat.player.message}",
                [LangKeys.Discord.Team.Message] = ":busts_in_silhouette: ({discordchat.server.time:HH:mm}) **{discordchat.player.name}**: {discordchat.player.message}",
                [LangKeys.Discord.Cards.Message] = ":black_joker: ({discordchat.server.time:HH:mm}) **{discordchat.player.name}**: {discordchat.player.message}",
                [LangKeys.Discord.AdminChat.ServerMessage] = ":mechanic: ({discordchat.server.time:HH:mm}) **{discordchat.player.name}**: {discordchat.player.message}",
                [LangKeys.Discord.AdminChat.DiscordMessage] = ":mechanic: ({discordchat.server.time:HH:mm}) **{discordchat.player.name}**: {discordchat.player.message}",
                [LangKeys.Discord.Clans.ClanMessage] = "({discordchat.server.time:HH:mm}) [Clan] [{discordchat.clan.tag}] **{discordchat.player.message}**: {discordchat.player.message}",
                [LangKeys.Discord.Clans.AllianceMessage] = "({discordchat.server.time:HH:mm}) [Alliance] [{discordchat.clan.tag}] **{discordchat.player.message}**: {discordchat.player.message}",
                [LangKeys.Server.DiscordTag] = "[#5f79d6][Discord][/#]",
                [LangKeys.Server.UnlinkedMessage] = "{discordchat.tag} [#5f79d6]{user.fullname}[/#]: {discordchat.player.message}",
                [LangKeys.Server.LinkedMessage] = "{discordchat.tag} [#5f79d6]{discordchat.player.name}[/#]: {discordchat.player.message}",
                [LangKeys.Server.ClanTag] = "[{0}] "
            }, this);
        }
    }
}