using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using DiscordChatPlugin.Enums;
using DiscordChatPlugin.Localization;
using DiscordChatPlugin.PluginHandlers;
using Oxide.Core.Libraries.Covalence;
using Oxide.Ext.Discord.Entities;
using Oxide.Ext.Discord.Entities.Channels;
using Oxide.Ext.Discord.Entities.Guilds;
using Oxide.Ext.Discord.Entities.Messages;
using Oxide.Ext.Discord.Entities.Permissions;
using Oxide.Ext.Discord.Entities.Users;
using MessageType = DiscordChatPlugin.Enums.MessageType;

namespace DiscordChatPlugin.Plugins
{
    public partial class DiscordChat
    {
        private readonly Regex _channelMention = new Regex(@"(<#\d+>)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        
        public void HandleMessage(string content, IPlayer player, DiscordUser user, MessageType type, DiscordMessage sourceMessage)
        {
            if (!CanSendMessage(content, player, user, type, sourceMessage))
            {
                return;
            }

            ProcessCallbackMessages(content, player, user, type, processedMessage =>
            {
                StringBuilder messageBuilder = _pool.GetStringBuilder(processedMessage);

                if (sourceMessage != null)
                {
                    ProcessMentions(sourceMessage, messageBuilder);
                }
                
                ProcessMessage(messageBuilder, player, user, type);
                SendMessage(_pool.FreeStringBuilderToString(messageBuilder), player, user, type, sourceMessage);
            });
        }

        public void ProcessMentions(DiscordMessage message, StringBuilder sb)
        {
            DiscordGuild guild = Client.Bot.GetGuild(message.GuildId);
            if (message.Mentions != null)
            {
                foreach (KeyValuePair<Snowflake, DiscordUser> mention in message.Mentions)
                {
                    sb.Replace($"<@{mention.Key.ToString()}>", $"@{mention.Value.DisplayName}");
                }
            
                foreach (KeyValuePair<Snowflake, DiscordUser> mention in message.Mentions)
                {
                    GuildMember member = guild.Members[mention.Key];
                    sb.Replace($"<@!{mention.Key.ToString()}>", $"@{member?.Nickname ?? mention.Value.DisplayName}");
                }
            }
            
            if (message.MentionsChannels != null)
            {
                foreach (KeyValuePair<Snowflake, ChannelMention> mention in message.MentionsChannels)
                {
                    sb.Replace($"<#{mention.Key.ToString()}>", $"#{mention.Value.Name}");
                }
            }

            foreach (Match match in _channelMention.Matches(message.Content))
            {
                string value = match.Value;
                Snowflake id = new Snowflake(value.Substring(2, value.Length - 3));
                DiscordChannel channel = guild.Channels[id];
                if (channel != null)
                {
                    sb.Replace(value, $"#{channel.Name}");
                }
            }

            if (message.MentionRoles != null)
            {
                foreach (Snowflake roleId in message.MentionRoles)
                {
                    DiscordRole role = guild.Roles[roleId];
                    sb.Replace($"<@&{roleId.ToString()}>", $"@{role.Name ?? roleId}");
                }
            }
        }

        public bool CanSendMessage(string message, IPlayer player, DiscordUser user, MessageType type, DiscordMessage sourceMessage)
        {
            for (int index = 0; index < _plugins.Count; index++)
            {
                if (!_plugins[index].CanSendMessage(message, player, user, type, sourceMessage))
                {
                    return false;
                }
            }
            
            return true;
        }

        public void ProcessCallbackMessages(string message, IPlayer player, DiscordUser user, MessageType type, Action<string> completed, int index = 0)
        {
            for (; index < _plugins.Count; index++)
            {
                IPluginHandler handler = _plugins[index];
                if (handler.HasCallbackMessage())
                {
                    handler.ProcessCallbackMessage(message, player, user, type, callbackMessage =>
                    {
                        ProcessCallbackMessages(callbackMessage, player, user, type, completed, index + 1);
                    });
                    return;
                }
            }
            
            completed.Invoke(message);
        }
        
        public void ProcessMessage(StringBuilder message, IPlayer player, DiscordUser user, MessageType type)
        {
            for (int index = 0; index < _plugins.Count; index++)
            {
                _plugins[index].ProcessMessage(message, player, user, type);
            }
        }

        public void SendMessage(string message, IPlayer player, DiscordUser user, MessageType type, DiscordMessage sourceMessage)
        {
            for (int index = 0; index < _plugins.Count; index++)
            {
                IPluginHandler plugin = _plugins[index];
                if (plugin.SendMessage(message, player, user, type, sourceMessage))
                {
                    return;
                }
            }
        }
    }
}