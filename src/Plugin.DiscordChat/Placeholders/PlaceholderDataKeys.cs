using Oxide.Ext.Discord.Libraries.Placeholders;

namespace DiscordChatPlugin.Placeholders
{
    public class PlaceholderDataKeys
    {
        public static readonly PlaceholderDataKey TemplateMessage = new PlaceholderDataKey("message");
        public static readonly PlaceholderDataKey PlayerMessage = new PlaceholderDataKey("player.message");
        public static readonly PlaceholderDataKey DisconnectReason = new PlaceholderDataKey("reason");
    }
}