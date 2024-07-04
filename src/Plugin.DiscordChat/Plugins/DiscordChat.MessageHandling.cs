using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using DiscordChatPlugin.Enums;
using DiscordChatPlugin.Placeholders;
using DiscordChatPlugin.PluginHandlers;
using Oxide.Core.Libraries.Covalence;
using Oxide.Ext.Discord.Cache;
using Oxide.Ext.Discord.Entities;
using Oxide.Ext.Discord.Libraries;

namespace DiscordChatPlugin.Plugins;

public partial class DiscordChat
{
    private readonly Regex _channelMention = new(@"(<#\d+>)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        
    public void HandleMessage(string content, IPlayer player, DiscordUser user, MessageSource source, DiscordMessage sourceMessage)
    {
        if (!CanSendMessage(content, player, user, source, sourceMessage))
        {
            return;
        }

#if RUST
        content = EmojiCache.Instance.ReplaceEmojiWithText(content);
#endif
        
        ProcessCallbackMessages(content, player, user, source, processedMessage =>
        {
            StringBuilder sb = Pool.GetStringBuilder(processedMessage);

            if (sourceMessage != null)
            {
                ProcessMentions(sourceMessage, sb);
            }
                
            ProcessMessage(sb, player, user, source);
            SendMessage(Pool.ToStringAndFree(sb), player, user, source, sourceMessage);
        });
    }

    public void ProcessMentions(DiscordMessage message, StringBuilder sb)
    {
        DiscordGuild guild = Client.Bot.GetGuild(message.GuildId);
        if (message.Mentions != null)
        {
            foreach (KeyValuePair<Snowflake, DiscordUser> mention in message.Mentions)
            {
                GuildMember member = guild.Members[mention.Key];
                sb.Replace($"<@{mention.Key.ToString()}>", $"@{member?.DisplayName ?? mention.Value.DisplayName}");
            }
            
            foreach (KeyValuePair<Snowflake, DiscordUser> mention in message.Mentions)
            {
                GuildMember member = guild.Members[mention.Key];
                sb.Replace($"<@!{mention.Key.ToString()}>", $"@{member?.DisplayName ?? mention.Value.DisplayName}");
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
            Snowflake id = new(value.AsSpan().Slice(2, value.Length - 3));
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

    public void SendMessage(string message, IPlayer player, DiscordUser user, MessageSource source, DiscordMessage sourceMessage)
    {
        using PlaceholderData data = GetPlaceholders(message, player, user, sourceMessage);
        data.ManualPool();
        for (int index = 0; index < _plugins.Count; index++)
        {
            IPluginHandler plugin = _plugins[index];
            if (plugin.SendMessage(message, player, user, source, sourceMessage, data))
            {
                return;
            }
        }
    }
        
    private PlaceholderData GetPlaceholders(string message, IPlayer player, DiscordUser user, DiscordMessage sourceMessage)
    {
        PlaceholderData placeholders = GetDefault().AddPlayer(player).AddUser(user).AddMessage(sourceMessage).Add(PlaceholderDataKeys.PlayerMessage, message);
        if (sourceMessage != null)
        {
            placeholders.AddGuildMember(Client.Bot.GetGuild(sourceMessage.GuildId)?.Members[sourceMessage.Author.Id]);
        }

        return placeholders;
    }
}