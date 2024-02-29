using Newtonsoft.Json;
using Oxide.Ext.Discord.Entities;

namespace DiscordChatPlugin.Configuration;

public class ServerStateSettings
{
    [JsonProperty("Server State Channel ID")]
    public Snowflake ServerStateChannel { get; set; }
        
    [JsonProperty("Send Booting Message")]
    public bool SendBootingMessage { get; set; }
        
    [JsonProperty("Send Online Message")]
    public bool SendOnlineMessage { get; set; }
        
    [JsonProperty("Send Shutdown Message")]
    public bool SendShutdownMessage { get; set; }

    public ServerStateSettings(ServerStateSettings settings)
    {
        ServerStateChannel = settings?.ServerStateChannel ?? default(Snowflake);
        SendBootingMessage = settings?.SendBootingMessage ?? true;
        SendOnlineMessage = settings?.SendOnlineMessage ?? true;
        SendShutdownMessage = settings?.SendShutdownMessage ?? true;
    }
}