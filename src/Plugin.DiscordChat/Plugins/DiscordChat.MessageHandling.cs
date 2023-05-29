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
using Oxide.Ext.Discord.Extensions;
using Oxide.Ext.Discord.Pooling;

namespace DiscordChatPlugin.Plugins
{
    public partial class DiscordChat
    {
        private readonly Regex _channelMention = new Regex(@"(<#\d+>)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        
        public void HandleMessage(string content, IPlayer player, DiscordUser user, MessageSource source, DiscordMessage sourceMessage)
        {
            if (!CanSendMessage(content, player, user, source, sourceMessage))
            {
                return;
            }

            ProcessCallbackMessages(content, player, user, source, processedMessage =>
            {
                StringBuilder messageBuilder = _pool.GetStringBuilder(processedMessage);

                if (sourceMessage != null)
                {
                    ProcessMentions(sourceMessage, messageBuilder);
                }
                
                ProcessMessage(messageBuilder, player, user, source);
                SendMessage(_pool.FreeStringBuilderToString(messageBuilder), player, user, source, sourceMessage);
            });
        }

        public void ProcessMentions(DiscordMessage message, StringBuilder sb)
        {
            if (message.Mentions != null)
            {
                foreach (KeyValuePair<Snowflake, DiscordUser> mention in message.Mentions)
                {
                    sb.Replace($"<@{mention.Key.ToString()}>", $"@{mention.Value.Username}");
                }
            
                foreach (KeyValuePair<Snowflake, DiscordUser> mention in message.Mentions)
                {
                    GuildMember member = Guild.Members[mention.Key];
                    sb.Replace($"<@!{mention.Key.ToString()}>", $"@{member?.Nickname ?? mention.Value.Username}");
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
                DiscordChannel channel = Guild.Channels[id];
                if (channel != null)
                {
                    sb.Replace(value, $"#{channel.Name}");
                }
            }

            if (message.MentionRoles != null)
            {
                foreach (Snowflake roleId in message.MentionRoles)
                {
                    DiscordRole role = Guild.Roles[roleId];
                    sb.Replace($"<@&{roleId.ToString()}>", $"@{role.Name ?? roleId}");
                }
            }
        }

        public bool CanSendMessage(string message, IPlayer player, DiscordUser user, MessageSource source, DiscordMessage sourceMessage)
        {
            for (int index = 0; index < _plugins.Count; index++)
            {
                if (!_plugins[index].CanSendMessage(message, player, user, source, sourceMessage))
                {
                    return false;
                }
            }

            return true;
        }

        public void ProcessCallbackMessages(string message, IPlayer player, DiscordUser user, MessageSource source, Action<string> completed, int index = 0)
        {
            for (; index < _plugins.Count; index++)
            {
                IPluginHandler handler = _plugins[index];
                if (handler.HasCallbackMessage())
                {
                    handler.ProcessCallbackMessage(message, player, user, source, callbackMessage =>
                    {
                        ProcessCallbackMessages(callbackMessage, player, user, source, completed, index + 1);
                    });
                    return;
                }
            }
            
            completed.Invoke(message);
        }
        
        public void ProcessMessage(StringBuilder message, IPlayer player, DiscordUser user, MessageSource source)
        {
            for (int index = 0; index < _plugins.Count; index++)
            {
                _plugins[index].ProcessMessage(message, player, user, source);
            }
        }

        public string GetPlayerName(IPlayer player)
        {
            StringBuilder name = _pool.GetStringBuilder(player.Name);
            for (int index = 0; index < _plugins.Count; index++)
            {
                _plugins[index].ProcessPlayerName(name, player);
            }

            return _pool.FreeStringBuilderToString(name);
        }
        
        public void SendMessage(string message, IPlayer player, DiscordUser user, MessageSource source, DiscordMessage sourceMessage)
        {
            for (int index = 0; index < _plugins.Count; index++)
            {
                if (_plugins[index].SendMessage(message, player, user, source, sourceMessage))
                {
                    return;
                }
            }
        }
    }
}