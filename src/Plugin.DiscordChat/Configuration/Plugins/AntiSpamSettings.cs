using Newtonsoft.Json;

namespace DiscordChatPlugin.Configuration.Plugins;

public class AntiSpamSettings
{
    [JsonProperty("Use AntiSpam On Player Names")]
    public bool PlayerName { get; set; }

    [JsonProperty("Use AntiSpam On Server Messages")]
    public bool ServerMessage { get; set; }

    [JsonProperty("Use AntiSpam On Chat Messages")]
    public bool DiscordMessage { get; set; }
        
    [JsonProperty("Use AntiSpam On Plugin Messages")]
    public bool PluginMessage { get; set; }

#if RUST
    [JsonProperty("Use AntiSpam On Team Messages")]
    public bool TeamMessage { get; set; }

    [JsonProperty("Use AntiSpam On Card Messages")]
    public bool CardMessages { get; set; }
        
    [JsonProperty("Use AntiSpam On Clan Messages")]
    public bool ClanMessages { get; set; }
#endif

    public AntiSpamSettings(AntiSpamSettings settings)
    {
        PlayerName = settings?.PlayerName ?? false;
        ServerMessage = settings?.ServerMessage ?? false;
        DiscordMessage = settings?.DiscordMessage ?? false;
        PluginMessage = settings?.PluginMessage ?? false;
#if RUST
        TeamMessage = settings?.TeamMessage ?? false;
        CardMessages = settings?.CardMessages ?? false;
        ClanMessages = settings?.ClanMessages ?? false;
#endif
    }
}