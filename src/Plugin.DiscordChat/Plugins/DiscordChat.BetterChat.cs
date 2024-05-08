using System.Collections.Generic;
using DiscordChatPlugin.Configuration.Plugins;
using DiscordChatPlugin.Enums;
using Oxide.Core.Libraries.Covalence;

namespace DiscordChatPlugin.Plugins;

public partial class DiscordChat
{
    public bool SendBetterChatMessage(IPlayer player, string message, MessageSource source)
    {
        if (!IsPluginLoaded(BetterChat))
        {
            return false;
        }
        
        Dictionary<string, object> data = BetterChat.Call<Dictionary<string, object>>("API_GetMessageData", player, message);
        if (source == MessageSource.Discord && !string.IsNullOrEmpty(_pluginConfig.ChatSettings.DiscordTag))
        {
            BetterChatSettings settings = _pluginConfig.PluginSupport.BetterChat;
            if (data["Titles"] is List<string> titles)
            {
                titles.Add(_pluginConfig.ChatSettings.DiscordTag);
                while (titles.Count > settings.MaxTags)
                {
                    titles.RemoveAt(0);
                }
            }
        }
        BetterChat.Call("API_SendMessage", data);
        return true;
    }
}