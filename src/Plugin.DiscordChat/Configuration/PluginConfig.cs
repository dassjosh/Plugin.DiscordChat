using System.ComponentModel;
using DiscordChatPlugin.Configuration.Plugins;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Oxide.Ext.Discord.Entities;
using Oxide.Ext.Discord.Logging;

namespace DiscordChatPlugin.Configuration
{
    public class PluginConfig
    {
        [JsonProperty(PropertyName = "Discord Bot Token")]
        public string DiscordApiKey { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "Discord Server ID (Optional if bot only in 1 guild)")]
        public Snowflake GuildId { get; set; }

        [JsonProperty("Chat Settings")]
        public ChatSettings ChatSettings { get; set; }
        
        [JsonProperty("Player State Settings")]
        public PlayerStateSettings PlayerStateSettings { get; set; }        
        
        [JsonProperty("Server State Settings")]
        public ServerStateSettings ServerStateSettings { get; set; }

        [JsonProperty("Plugin Support")]
        public PluginSupport PluginSupport { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        [DefaultValue(DiscordLogLevel.Info)]
        [JsonProperty(PropertyName = "Discord Extension Log Level (Verbose, Debug, Info, Warning, Error, Exception, Off)")]
        public DiscordLogLevel ExtensionDebugging { get; set; } = DiscordLogLevel.Info;
    }
}