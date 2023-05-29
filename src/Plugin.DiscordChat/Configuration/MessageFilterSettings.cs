using System.Collections.Generic;
using Newtonsoft.Json;
using Oxide.Ext.Discord.Entities;
using Oxide.Ext.Discord.Entities.Guilds;
using Oxide.Ext.Discord.Entities.Messages;
using Oxide.Ext.Discord.Entities.Users;

namespace DiscordChatPlugin.Configuration
{
    public class MessageFilterSettings
    {
        [JsonProperty("Ignore messages from users in this list (Discord ID)")]
        public List<Snowflake> IgnoreUsers { get; set; }

        [JsonProperty("Ignore messages from users in this role (Role ID)")]
        public List<Snowflake> IgnoreRoles { get; set; }

        [JsonProperty("Ignored Prefixes")]
        public List<string> IgnoredPrefixes { get; set; }

        public MessageFilterSettings(MessageFilterSettings settings)
        {
            IgnoreUsers = settings?.IgnoreUsers ?? new List<Snowflake>();
            IgnoreRoles = settings?.IgnoreRoles ?? new List<Snowflake>();
            IgnoredPrefixes = settings?.IgnoredPrefixes ?? new List<string>();
        }

        public bool IgnoreMessage(DiscordMessage message, GuildMember member)
        {
            return IsIgnoredUser(message.Author, member) || IsIgnoredPrefix(message.Content);
        }
        
        public bool IsIgnoredUser(DiscordUser user, GuildMember member)
        {
            if (user.IsBot)
            {
                return true;
            }
            
            if (IgnoreUsers.Contains(user.Id))
            {
                return true;
            }
            
            return member != null && IsRoleIgnoredMember(member);
        }

        public bool IsRoleIgnoredMember(GuildMember member)
        {
            for (int index = 0; index < IgnoreRoles.Count; index++)
            {
                Snowflake role = IgnoreRoles[index];
                if (member.Roles.Contains(role))
                {
                    return true;
                }
            }

            return false;
        }

        public bool IsIgnoredPrefix(string content)
        {
            for (int index = 0; index < IgnoredPrefixes.Count; index++)
            {
                string prefix = IgnoredPrefixes[index];
                if (content.StartsWith(prefix))
                {
                    return true;
                }
            }

            return false;
        }
    }
}