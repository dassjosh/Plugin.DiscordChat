using Newtonsoft.Json;

namespace DiscordChatPlugin.Configuration.Plugins
{
    public class UFilterSettings
    {
        [JsonProperty("Use UFilter On Player Names")]
        public bool PlayerNames { get; set; }

        [JsonProperty("Use UFilter On Server Messages")]
        public bool ServerMessage { get; set; }

        [JsonProperty("Use UFilter On Discord Messages")]
        public bool DiscordMessages { get; set; }
        
        [JsonProperty("Use UFilter On Plugin Messages")]
        public bool PluginMessages { get; set; }
        
#if RUST
        [JsonProperty("Use UFilter On Team Messages")]
        public bool TeamMessage { get; set; }

        [JsonProperty("Use UFilter On Card Messages")]
        public bool CardMessage { get; set; }
#endif

        [JsonProperty("Replacement Character")]
        public char ReplacementCharacter { get; set; }
        
        public UFilterSettings(UFilterSettings settings)
        {
            PlayerNames = settings?.PlayerNames ?? false;
            ServerMessage = settings?.ServerMessage ?? false;
            DiscordMessages = settings?.DiscordMessages ?? false;
            PluginMessages = settings?.PluginMessages ?? false;
#if RUST
            TeamMessage = settings?.TeamMessage ?? false;
            CardMessage = settings?.CardMessage ?? false;
#endif

            ReplacementCharacter = settings?.ReplacementCharacter ?? '＊';
        }
    }
}