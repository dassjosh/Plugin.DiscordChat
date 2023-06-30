using DiscordChatPlugin.Placeholders;
using Oxide.Core.Libraries.Covalence;
using Oxide.Ext.Discord.Libraries.Placeholders;

namespace DiscordChatPlugin.Plugins
{
    public partial class DiscordChat
    {
        public void RegisterPlaceholders()
        {
            _placeholders.RegisterPlaceholder<string>(this, PlaceholderKeys.Message, PlaceholderKeys.Data.Message);
            _placeholders.RegisterPlaceholder<string>(this, PlaceholderKeys.PlayerMessage, PlaceholderKeys.Data.PlayerMessage);
            _placeholders.RegisterPlaceholder<string>(this, PlaceholderKeys.DisconnectReason, PlaceholderKeys.Data.DisconnectReason);
            _placeholders.RegisterPlaceholder<IPlayer, string>(this, PlaceholderKeys.PlayerName, PlaceholderKeys.Data.PlayerName, GetPlayerName);
            _placeholders.RegisterPlaceholder(this, PlaceholderKeys.DiscordTag, _pluginConfig.ChatSettings.DiscordTag);
        }

        public PlaceholderData GetDefault()
        {
            return _placeholders.CreateData(this);
        }
    }
}