using System.Collections.Generic;
using DiscordChatPlugin.Enums;
using DiscordChatPlugin.Placeholders;
using DiscordChatPlugin.Templates;
using Oxide.Ext.Discord.Entities;
using Oxide.Ext.Discord.Libraries;

namespace DiscordChatPlugin.Plugins;

public partial class DiscordChat
{
    public void RegisterTemplates()
    {
        DiscordMessageTemplate connecting = CreateTemplateEmbed($"{PlaceholderKeys.TemplateMessage}",  DiscordColor.Warning);
        _templates.RegisterGlobalTemplateAsync(this, TemplateKeys.Player.Connecting, connecting, new TemplateVersion(1, 0, 0), new TemplateVersion(1, 0, 0));
            
        DiscordMessageTemplate connected = CreateTemplateEmbed($"{PlaceholderKeys.TemplateMessage}",  DiscordColor.Success);
        _templates.RegisterGlobalTemplateAsync(this, TemplateKeys.Player.Connected, connected, new TemplateVersion(1, 0, 0), new TemplateVersion(1, 0, 0));
            
        DiscordMessageTemplate disconnected = CreateTemplateEmbed($"{PlaceholderKeys.TemplateMessage}",  DiscordColor.Danger);
        _templates.RegisterGlobalTemplateAsync(this, TemplateKeys.Player.Disconnected, disconnected, new TemplateVersion(1, 0, 0), new TemplateVersion(1, 0, 0));

        DiscordMessageTemplate online = CreateTemplateEmbed($":green_circle: {DefaultKeys.TimestampNow.ShortTime} The server is now online", DiscordColor.Success);
        _templates.RegisterGlobalTemplateAsync(this, TemplateKeys.Server.Online, online, new TemplateVersion(1, 0, 0), new TemplateVersion(1, 0, 0));
            
        DiscordMessageTemplate shutdown = CreateTemplateEmbed($":red_circle: {DefaultKeys.TimestampNow.ShortTime} The server has shutdown", DiscordColor.Danger);
        _templates.RegisterGlobalTemplateAsync(this, TemplateKeys.Server.Shutdown, shutdown, new TemplateVersion(1, 0, 0), new TemplateVersion(1, 0, 0));
            
        DiscordMessageTemplate booting = CreateTemplateEmbed($":yellow_circle: {DefaultKeys.TimestampNow.ShortTime} The server is now booting", DiscordColor.Warning);
        _templates.RegisterGlobalTemplateAsync(this, TemplateKeys.Server.Booting, booting, new TemplateVersion(1, 0, 0), new TemplateVersion(1, 0, 0));
            
        DiscordMessageTemplate serverChat = CreateTemplateEmbed($"{PlaceholderKeys.TemplateMessage}", DiscordColor.Blurple);
        _templates.RegisterGlobalTemplateAsync(this, TemplateKeys.Chat.General, serverChat, new TemplateVersion(1, 0, 0), new TemplateVersion(1, 0, 0));
            
        DiscordMessageTemplate teamChat = CreateTemplateEmbed($"{PlaceholderKeys.TemplateMessage}", DiscordColor.Success);
        _templates.RegisterGlobalTemplateAsync(this, TemplateKeys.Chat.Teams, teamChat, new TemplateVersion(1, 0, 0), new TemplateVersion(1, 0, 0));
            
        DiscordMessageTemplate clanChat = CreateTemplateEmbed($"{PlaceholderKeys.TemplateMessage}", DiscordColor.Success);
        _templates.RegisterGlobalTemplateAsync(this, TemplateKeys.Chat.Clan, clanChat, new TemplateVersion(1, 0, 0), new TemplateVersion(1, 0, 0));
            
        DiscordMessageTemplate cardsChat = CreateTemplateEmbed($"{PlaceholderKeys.TemplateMessage}", DiscordColor.Danger);
        _templates.RegisterGlobalTemplateAsync(this, TemplateKeys.Chat.Cards, cardsChat, new TemplateVersion(1, 0, 0), new TemplateVersion(1, 0, 0));
            
        DiscordMessageTemplate pluginClanChat = CreateTemplateEmbed($"{PlaceholderKeys.TemplateMessage}", new DiscordColor("a1ff46"));
        _templates.RegisterGlobalTemplateAsync(this, TemplateKeys.Chat.Clans.Clan, pluginClanChat, new TemplateVersion(1, 0, 0), new TemplateVersion(1, 0, 0));
            
        DiscordMessageTemplate pluginAllianceChat = CreateTemplateEmbed($"{PlaceholderKeys.TemplateMessage}",  new DiscordColor("80cc38"));
        _templates.RegisterGlobalTemplateAsync(this, TemplateKeys.Chat.Clans.Alliance, pluginAllianceChat, new TemplateVersion(1, 0, 0), new TemplateVersion(1, 0, 0));
            
        DiscordMessageTemplate errorNotLinked = CreatePrefixedTemplateEmbed("You're not allowed to chat with the server unless you are linked.", DiscordColor.Danger);
        _templates.RegisterGlobalTemplateAsync(this, TemplateKeys.Error.NotLinked, errorNotLinked, new TemplateVersion(1, 0, 0), new TemplateVersion(1, 0, 0));
            
        DiscordMessageTemplate errorAdminChatNotLinked = CreatePrefixedTemplateEmbed("You're not allowed to use admin chat because you have not linked your Discord and game server accounts", DiscordColor.Danger);
        _templates.RegisterGlobalTemplateAsync(this, TemplateKeys.Error.AdminChat.NotLinked, errorAdminChatNotLinked, new TemplateVersion(1, 0, 0), new TemplateVersion(1, 0, 0));
            
        DiscordMessageTemplate errorAdminChatNotPermission = CreatePrefixedTemplateEmbed(":no_entry: You're not allowed to use admin chat channel because you do not have permission.", DiscordColor.Danger);
        _templates.RegisterGlobalTemplateAsync(this, TemplateKeys.Error.AdminChat.NoPermission, errorAdminChatNotPermission, new TemplateVersion(1, 0, 0), new TemplateVersion(1, 0, 0));
        
        DiscordMessageTemplate errorBetterChatMuteMuted = CreatePrefixedTemplateEmbed(":no_entry: You're not allowed to chat with the server because you are muted.", DiscordColor.Danger);
        _templates.RegisterGlobalTemplateAsync(this, TemplateKeys.Error.BetterChatMute.Muted, errorBetterChatMuteMuted, new TemplateVersion(1, 0, 0), new TemplateVersion(1, 0, 0));
    }
        
