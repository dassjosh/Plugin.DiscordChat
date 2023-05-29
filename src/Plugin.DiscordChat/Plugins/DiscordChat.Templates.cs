using System.Collections.Generic;
using DiscordChatPlugin.Enums;
using DiscordChatPlugin.Templates;
using Oxide.Ext.Discord.Entities;
using Oxide.Ext.Discord.Entities.Channels;
using Oxide.Ext.Discord.Entities.Messages;
using Oxide.Ext.Discord.Libraries.Placeholders;
using Oxide.Ext.Discord.Libraries.Templates;
using Oxide.Ext.Discord.Libraries.Templates.Messages;
using Oxide.Ext.Discord.Libraries.Templates.Messages.Embeds;

namespace DiscordChatPlugin.Plugins
{
    public partial class DiscordChat
    {
        public void RegisterTemplates()
        {
            DiscordMessageTemplate connected = CreateTemplateEmbed(":white_check_mark: ({discordchat.server.time:HH:mm}) **{discordchat.player.name}** has joined.", DiscordSuccess, new TemplateVersion(1, 0, 0));
            _templates.RegisterGlobalTemplateAsync(this, TemplateKeys.Player.StateChanged, connected, new TemplateVersion(1, 0, 0));

            DiscordMessageTemplate online = CreateTemplateEmbed(":green_circle: The server is now online", DiscordSuccess, new TemplateVersion(1, 0, 0));
            _templates.RegisterGlobalTemplateAsync(this, TemplateKeys.Server.Online, online, new TemplateVersion(1, 0, 0));
            
            DiscordMessageTemplate shutdown = CreateTemplateEmbed(":red_circle: The server has shutdown", DiscordDanger, new TemplateVersion(1, 0, 0));
            _templates.RegisterGlobalTemplateAsync(this, TemplateKeys.Server.Shutdown, shutdown, new TemplateVersion(1, 0, 0));
            
            DiscordMessageTemplate booting = CreateTemplateEmbed(":yellow_circle: The server is now booting", DiscordWarning, new TemplateVersion(1, 0, 0));
            _templates.RegisterGlobalTemplateAsync(this, TemplateKeys.Server.Booting, booting, new TemplateVersion(1, 0, 0));
            
            DiscordMessageTemplate serverChat = CreateTemplateEmbed("{discordchat.message}", DiscordBlurple, new TemplateVersion(1, 0, 0));
            _templates.RegisterGlobalTemplateAsync(this, TemplateKeys.Chat.General, serverChat, new TemplateVersion(1, 0, 0));
            
            DiscordMessageTemplate teamChat = CreateTemplateEmbed("{discordchat.message}", DiscordSuccess, new TemplateVersion(1, 0, 0));
            _templates.RegisterGlobalTemplateAsync(this, TemplateKeys.Chat.Teams, teamChat, new TemplateVersion(1, 0, 0));
            
            DiscordMessageTemplate cardsChat = CreateTemplateEmbed("{discordchat.message}", DiscordDanger, new TemplateVersion(1, 0, 0));
            _templates.RegisterGlobalTemplateAsync(this, TemplateKeys.Chat.Cards, cardsChat, new TemplateVersion(1, 0, 0));
            
            DiscordMessageTemplate clanChat = CreateTemplateEmbed("{discordchat.message}", "a1ff46", new TemplateVersion(1, 0, 0));
            _templates.RegisterGlobalTemplateAsync(this, TemplateKeys.Chat.Clans.Clan, clanChat, new TemplateVersion(1, 0, 0));
            
            DiscordMessageTemplate allianceChat = CreateTemplateEmbed("{discordchat.message}", "a1ff46", new TemplateVersion(1, 0, 0));
            _templates.RegisterGlobalTemplateAsync(this, TemplateKeys.Chat.Clans.Alliance, allianceChat, new TemplateVersion(1, 0, 0));
            
            DiscordMessageTemplate errorNotLinked = CreatePrefixedTemplateEmbed("You're not allowed to chat with the server unless you are linked.", DiscordDanger, new TemplateVersion(1, 0, 0));
            _templates.RegisterGlobalTemplateAsync(this, TemplateKeys.Error.NotLinked, errorNotLinked, new TemplateVersion(1, 0, 0));
            
            DiscordMessageTemplate errorAdminChatNotLinked = CreatePrefixedTemplateEmbed("You're not allowed to use admin chat because you have not linked your Discord and game server accounts", DiscordDanger, new TemplateVersion(1, 0, 0));
            _templates.RegisterGlobalTemplateAsync(this, TemplateKeys.Error.AdminChat.NotLinked, errorAdminChatNotLinked, new TemplateVersion(1, 0, 0));
            
            DiscordMessageTemplate errorAdminChatNotPermission = CreatePrefixedTemplateEmbed(":no_entry: You're not allowed to use admin chat channel because you do not have permission.", DiscordDanger, new TemplateVersion(1, 0, 0));
            _templates.RegisterGlobalTemplateAsync(this, TemplateKeys.Error.AdminChat.NoPermission, errorAdminChatNotPermission, new TemplateVersion(1, 0, 0));
        }
        
        public DiscordMessageTemplate CreateTemplateEmbed(string description, string color, TemplateVersion version)
        {
            return new DiscordMessageTemplate
            {
                Embeds = new List<MessageEmbedTemplate>
                {
                    new MessageEmbedTemplate
                    {
                        Description = description,
                        Color = $"#{color}"
                    }
                },
                Version = version
            };
        }
        
        public DiscordMessageTemplate CreatePrefixedTemplateEmbed(string description, string color, TemplateVersion version)
        {
            return new DiscordMessageTemplate
            {
                Embeds = new List<MessageEmbedTemplate>
                {
                    new MessageEmbedTemplate
                    {
                        Description = $"[{{plugin.title}}] {description}",
                        Color = $"#{color}"
                    }
                },
                Version = version
            };
        }
        
        public void SendGlobalTemplateMessage(string templateName, Snowflake channelId, PlaceholderData placeholders = null)
        {
            MessageCreate create = new MessageCreate
            {
                AllowedMention = _allowedMention
            };
            DiscordChannel channel = Guild.Channels[channelId];
            channel?.CreateGlobalTemplateMessage(_client, this, templateName, create, placeholders);
        }

        public string GetTemplateName(MessageSource type)
        {
            switch (type)
            {
                case MessageSource.Discord:
                case MessageSource.Server:
                    return TemplateKeys.Chat.General;
                case MessageSource.Team:
                    return TemplateKeys.Chat.Teams;
                case MessageSource.Cards:
                    return TemplateKeys.Chat.Cards;
                case MessageSource.PlayerState:
                    return TemplateKeys.Player.StateChanged;
                case MessageSource.AdminChat:
                    return TemplateKeys.Chat.AdminChat.Message;
                case MessageSource.ClanChat:
                    return TemplateKeys.Chat.Clans.Clan;
                case MessageSource.AllianceChat:
                    return TemplateKeys.Chat.Clans.Alliance;
            }

            return null;
        }
    }
}