using System.Collections.Generic;
using System.Text;
using DiscordChatPlugin.Configuration;
using DiscordChatPlugin.Enums;
using DiscordChatPlugin.Localization;
using DiscordChatPlugin.Plugins;
using Oxide.Core;
using Oxide.Core.Libraries.Covalence;
using Oxide.Core.Plugins;
using Oxide.Ext.Discord.Entities;
using Oxide.Ext.Discord.Extensions;
using Oxide.Ext.Discord.Libraries;

namespace DiscordChatPlugin.PluginHandlers;

public class DiscordChatHandler : BasePluginHandler
{
    private readonly ChatSettings _settings;
    private readonly IServer _server;
    private readonly object[] _unlinkedArgs = new object[3];

    public DiscordChatHandler(DiscordChat chat, ChatSettings settings, Plugin plugin, IServer server) : base(chat, plugin)
    {
        _settings = settings;
        _server = server;
        _unlinkedArgs[0] = 2;
        _unlinkedArgs[1] = settings.UnlinkedSettings.SteamIcon;
    }

    public override bool CanSendMessage(string message, IPlayer player, DiscordUser user, MessageSource source, DiscordMessage sourceMessage)
    {
        if (sourceMessage != null)
        {
            if (_settings.Filter.IgnoreMessage(sourceMessage, Chat.Client.Bot.GetGuild(sourceMessage.GuildId)?.Members[sourceMessage.Author.Id]))
            {
                return false;
            }
        }

        switch (source)
        {
            case MessageSource.Discord:
                return _settings.DiscordToServer && (_settings.UnlinkedSettings.AllowedUnlinked || (player != null && player.IsLinked()));
 
            case MessageSource.Server:
                return _settings.ServerToDiscord;
        }

        return true;
    }

    public override bool SendMessage(string message, IPlayer player, DiscordUser user, MessageSource source, DiscordMessage sourceMessage, PlaceholderData data)
    {
        switch (source)
        {
            case MessageSource.Discord:
                if (_settings.UseBotToDisplayChat)
                {
                    if (player.IsLinked())
                    {
                        Chat.Sends[MessageSource.Discord]?.QueueMessage(Chat.Lang(LangKeys.Discord.Chat.LinkedMessage, data));
                    }
                    else
                    {
                        Chat.Sends[MessageSource.Discord]?.QueueMessage(Chat.Lang(LangKeys.Discord.Chat.UnlinkedMessage, data));
                    }
                }

                if (player.IsLinked())
                {
                    SendLinkedToServer(player, message, data);
                }
                else
                {
                    SendUnlinkedToServer(data);
                }

                return true;
                
            case MessageSource.Server:
                Chat.Sends[MessageSource.Discord]?.QueueMessage(Chat.Lang(LangKeys.Discord.Chat.Server, data));
                return true;
            case MessageSource.Team:
                Chat.Sends[MessageSource.Team]?.QueueMessage(Chat.Lang(LangKeys.Discord.Team.Message, data));
                return true;
            case MessageSource.Cards:
                Chat.Sends[MessageSource.Cards]?.QueueMessage(Chat.Lang(LangKeys.Discord.Cards.Message, data));
                return true;
            case MessageSource.Clan:
                Chat.Sends[MessageSource.Clan]?.QueueMessage(Chat.Lang(LangKeys.Discord.Clans.Message, data));
                return true;
            case MessageSource.PluginAdminChat:
                Chat.Sends[MessageSource.PluginAdminChat]?.QueueMessage(Chat.Lang(LangKeys.Discord.AdminChat.DiscordMessage, data));
                return true;
            case MessageSource.PluginClan:
                Chat.Sends[MessageSource.PluginClan]?.QueueMessage(Chat.Lang(LangKeys.Discord.PluginClans.ClanMessage, data));
                return true;
            case MessageSource.PluginAlliance:
                Chat.Sends[MessageSource.PluginAlliance]?.QueueMessage(Chat.Lang(LangKeys.Discord.PluginClans.AllianceMessage, data));
                return true;
        }

        return false;
    }

    public void SendLinkedToServer(IPlayer player, string message, PlaceholderData placeholders)
    {
        if (_settings.AllowPluginProcessing)
        {
            bool playerReturn = false;
#if RUST
            //Let other chat plugins process first
            if (player.Object != null)
            {
                Chat.Unsubscribe("OnPlayerChat");
                playerReturn = Interface.Call("OnPlayerChat", player.Object, message, ConVar.Chat.ChatChannel.Global) != null;
                Chat.Subscribe("OnPlayerChat");
            }
#endif

            //Let other chat plugins process first
            Chat.Unsubscribe("OnUserChat");
            bool userReturn = Interface.Call("OnUserChat", player, message) != null;
            Chat.Subscribe("OnUserChat");

            if (playerReturn || userReturn)
            {
                return;
            }
                
            if (Chat.SendBetterChatMessage(player, message))
            {
                return;
            }
        }
            
        message = Chat.Lang(LangKeys.Server.LinkedMessage, placeholders);
        _server.Broadcast(message);
        Chat.Puts(Formatter.ToPlaintext(message));
    }

    public void SendUnlinkedToServer(PlaceholderData placeholders)
    {
        string serverMessage = Chat.Lang(LangKeys.Server.UnlinkedMessage, placeholders);
#if RUST
        _unlinkedArgs[2] = Formatter.ToUnity(serverMessage);
        ConsoleNetwork.BroadcastToAllClients("chat.add", _unlinkedArgs);
#else
            _server.Broadcast(serverMessage);
#endif
            
        Chat.Puts(Formatter.ToPlaintext(serverMessage));
    }

    public override void ProcessMessage(StringBuilder message, IPlayer player, DiscordUser user, MessageSource source)
    {
        foreach (KeyValuePair<string, string> replacement in _settings.TextReplacements)
        {
            message.Replace(replacement.Key, replacement.Value);
        }
    }
}