    public DiscordMessageTemplate CreateTemplateEmbed(string description, DiscordColor color)
    {
        return new DiscordMessageTemplate
        {
            Embeds = new List<DiscordEmbedTemplate>
            {
                new()
                {
                    Description = description,
                    Color = color.ToHex()
                }
            }
        };
    }
        
    public DiscordMessageTemplate CreatePrefixedTemplateEmbed(string description, DiscordColor color)
    {
        return new DiscordMessageTemplate
        {
            Embeds = new List<DiscordEmbedTemplate>
            {
                new()
                {
                    Description = $"[{DefaultKeys.Plugin.Name}] {description}",
                    Color = color.ToHex()
                }
            }
        };
    }
        
    public void SendGlobalTemplateMessage(TemplateKey templateName, DiscordChannel channel, PlaceholderData placeholders = null)
    {
        if (channel == null)
        {
            return;
        }
            
        MessageCreate create = new()
        {
            AllowedMentions = AllowedMentions.None
        };
        channel.CreateGlobalTemplateMessage(Client, templateName, create, placeholders);
    }

    public TemplateKey GetTemplateName(MessageSource source)
    {
        switch (source)
        {
            case MessageSource.Discord:
            case MessageSource.Server:
                return TemplateKeys.Chat.General;
            case MessageSource.Team:
                return TemplateKeys.Chat.Teams;
            case MessageSource.Cards:
                return TemplateKeys.Chat.Cards;
            case MessageSource.Clan:
                return TemplateKeys.Chat.Clan;
            case MessageSource.Connecting:
                return TemplateKeys.Player.Connecting;
            case MessageSource.Connected:
                return TemplateKeys.Player.Connected;
            case MessageSource.Disconnected:
                return TemplateKeys.Player.Disconnected;
            case MessageSource.PluginAdminChat:
                return TemplateKeys.Chat.AdminChat.Message;
            case MessageSource.PluginClan:
                return TemplateKeys.Chat.Clans.Clan;
            case MessageSource.PluginAlliance:
                return TemplateKeys.Chat.Clans.Alliance;
        }

        return default;
    }
}