using Newtonsoft.Json;
using Oxide.Core;
using Oxide.Core.Libraries;

namespace DiscordChatPlugin.Configuration.Plugins;

public class ChatTranslatorSettings
{
    [JsonProperty("Enable Chat Translator")]
    public bool Enabled { get; set; }

    [JsonProperty("Use ChatTranslator On Server Messages")]
    public bool ServerMessage { get; set; }

    [JsonProperty("Use ChatTranslator On Chat Messages")]
    public bool DiscordMessage { get; set; }
        
    [JsonProperty("Use ChatTranslator On Plugin Messages")]
    public bool PluginMessage { get; set; }

#if RUST
    [JsonProperty("Use ChatTranslator On Team Messages")]
    public bool TeamMessage { get; set; }

    [JsonProperty("Use ChatTranslator On Card Messages")]
    public bool CardMessages { get; set; }
        
    [JsonProperty("Use ChatTranslator On Clan Messages")]
    public bool ClanMessages { get; set; }
#endif

    [JsonProperty("Discord Server Chat Language")]
    public string DiscordServerLanguage { get; set; }

    public ChatTranslatorSettings(ChatTranslatorSettings settings)
    {
        Enabled = settings?.Enabled ?? false;
        ServerMessage = settings?.ServerMessage ?? false;
        DiscordMessage = settings?.DiscordMessage ?? false;
#if RUST
        TeamMessage = settings?.TeamMessage ?? false;
        CardMessages = settings?.CardMessages ?? false;
        ClanMessages = settings?.ClanMessages ?? false;
#endif
        DiscordServerLanguage = settings?.DiscordServerLanguage ?? Interface.Oxide.GetLibrary<Lang>().GetServerLanguage();
    }
}