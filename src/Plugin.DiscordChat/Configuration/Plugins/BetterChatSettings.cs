using Newtonsoft.Json;

namespace DiscordChatPlugin.Configuration.Plugins;

public class BetterChatSettings
{
    [JsonProperty("Max BetterChat Tags To Show When Sent From Discord")]
    public byte ServerMaxTags { get; set; }
    
    [JsonProperty("Max BetterChat Tags To Show When Sent From Server")]
    public byte DiscordMaxTags { get; set; }

    public BetterChatSettings(BetterChatSettings settings)
    {
        ServerMaxTags = settings?.ServerMaxTags ?? 10;
        DiscordMaxTags = settings?.DiscordMaxTags ?? 10;
    }
}