﻿using System;
using DiscordChatPlugin.Enums;
using Oxide.Core.Libraries.Covalence;
using Oxide.Core.Plugins;

namespace DiscordChatPlugin.Plugins
{
    public partial class DiscordChat
    {
        private DateTime GetServerTime()
        {
            return DateTime.Now + TimeSpan.FromHours(_pluginConfig.ChatSettings.ServerTimeOffset);
        }

        public MessageSource GetSourceFromServerChannel(int channel)
        {
            switch (channel)
            {
                case 1:
                    return MessageSource.Team;
                case 3:
                    return MessageSource.Cards;
            }
            
            return MessageSource.Server;
        }
        
        public bool IsPluginLoaded(Plugin plugin) => plugin != null && plugin.IsLoaded;

        public string GetBetterChatTag(IPlayer player)
        {
            return player.IsConnected ? null : _pluginConfig.ChatSettings.DiscordTag;
        }

        public new void Subscribe(string hook)
        {
            base.Subscribe(hook);
        }
        
        public new void Unsubscribe(string hook)
        {
            base.Unsubscribe(hook);
        }
        
        public void Puts(string message)
        {
            base.Puts(message);
        }
    }
}