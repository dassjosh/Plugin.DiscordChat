using ConVar;
using DiscordChatPlugin.Enums;
using DiscordChatPlugin.Localization;
using DiscordChatPlugin.PluginHandlers;
using Oxide.Core;
using Oxide.Core.Libraries.Covalence;
using Oxide.Core.Plugins;
using Oxide.Ext.Discord.Entities.Users;
using Oxide.Ext.Discord.Extensions;
using Oxide.Ext.Discord.Libraries.Placeholders;

namespace DiscordChatPlugin.Plugins
{
    public partial class DiscordChat
    {
        private void OnUserConnected(IPlayer player)
        {
            if (_pluginConfig.PlayerStateSettings.ShowAdmins || !player.IsAdmin)
            {
                PlaceholderData placeholders = GetDefault().AddPlayer(player);
                ProcessPlayerState(LangKeys.Discord.Player.Connected, placeholders);
            }
        }

        private void OnUserDisconnected(IPlayer player, string reason)
        {
            if (_pluginConfig.PlayerStateSettings.ShowAdmins || !player.IsAdmin)
            {
                PlaceholderData placeholders = GetDefault().AddPlayer(player).Add(DisconnectReasonPlaceholder, reason);
                ProcessPlayerState(LangKeys.Discord.Player.Disconnected, placeholders);
            }
        }

        public void ProcessPlayerState(string langKey, PlaceholderData data)
        {
            Sends[MessageSource.PlayerState]?.QueueMessage(Lang(langKey, null, data));
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
                    _plugins.Add(new AdminChatHandler(Client, this, _pluginConfig.PluginSupport.AdminChat, plugin));
                    break;
                
                case "AdminDeepCover":
                    _plugins.Add(new AdminDeepCoverHandler(this, plugin));
                    break;
                
                case "AntiSpam":
                    if (plugin.Version < new VersionNumber(2, 0, 0))
                    {
                        PrintError("AntiSpam plugin must be version 2.0.0 or higher");
                        break;
                    }
                    
                    _plugins.Add(new AntiSpamHandler(this, _pluginConfig.PluginSupport.AntiSpam, plugin));
                    break;

                case "BetterChatMute":
                    _plugins.Add(new BetterChatMuteHandler(this, _pluginConfig.PluginSupport.BetterChatMute, plugin));
                    break;
                
                case "Clans":
                    _plugins.Add(new ClansHandler(this, _pluginConfig.PluginSupport.Clans, plugin));
                    break;

                case "TranslationAPI":
                    _plugins.Add(new TranslationApiHandler(this, _pluginConfig.PluginSupport.ChatTranslator, plugin));
                    break;
                
                case "UFilter":
                    _plugins.Add(new UFilterHandler(this, _pluginConfig.PluginSupport.UFilter, plugin));
                    break;
            }
        }

        private void OnPluginUnloaded(Plugin plugin)
        {
            _plugins.RemoveAll(h => h.GetPluginName() == plugin.Name);
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

            if (!Sends.ContainsKey(source))
            {
                return;
            }
            
            HandleMessage(message, player, user, source, null);
        }
    }
}