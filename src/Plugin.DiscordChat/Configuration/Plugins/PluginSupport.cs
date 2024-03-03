using Newtonsoft.Json;

namespace DiscordChatPlugin.Configuration.Plugins;

public class PluginSupport
{
    [JsonProperty("AdminChat Settings")]
    public AdminChatSettings AdminChat { get; set; }

    [JsonProperty("AntiSpam Settings")]
    public AntiSpamSettings AntiSpam { get; set; }

    [JsonProperty("BetterChat Settings")]
    public BetterChatSettings BetterChat { get; set; }
    
    [JsonProperty("BetterChatMute Settings")]
    public BetterChatMuteSettings BetterChatMute { get; set; }

    [JsonProperty("ChatTranslator Settings")]
    public ChatTranslatorSettings ChatTranslator { get; set; }

    [JsonProperty("Clan Settings")]
    public ClansSettings Clans { get; set; }

    [JsonProperty("UFilter Settings")]
    public UFilterSettings UFilter { get; set; }

    public PluginSupport(PluginSupport settings)
    {
        AdminChat = new AdminChatSettings(settings?.AdminChat);
        AntiSpam = new AntiSpamSettings(settings?.AntiSpam);
        BetterChat = new BetterChatSettings(settings?.BetterChat);
        BetterChatMute = new BetterChatMuteSettings(settings?.BetterChatMute);
        ChatTranslator = new ChatTranslatorSettings(settings?.ChatTranslator);
        Clans = new ClansSettings(settings?.Clans);
        UFilter = new UFilterSettings(settings?.UFilter);
    }
}