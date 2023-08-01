using DiscordChatPlugin.Enums;
using Oxide.Core.Libraries.Covalence;
using Oxide.Core.Plugins;
using Oxide.Ext.Discord.Entities;
using Oxide.Ext.Discord.Entities.Channels;
using Oxide.Ext.Discord.Entities.Guilds;

namespace DiscordChatPlugin.Plugins
{
    public partial class DiscordChat
    {
        public MessageSource GetSourceFromServerChannel(int channel)
        {
            switch (channel)
            {
                case 1:
                    return MessageSource.Team;
                case 3:
                    return MessageSource.Cards;
                case 5:
                    return MessageSource.Clan;
            }
            
            return MessageSource.Server;
        }

        public DiscordChannel FindChannel(Snowflake channelId)
        {
            if (!channelId.IsValid())
            {
                return null;
            }
            
            foreach (DiscordGuild guild in Client.Bot.Servers.Values)
            {
                DiscordChannel channel = guild.Channels[channelId];
                if (channel != null)
                {
                    return channel;
                }
            }

            return null;
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