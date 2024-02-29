using Newtonsoft.Json;
using Oxide.Ext.Discord.Entities;

namespace DiscordChatPlugin.Configuration.Plugins;

public class ClansSettings
{
    [JsonProperty("Clans Chat Channel ID")]
    public Snowflake ClansChatChannel { get; set; }
            
    [JsonProperty("Alliance Chat Channel ID")]
    public Snowflake AllianceChatChannel { get; set; }

    public ClansSettings(ClansSettings settings)
    {
        ClansChatChannel = settings?.ClansChatChannel ?? default(Snowflake);
        AllianceChatChannel = settings?.AllianceChatChannel ?? default(Snowflake);
    }
}