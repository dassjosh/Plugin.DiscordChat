﻿using Newtonsoft.Json;

namespace DiscordChatPlugin.Configuration.Plugins;

public class BetterChatMuteSettings
{
    [JsonProperty("Ignore Muted Players")]
    public bool IgnoreMuted { get; set; }
    
    [JsonProperty("Send Muted Notification")]
    public bool SendMutedNotification { get; set; }

    public BetterChatMuteSettings(BetterChatMuteSettings settings)
    {
        IgnoreMuted = settings?.IgnoreMuted ?? true;
        SendMutedNotification = settings?.SendMutedNotification ?? true;
    }
}