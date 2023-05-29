using Newtonsoft.Json;
using Oxide.Ext.Discord.Entities;

namespace DiscordChatPlugin.Configuration
{
    public class ServerStateSettings
    {
        [JsonProperty("Server State Channel ID")]
        public Snowflake ServerStateChannel { get; set; }

        public ServerStateSettings(ServerStateSettings settings)
        {
            ServerStateChannel = settings?.ServerStateChannel ?? default(Snowflake);
        }
    }
}