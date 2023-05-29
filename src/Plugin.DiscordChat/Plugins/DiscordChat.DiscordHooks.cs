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
using Oxide.Ext.Discord.Entities.Applications;
using Oxide.Ext.Discord.Entities.Channels;
using Oxide.Ext.Discord.Entities.Gateway.Events;
using Oxide.Ext.Discord.Entities.Guilds;
using Oxide.Ext.Discord.Entities.Messages;

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

            DiscordGuild guild = null;
            if (ready.Guilds.Count == 1 && !_pluginConfig.GuildId.IsValid())
            {
                guild = ready.Guilds.Values.FirstOrDefault();
            }

            if (guild == null)
            {
                guild = ready.Guilds[_pluginConfig.GuildId];
                if (guild == null)
                {
                    PrintError("Failed to find a matching guild for the Discord Server Id. " +
                               "Please make sure your guild Id is correct and the bot is in the discord server.");
                    return;
                }
            }

            DiscordApplication app = Client.Bot.Application;
            if (!app.HasApplicationFlag(ApplicationFlags.GatewayMessageContentLimited))
            {
                PrintWarning($"You will need to enable \"Message Content Intent\" for {Client.Bot.BotUser.Username} @ https://discord.com/developers/applications\n by April 2022" +
                             $"{Name} will stop function correctly after that date until that is fixed. Once updated please reload {Name}.");
            }

            _guildId = guild.Id;
            Puts($"{Title} Ready");
        }

        [HookMethod(DiscordExtHooks.OnDiscordGuildCreated)]
        private void OnDiscordGuildCreated(DiscordGuild guild)
        {
            if (guild.Id != _guildId)
            {
                return;
            }

            Guild = guild;
            if (_pluginConfig.ChatSettings.DiscordToServer)
            {
                SetupChannel(MessageSource.Server, _pluginConfig.ChatSettings.ChatChannel, _pluginConfig.ChatSettings.UseBotToDisplayChat, HandleDiscordChatMessage);
            }
            else
            {
                SetupChannel(MessageSource.Server, _pluginConfig.ChatSettings.ChatChannel, _pluginConfig.ChatSettings.UseBotToDisplayChat);
            }

            SetupChannel(MessageSource.PlayerState, _pluginConfig.PlayerStateSettings.PlayerStateChannel, false);
            SetupChannel(MessageSource.ServerState, _pluginConfig.ServerStateSettings.ServerStateChannel, false);
            SetupChannel(MessageSource.AdminChat, _pluginConfig.PluginSupport.AdminChat.ChatChannel, _pluginConfig.ChatSettings.UseBotToDisplayChat, HandleAdminChatDiscordMessage);
            SetupChannel(MessageSource.ClanChat, _pluginConfig.PluginSupport.Clans.ClansChatChannel, _pluginConfig.ChatSettings.UseBotToDisplayChat);
            SetupChannel(MessageSource.AllianceChat, _pluginConfig.PluginSupport.Clans.AllianceChatChannel, _pluginConfig.ChatSettings.UseBotToDisplayChat);

#if RUST
            SetupChannel(MessageSource.Team, _pluginConfig.ChatSettings.TeamChannel, false);
            SetupChannel(MessageSource.Cards, _pluginConfig.ChatSettings.CardsChannel, false);
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
                Subscribe(nameof(OnUserConnected));
                Subscribe(nameof(OnUserDisconnected));
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

            timer.In(1f, () =>
            {
                if (!_serverInitCalled)
                {
                    Sends[MessageSource.ServerState]?.SendTemplate(TemplateKeys.Server.Booting, GetDefault());
                }
            });
        }

        public void SetupChannel(MessageSource type, Snowflake id, bool wipeNonBotMessages, Action<DiscordMessage> callback = null)
        {
            if (!id.IsValid())
            {
                return;
            }

            DiscordChannel channel = Guild.Channels[id];
            if (channel == null)
            {
                PrintWarning($"Channel with ID: '{id}' not found in guild");
                return;
            }

            if (callback != null)
            {
                _subscriptions.AddChannelSubscription(Client, id, callback);
            }

            if (wipeNonBotMessages)
            {
                channel.GetMessages(Client, new ChannelMessagesRequest{Limit = 100}).Then(messages => OnGetChannelMessages(messages, channel));
            }

            Sends[type] = new DiscordSendQueue(id, GetTemplateName(type), timer);

            Puts($"Setup Channel {type} With ID: {id}");
        }

        private void OnGetChannelMessages(List<DiscordMessage> messages, DiscordChannel channel)
        {
            if (messages.Count == 0)
            {
                return;
            }

            DiscordMessage[] messagesToDelete = messages
                                                .Where(m => !CanSendMessage(m.Content, m.Author.Player, m.Author, MessageSource.Server, m))
                                                .ToArray();

            if (messagesToDelete.Length == 0)
            {
                return;
            }

            if (messagesToDelete.Length == 1)
            {
                messagesToDelete[0]?.Delete(Client);
                return;
            }

            channel.BulkDeleteMessages(Client, messagesToDelete.Take(100).Select(m => m.Id).ToArray());
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