using Newtonsoft.Json;
using Oxide.Ext.Discord.Entities;

namespace DiscordChatPlugin.Configuration.Plugins;

public class AdminChatSettings
{
    [JsonProperty("Enable AdminChat Plugin Support")]
    public bool Enabled { get; set; }

    [JsonProperty("Chat Channel ID")]
    public Snowflake ChatChannel { get; set; }

    [JsonProperty("Chat Prefix")]
    public string AdminChatPrefix { get; set; }

    [JsonProperty("Replace Discord Message With Bot")]
    public bool ReplaceWithBot { get; set; }

    public AdminChatSettings(AdminChatSettings settings)
    {
        Enabled = settings?.Enabled ?? false;
        ChatChannel = settings?.ChatChannel ?? default(Snowflake);
        AdminChatPrefix = settings?.AdminChatPrefix ?? "@";
        ReplaceWithBot = settings?.ReplaceWithBot ?? true;
    }
}