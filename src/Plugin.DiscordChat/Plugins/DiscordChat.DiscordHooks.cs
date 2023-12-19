using System;
using System.Collections.Generic;
using System.Linq;
using DiscordChatPlugin.Enums;
using DiscordChatPlugin.Helpers;
using DiscordChatPlugin.Templates;
using Oxide.Core;
using Oxide.Core.Libraries.Covalence;
using Oxide.Core.Plugins;
using Oxide.Ext.Discord.Constants;
using Oxide.Ext.Discord.Entities;

namespace DiscordChatPlugin.Plugins
{
    public partial class DiscordChat
    {
        [HookMethod(DiscordExtHooks.OnDiscordClientCreated)]
        private void OnDiscordClientCreated()
        {
            if (!string.IsNullOrEmpty(_pluginConfig.DiscordApiKey))
            {
                RegisterPlaceholders();
                RegisterTemplates();
                Client.Connect(_discordSettings);
            }
        }
        
        [HookMethod(DiscordExtHooks.OnDiscordGatewayReady)]
        private void OnDiscordGatewayReady(GatewayReadyEvent ready)
        {
            if (ready.Guilds.Count == 0)
            {
                PrintError("Your bot was not found in any discord servers. Please invite it to a server and reload the plugin.");
                return;
            }

            DiscordApplication app = Client.Bot.Application;
            if (!app.HasApplicationFlag(ApplicationFlags.GatewayMessageContentLimited))
            {
                PrintWarning($"You will need to enable \"Message Content Intent\" for {Client.Bot.BotUser.Username} @ https://discord.com/developers/applications\n by April 2022" +
                             $"{Name} will stop function correctly after that date until that is fixed. Once updated please reload {Name}.");
            }
            
            Puts($"{Title} Ready");
        }

        [HookMethod(DiscordExtHooks.OnDiscordGuildCreated)]
        private void OnDiscordGuildCreated(DiscordGuild guild)
        {
            if (_pluginConfig.ChatSettings.DiscordToServer)
            {
                SetupChannel(guild, MessageSource.Server, _pluginConfig.ChatSettings.ChatChannel, _pluginConfig.ChatSettings.UseBotToDisplayChat, HandleDiscordChatMessage);
            }
            else
            {
                SetupChannel(guild, MessageSource.Server, _pluginConfig.ChatSettings.ChatChannel, _pluginConfig.ChatSettings.UseBotToDisplayChat);
            }
            
            SetupChannel(guild, MessageSource.Discord, _pluginConfig.ChatSettings.ChatChannel, _pluginConfig.ChatSettings.UseBotToDisplayChat);

            SetupChannel(guild, MessageSource.Connecting, _pluginConfig.PlayerStateSettings.PlayerStateChannel);
            SetupChannel(guild, MessageSource.Connected, _pluginConfig.PlayerStateSettings.PlayerStateChannel);
            SetupChannel(guild, MessageSource.Disconnected, _pluginConfig.PlayerStateSettings.PlayerStateChannel);
            SetupChannel(guild, MessageSource.PluginAdminChat, _pluginConfig.PluginSupport.AdminChat.ChatChannel, _pluginConfig.ChatSettings.UseBotToDisplayChat, HandleAdminChatDiscordMessage);
            SetupChannel(guild, MessageSource.PluginClan, _pluginConfig.PluginSupport.Clans.ClansChatChannel, _pluginConfig.ChatSettings.UseBotToDisplayChat);
            SetupChannel(guild, MessageSource.PluginAlliance, _pluginConfig.PluginSupport.Clans.AllianceChatChannel, _pluginConfig.ChatSettings.UseBotToDisplayChat);

#if RUST
            SetupChannel(guild, MessageSource.Team, _pluginConfig.ChatSettings.TeamChannel);
            SetupChannel(guild, MessageSource.Cards, _pluginConfig.ChatSettings.CardsChannel);
            SetupChannel(guild, MessageSource.Clan, _pluginConfig.ChatSettings.ClansChannel);
#endif

            if (_pluginConfig.ChatSettings.ChatChannel.IsValid()
#if RUST
                || _pluginConfig.ChatSettings.TeamChannel.IsValid()
                || _pluginConfig.ChatSettings.CardsChannel.IsValid()
#endif
               )
            {
#if RUST
                Subscribe(nameof(OnPlayerChat));
#else
                Subscribe(nameof(OnUserChat));
#endif
            }

            if (_pluginConfig.PlayerStateSettings.PlayerStateChannel.IsValid())
            {
                if (_pluginConfig.PlayerStateSettings.SendConnectingMessage)
                {
                    Subscribe(nameof(OnUserApproved));
                }
                
                if (_pluginConfig.PlayerStateSettings.SendConnectedMessage)
                {
                    Subscribe(nameof(OnUserConnected));
                }

                if (_pluginConfig.PlayerStateSettings.SendDisconnectedMessage)
                {
                    Subscribe(nameof(OnUserDisconnected));
                }
            }

            if (_pluginConfig.ServerStateSettings.ServerStateChannel.IsValid())
            {
                Subscribe(nameof(OnServerShutdown));
            }
            
            if (_pluginConfig.PluginSupport.Clans.ClansChatChannel.IsValid())
            {
                Subscribe(nameof(OnClanChat));
            }
            
            if (_pluginConfig.PluginSupport.Clans.AllianceChatChannel.IsValid())
            {
                Subscribe(nameof(OnAllianceChat));
            }

            timer.In(0.1f, () =>
            {
                if (!_serverInitCalled && _pluginConfig.ServerStateSettings.SendBootingMessage)
                {
                    SendGlobalTemplateMessage(TemplateKeys.Server.Booting, FindChannel(_pluginConfig.ServerStateSettings.ServerStateChannel));
                }
            });
        }

        public void SetupChannel(DiscordGuild guild, MessageSource source, Snowflake id, bool wipeNonBotMessages = false, Action<DiscordMessage> callback = null)
        {
            if (!id.IsValid())
            {
                return;
            }

            DiscordChannel channel = guild.Channels[id];
            if (channel == null)
            {
                //PrintWarning($"Channel with ID: '{id}' not found in guild");
                return;
            }

            if (callback != null)
            {
                _subscriptions.AddChannelSubscription(Client, id, callback);
            }
            
            if (wipeNonBotMessages)
            {
                channel.GetMessages(Client, new ChannelMessagesRequest{Limit = 100})
                       .Then(messages =>
                       {
                           OnGetChannelMessages(messages, callback);
                       });
            }

            Sends[source] = new DiscordSendQueue(channel, GetTemplateName(source), timer);;
            Puts($"Setup Channel {source} With ID: {id}");
        }

        private void OnGetChannelMessages(List<DiscordMessage> messages, Action<DiscordMessage> callback)
        {
            if (messages.Count == 0 || callback == null)
            {
                return;
            }

            foreach (DiscordMessage message in messages
                         .Where(m => !m.Author.IsBot && (DateTimeOffset.UtcNow - m.Id.GetCreationDate()).TotalDays < 14 && CanSendMessage(m.Content, m.Author.Player, m.Author, MessageSource.Discord, m)))
            {
                callback.Invoke(message);
            }
        }

        public void HandleDiscordChatMessage(DiscordMessage message)
        {
            IPlayer player = message.Author.Player;
            if (Interface.Oxide.CallHook("OnDiscordChatMessage", player, message.Content, message.Author) != null)
            {
                return;
            }
            
            HandleMessage(message.Content, player, message.Author, MessageSource.Discord, message);
            
            if (_pluginConfig.ChatSettings.UseBotToDisplayChat)
            {
                message.Delete(Client);
            }
        }
    }
}