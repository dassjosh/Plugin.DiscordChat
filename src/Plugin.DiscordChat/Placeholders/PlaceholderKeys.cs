using DiscordChatPlugin.Plugins;
using Oxide.Ext.Discord.Libraries;

namespace DiscordChatPlugin.Placeholders;

public class PlaceholderKeys
{
    public static readonly PlaceholderKey TemplateMessage = new PlaceholderKey(nameof(DiscordChat), "message");
    public static readonly PlaceholderKey PlayerMessage = new PlaceholderKey(nameof(DiscordChat), "player.message");
    public static readonly PlaceholderKey DisconnectReason = new PlaceholderKey(nameof(DiscordChat), "disconnect.reason");
    public static readonly PlaceholderKey PlayerName = new PlaceholderKey(nameof(DiscordChat), "player.name");
    public static readonly PlaceholderKey DiscordTag = new PlaceholderKey(nameof(DiscordChat), "discord.tag");
}