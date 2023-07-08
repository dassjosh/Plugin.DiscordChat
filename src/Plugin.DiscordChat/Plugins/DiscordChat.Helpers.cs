using DiscordChatPlugin.Enums;
using Oxide.Core.Libraries.Covalence;
using Oxide.Core.Plugins;

namespace DiscordChatPlugin.Plugins
{
    public partial class DiscordChat
    {
        public MessageType GetSourceFromServerChannel(int channel)
        {
            switch (channel)
            {
                case 1:
                    return MessageType.Team;
                case 3:
                    return MessageType.Cards;
            }
            
            return MessageType.Server;
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