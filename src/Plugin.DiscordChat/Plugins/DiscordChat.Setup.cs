using System;
using DiscordChatPlugin.Configuration;
using DiscordChatPlugin.Configuration.Plugins;
using DiscordChatPlugin.Enums;
using DiscordChatPlugin.PluginHandlers;
using DiscordChatPlugin.Templates;
using Oxide.Core;
using Oxide.Core.Libraries.Covalence;

namespace DiscordChatPlugin.Plugins
{
    public partial class DiscordChat
    {
         private void Init()
        {
            Instance = this;

            _discordSettings.ApiToken = _pluginConfig.DiscordApiKey;
            _discordSettings.LogLevel = _pluginConfig.ExtensionDebugging;

            _adminChatSettings = _pluginConfig.PluginSupport.AdminChat;
            
#if RUST
            Unsubscribe(nameof(OnPlayerChat));
#else
            Unsubscribe(nameof(OnUserChat));
#endif
            
            Unsubscribe(nameof(OnUserConnected));
            Unsubscribe(nameof(OnUserDisconnected));
            Unsubscribe(nameof(OnServerShutdown));
            Unsubscribe(nameof(OnClanChat));
            Unsubscribe(nameof(OnAllianceChat));

            _chatHandler = new DiscordChatHandler(this, _pluginConfig.ChatSettings, this, server);
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

                if (!string.IsNullOrEmpty(_pluginConfig.ChatSettings.DiscordTag))
                {
                    BetterChat.Call("API_RegisterThirdPartyTitle", this, new Func<IPlayer, string>(GetBetterChatTag));
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
            OnPluginLoaded(plugins.Find("Clans"));
            OnPluginLoaded(plugins.Find("TranslationAPI"));
            OnPluginLoaded(plugins.Find("UFilter"));
            
            if (startup)
            {
                Sends[MessageSource.ServerState]?.SendTemplate(TemplateKeys.Server.Online, GetDefault());
            }
        }

        private void OnServerShutdown()
        {
            Sends[MessageSource.ServerState]?.SendTemplate(TemplateKeys.Server.Shutdown, GetDefault());
        }

        private void Unload()
        {
            Instance = null;
        }
    }
}