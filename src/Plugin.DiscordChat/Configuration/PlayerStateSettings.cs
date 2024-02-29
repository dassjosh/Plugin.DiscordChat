using Newtonsoft.Json;
using Oxide.Ext.Discord.Entities;

namespace DiscordChatPlugin.Configuration;

public class PlayerStateSettings
{
    [JsonProperty("Player State Channel ID")]
    public Snowflake PlayerStateChannel { get; set; }
        
    [JsonProperty("Show Admins")]
    public bool ShowAdmins { get; set; }
        
    [JsonProperty("Send Connecting Message")]
    public bool SendConnectingMessage { get; set; }
        
    [JsonProperty("Send Connected Message")]
    public bool SendConnectedMessage { get; set; }
        
    [JsonProperty("Send Disconnected Message")]
    public bool SendDisconnectedMessage { get; set; }

    public PlayerStateSettings(PlayerStateSettings settings)
    {
        PlayerStateChannel = settings?.PlayerStateChannel ?? default(Snowflake);
        ShowAdmins = settings?.ShowAdmins ?? true;
        SendConnectingMessage = settings?.SendConnectingMessage ?? true;
        SendConnectedMessage = settings?.SendConnectedMessage ?? true;
        SendDisconnectedMessage = settings?.SendDisconnectedMessage ?? true;
    }
}