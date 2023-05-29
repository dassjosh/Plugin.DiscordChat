using Newtonsoft.Json;
using Oxide.Ext.Discord.Entities;

namespace DiscordChatPlugin.Configuration.Plugins
{
    public class ClansSettings
    {
        [JsonProperty("Display Clan Tag")]
        public bool ShowClanTag { get; set; }
            
        [JsonProperty("Clans Chat Channel ID")]
        public Snowflake ClansChatChannel { get; set; }
            
        [JsonProperty("Alliance Chat Channel ID")]
        public Snowflake AllianceChatChannel { get; set; }

        public ClansSettings(ClansSettings settings)
        {
            ShowClanTag = settings?.ShowClanTag ?? true;
            ClansChatChannel = settings?.ClansChatChannel ?? default(Snowflake);
            AllianceChatChannel = settings?.AllianceChatChannel ?? default(Snowflake);
        }
    }
}