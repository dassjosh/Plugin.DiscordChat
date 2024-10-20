﻿using System.Collections.Generic;
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
        
        Dictionary<string, object> data = GetBetterChatMessageData(player, message);
        if (source == MessageSource.Discord && !string.IsNullOrEmpty(_pluginConfig.ChatSettings.DiscordTag))
        {
            BetterChatSettings settings = _pluginConfig.PluginSupport.BetterChat;
            List<string> titles = GetBetterChatTags(data);
            if (titles != null)
            {
                titles.Add(_pluginConfig.ChatSettings.DiscordTag);
                while (titles.Count > settings.ServerMaxTags)
                {
                    titles.RemoveAt(0);
                }
            }
        }
        BetterChat.Call("API_SendMessage", data);
        return true;
    }
    
    public Dictionary<string, object> GetBetterChatMessageData(IPlayer player, string message)
    {
        return BetterChat.Call<Dictionary<string, object>>("API_GetMessageData", player, message);
    }

    public List<string> GetBetterChatTags(Dictionary<string, object> data)
    {
        if (data["Titles"] is List<string> titles)
        {
            titles.RemoveAll(string.IsNullOrWhiteSpace);
            for (int index = 0; index < titles.Count; index++)
            {
                string title = titles[index];
            }

            return titles;
        }
        
        return null;
    }
}