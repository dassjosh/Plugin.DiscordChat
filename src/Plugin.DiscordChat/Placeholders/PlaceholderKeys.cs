using DiscordChatPlugin.Plugins;
using Oxide.Ext.Discord.Libraries;

namespace DiscordChatPlugin.Placeholders;

public class PlaceholderKeys
{
    public static readonly PlaceholderKey TemplateMessage = new(nameof(DiscordChat), "message");
    public static readonly PlaceholderKey PlayerMessage = new(nameof(DiscordChat), "player.message");
    public static readonly PlaceholderKey DisconnectReason = new(nameof(DiscordChat), "disconnect.reason");
    public static readonly PlaceholderKey PlayerName = new(nameof(DiscordChat), "player.name");
    public static readonly PlaceholderKey DiscordTag = new(nameof(DiscordChat), "discord.tag");
}