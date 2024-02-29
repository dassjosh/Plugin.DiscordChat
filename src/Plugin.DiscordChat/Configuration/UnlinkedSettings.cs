using Newtonsoft.Json;

namespace DiscordChatPlugin.Configuration;

public class UnlinkedSettings
{
    [JsonProperty("Allow Unlinked Players To Chat With Server")]
    public bool AllowedUnlinked { get; set; }

#if RUST
    [JsonProperty("Steam Icon ID")]
    public ulong SteamIcon { get; set; }
#endif

    public UnlinkedSettings(UnlinkedSettings settings)
    {
        AllowedUnlinked = settings?.AllowedUnlinked ?? true;
#if RUST
        SteamIcon = settings?.SteamIcon ?? 76561199144296099;
#endif
    }
}