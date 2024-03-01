using Oxide.Ext.Discord.Libraries;

namespace DiscordChatPlugin.Placeholders;

public class PlaceholderDataKeys
{
    public static readonly PlaceholderDataKey TemplateMessage = new("message");
    public static readonly PlaceholderDataKey PlayerMessage = new("player.message");
    public static readonly PlaceholderDataKey DisconnectReason = new("reason");
}