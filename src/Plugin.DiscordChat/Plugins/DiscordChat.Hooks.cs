﻿using ConVar;
using DiscordChatPlugin.Configuration.Plugins;
using DiscordChatPlugin.Enums;
using DiscordChatPlugin.Localization;
using DiscordChatPlugin.Placeholders;
using DiscordChatPlugin.PluginHandlers;
using Oxide.Core;
using Oxide.Core.Libraries.Covalence;
using Oxide.Core.Plugins;
using Oxide.Ext.Discord.Entities;
using Oxide.Ext.Discord.Extensions;
using Oxide.Ext.Discord.Libraries;

namespace DiscordChatPlugin.Plugins;

public partial class DiscordChat
{
    private void OnUserApproved(string name, string id, string ip)
    {
        IPlayer player = players.FindPlayerById(id) ?? PlayerExt.CreateDummyPlayer(id, name, ip);
        if (_pluginConfig.PlayerStateSettings.ShowAdmins || !player.IsAdmin)
        {
            PlaceholderData placeholders = GetDefault().AddPlayer(player).AddIp(ip);
            ProcessPlayerState(MessageSource.Connecting, LangKeys.Discord.Player.Connecting, placeholders);
        }
    }
        
    private void OnUserConnected(IPlayer player)
    {
        if (_pluginConfig.PlayerStateSettings.ShowAdmins || !player.IsAdmin)
        {
            PlaceholderData placeholders = GetDefault().AddPlayer(player);
            ProcessPlayerState(MessageSource.Connected, LangKeys.Discord.Player.Connected, placeholders);
        }
    }

    private void OnUserDisconnected(IPlayer player, string reason)
    {
        if (_pluginConfig.PlayerStateSettings.ShowAdmins || !player.IsAdmin)
        {
            PlaceholderData placeholders = GetDefault().AddPlayer(player).Add(PlaceholderDataKeys.DisconnectReason, reason);
            ProcessPlayerState(MessageSource.Disconnected, LangKeys.Discord.Player.Disconnected, placeholders);
        }
    }

    public void ProcessPlayerState(MessageSource source, string langKey, PlaceholderData data)
    {
        string message = Lang(langKey, data);
        Sends[source]?.QueueMessage(message);
    }

    private void OnPluginLoaded(Plugin plugin)
    {
        if (plugin == null)
        {
            return;
        }

        OnPluginUnloaded(plugin);
            
        switch (plugin.Name)
        {
            case "AdminChat":
                AddHandler(new AdminChatHandler(Client, this, _pluginConfig.PluginSupport.AdminChat, plugin));
                break;
                
            case "AdminDeepCover":
                AddHandler(new AdminDeepCoverHandler(this, plugin));
                break;
                
            case "AntiSpam":
                if (plugin.Version < new VersionNumber(2, 0, 0))
                {
                    PrintError("AntiSpam plugin must be version 2.0.0 or higher");
                    break;
                }
                    
                AddHandler(new AntiSpamHandler(this, _pluginConfig.PluginSupport.AntiSpam, plugin));
                break;

            case "BetterChatMute":
                BetterChatMuteSettings muteSettings = _pluginConfig.PluginSupport.BetterChatMute;
                if (muteSettings.IgnoreMuted)
                {
                    AddHandler(new BetterChatMuteHandler(this, muteSettings, plugin));
                }
                break;
            
            case "BetterChat": 
                AddHandler(new BetterChatHandler(this, _pluginConfig.PluginSupport.BetterChat, plugin));
                break;

            case "TranslationAPI":
                AddHandler(new TranslationApiHandler(this, _pluginConfig.PluginSupport.ChatTranslator, plugin));
                break;
                
            case "UFilter":
                AddHandler(new UFilterHandler(this, _pluginConfig.PluginSupport.UFilter, plugin));
                break;
        }
    }

    public void AddHandler(IPluginHandler handler)
    {
        _plugins.Add(handler);
    }

    private void OnPluginUnloaded(Plugin plugin)
    {
        if (plugin.Name != Name)
        {
            _plugins.RemoveAll(h => h.GetPluginName() == plugin.Name);
        }
    }
        
#if RUST
    private void OnPlayerChat(BasePlayer rustPlayer, string message, Chat.ChatChannel chatChannel)
    {
        HandleChat(rustPlayer.IPlayer, message, (int)chatChannel);
    }
#else
    private void OnUserChat(IPlayer player, string message)
    {
        HandleChat(player, message, 0);
    }
#endif

    public void HandleChat(IPlayer player, string message, int channel)
    {
        DiscordUser user = player.GetDiscordUser();
        MessageSource source = GetSourceFromServerChannel(channel);

        if (Sends.ContainsKey(source))
        {
            HandleMessage(message, player, user, source, null);
        }
    }
}