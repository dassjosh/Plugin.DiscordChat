using Newtonsoft.Json;
using Oxide.Ext.Discord.Entities;

namespace DiscordChatPlugin.Configuration
{
    public class PlayerStateSettings
    {
        [JsonProperty("Player State Channel ID")]
        public Snowflake PlayerStateChannel { get; set; }
        
        [JsonProperty("Show Admins")]
        public bool ShowAdmins { get; set; }

        public PlayerStateSettings(PlayerStateSettings settings)
        {
            PlayerStateChannel = settings?.PlayerStateChannel ?? default(Snowflake);
            ShowAdmins = settings?.ShowAdmins ?? true;
        }
    }
}