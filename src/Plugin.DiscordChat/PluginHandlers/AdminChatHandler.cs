using DiscordChatPlugin.Configuration.Plugins;
using DiscordChatPlugin.Enums;
using DiscordChatPlugin.Localization;
using DiscordChatPlugin.Plugins;
using Oxide.Core.Libraries.Covalence;
using Oxide.Core.Plugins;
using Oxide.Ext.Discord.Clients;
using Oxide.Ext.Discord.Entities;
using Oxide.Ext.Discord.Libraries;

namespace DiscordChatPlugin.PluginHandlers;

public class AdminChatHandler : BasePluginHandler
{
    private readonly AdminChatSettings _settings;
    private readonly DiscordClient _client;

    public AdminChatHandler(DiscordClient client, DiscordChat chat, AdminChatSettings settings, Plugin plugin) : base(chat, plugin)
    {
        _client = client;
        _settings = settings;
    }

    public override bool CanSendMessage(string message, IPlayer player, DiscordUser user, MessageSource source, DiscordMessage sourceMessage)
    {
        return source == MessageSource.PluginAdminChat ? !_settings.Enabled : !IsAdminChatMessage(player, message);
    }

    public override bool SendMessage(string message, IPlayer player, DiscordUser user, MessageSource source, DiscordMessage sourceMessage, PlaceholderData data)
    {
        if (source != MessageSource.PluginAdminChat)
        {
            return false;
        }

        if (sourceMessage != null)
        {
            if (_settings.ReplaceWithBot)
            {
                sourceMessage.Delete(_client);
                Chat.Sends[source]?.QueueMessage(Chat.Lang(LangKeys.Discord.AdminChat.DiscordMessage, data));
            }
                    
            Plugin.Call("SendAdminMessage", player, message);
        }
        else
        {
            Chat.Sends[source]?.QueueMessage(Chat.Lang(LangKeys.Discord.AdminChat.ServerMessage, data));
        }

        return true;
    }
        
    private bool IsAdminChatMessage(IPlayer player, string message) => Chat.CanPlayerAdminChat(player) && (message.StartsWith(_settings.AdminChatPrefix) || Plugin.Call<bool>("HasAdminChatEnabled", player));
}