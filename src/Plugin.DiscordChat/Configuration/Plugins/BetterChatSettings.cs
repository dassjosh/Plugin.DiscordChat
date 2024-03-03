using Newtonsoft.Json;

namespace DiscordChatPlugin.Configuration.Plugins;

public class BetterChatSettings
{
    [JsonProperty("Max BetterChat Tags To Show When Sent From Discord")]
    public byte MaxTags { get; set; }

    public BetterChatSettings(BetterChatSettings settings)
    {
        MaxTags = settings?.MaxTags ?? 3;
    }
}