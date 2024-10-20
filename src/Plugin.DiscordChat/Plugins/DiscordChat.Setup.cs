﻿using DiscordChatPlugin.Configuration;
using DiscordChatPlugin.Configuration.Plugins;
using DiscordChatPlugin.PluginHandlers;
using DiscordChatPlugin.Templates;
using Oxide.Core;

namespace DiscordChatPlugin.Plugins;

public partial class DiscordChat
{
    private void Init()
    {
        Instance = this;

        _adminChatSettings = _pluginConfig.PluginSupport.AdminChat;
            
#if RUST
        Unsubscribe(nameof(OnPlayerChat));
#else
        Unsubscribe(nameof(OnUserChat));
#endif
            
        Unsubscribe(nameof(OnUserApproved));
        Unsubscribe(nameof(OnUserConnected));
        Unsubscribe(nameof(OnUserDisconnected));
        Unsubscribe(nameof(OnServerShutdown));
        Unsubscribe(nameof(OnClanChat));
        Unsubscribe(nameof(OnAllianceChat));
            
        _plugins.Add(new DiscordChatHandler(this, _pluginConfig.ChatSettings, this, server));
    }

    protected override void LoadDefaultConfig()
    {
        PrintWarning("Loading Default Config");
    }

    protected override void LoadConfig()
    {
        base.LoadConfig();
        _pluginConfig = AdditionalConfig(Config.ReadObject<PluginConfig>());
        Config.WriteObject(_pluginConfig);
    }

    private PluginConfig AdditionalConfig(PluginConfig config)
    {
        config.ChatSettings = new ChatSettings(config.ChatSettings);
        config.PlayerStateSettings = new PlayerStateSettings(config.PlayerStateSettings);
        config.ServerStateSettings = new ServerStateSettings(config.ServerStateSettings);
        config.PluginSupport = new PluginSupport(config.PluginSupport);
        return config;
    }

    private void OnServerInitialized(bool startup)
    {
        _serverInitCalled = true;
        if (IsPluginLoaded(BetterChat))
        {
            if (BetterChat.Version < new VersionNumber(5, 2, 7))
            {
                PrintWarning("Please update your version of BetterChat to version >= 5.2.7");
            }
        }

        if (string.IsNullOrEmpty(_pluginConfig.DiscordApiKey))
        {
            PrintWarning("Please set the Discord Bot Token and reload the plugin");
            return;
        }

        OnPluginLoaded(plugins.Find("AdminChat"));
        OnPluginLoaded(plugins.Find("AdminDeepCover"));
        OnPluginLoaded(plugins.Find("AntiSpam"));
        OnPluginLoaded(plugins.Find("BetterChatMute"));
        OnPluginLoaded(plugins.Find("TranslationAPI"));
        OnPluginLoaded(plugins.Find("UFilter"));
        OnPluginLoaded(plugins.Find("BetterChat"));
            
        if (startup && _pluginConfig.ServerStateSettings.SendOnlineMessage)
        {
            SendGlobalTemplateMessage(TemplateKeys.Server.Online, FindChannel(_pluginConfig.ServerStateSettings.ServerStateChannel), GetDefault());
        }
    }

    private void OnServerShutdown()
    {
        if(_pluginConfig.ServerStateSettings.SendShutdownMessage)
        {
            SendGlobalTemplateMessage(TemplateKeys.Server.Shutdown, FindChannel(_pluginConfig.ServerStateSettings.ServerStateChannel), GetDefault());
        }
    }

    private void Unload()
    {
        Instance = null;
    }
}