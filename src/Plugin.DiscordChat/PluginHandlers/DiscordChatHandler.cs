using DiscordChatPlugin.Configuration;
using DiscordChatPlugin.Enums;
using DiscordChatPlugin.Localization;
using DiscordChatPlugin.Placeholders;
using DiscordChatPlugin.Plugins;
using Oxide.Core;
using Oxide.Core.Libraries.Covalence;
using Oxide.Core.Plugins;
using Oxide.Ext.Discord.Entities.Messages;
using Oxide.Ext.Discord.Entities.Users;
using Oxide.Ext.Discord.Extensions;
using Oxide.Ext.Discord.Libraries.Placeholders;

namespace DiscordChatPlugin.PluginHandlers
{
    public class DiscordChatHandler : BasePluginHandler
    {
        private readonly ChatSettings _settings;
        private readonly IServer _server;

        public DiscordChatHandler(DiscordChat chat, ChatSettings settings, Plugin plugin, IServer server) : base(chat, plugin)
        {
            _settings = settings;
            _server = server;
        }

        public override bool CanSendMessage(string message, IPlayer player, DiscordUser user, MessageSource source, DiscordMessage sourceMessage)
        {
            if (sourceMessage != null)
            {
                if (_settings.Filter.IgnoreMessage(sourceMessage, Chat.Client.Bot.GetGuild(sourceMessage.GuildId).Members[sourceMessage.Author.Id]))
                {
                    return false;
                }
            }

            switch (source)
            {
                case MessageSource.Discord:
                    return _settings.DiscordToServer && (!_settings.UnlinkedSettings.AllowedUnlinked || player.IsLinked());
 
                case MessageSource.Server:
                    return _settings.ServerToDiscord;
            }

            return true;
        }

        public override bool SendMessage(string message, IPlayer player, DiscordUser user, MessageSource source, DiscordMessage sourceMessage)
        {
            switch (source)
            {
                case MessageSource.Discord:
                    if (_settings.UseBotToDisplayChat)
                    {
                        if (player.IsLinked())
                        {
                            Chat.Sends[MessageSource.Discord].QueueMessage(Chat.ProcessPlaceholders(LangKeys.Discord.Chat.LinkedMessage, GetPlaceholders(message, player, user, sourceMessage)));
                        }
                        else
                        {
                            Chat.Sends[MessageSource.Discord].QueueMessage(Chat.ProcessPlaceholders(LangKeys.Discord.Chat.UnlinkedMessage, GetPlaceholders(message, player, user, sourceMessage)));
                        }
                    }

                    if (player.IsLinked())
                    {
                        SendLinkedToServer(player, message, sourceMessage);
                    }
                    else
                    {
                        SendUnlinkedToServer(sourceMessage, message);
                    }

                    return false;
                
                case MessageSource.Server:
                    Chat.Sends[MessageSource.Discord].QueueMessage(Chat.ProcessPlaceholders(LangKeys.Discord.Chat.Server, GetPlaceholders(message, player, user, sourceMessage)));
                    return false;
                case MessageSource.Team:
                    Chat.Sends[MessageSource.Team].QueueMessage(Chat.ProcessPlaceholders(LangKeys.Discord.Team.Message, GetPlaceholders(message, player, user, sourceMessage)));
                    return false;
                case MessageSource.Cards:
                    Chat.Sends[MessageSource.Cards].QueueMessage(Chat.ProcessPlaceholders(LangKeys.Discord.Cards.Message, GetPlaceholders(message, player, user, sourceMessage)));
                    return false;
                case MessageSource.AdminChat:
                    Chat.Sends[MessageSource.AdminChat].QueueMessage(Chat.ProcessPlaceholders(LangKeys.Discord.AdminChat.DiscordMessage, GetPlaceholders(message, player, user, sourceMessage)));
                    return false;
                case MessageSource.ClanChat:
                    Chat.Sends[MessageSource.ClanChat].QueueMessage(Chat.ProcessPlaceholders(LangKeys.Discord.Clans.ClanMessage, GetPlaceholders(message, player, user, sourceMessage)));
                    return false;
                case MessageSource.AllianceChat:
                    Chat.Sends[MessageSource.AllianceChat].QueueMessage(Chat.ProcessPlaceholders(LangKeys.Discord.Clans.AllianceMessage, GetPlaceholders(message, player, user, sourceMessage)));
                    return false;
            }

            return true;
        }

        public void SendLinkedToServer(IPlayer player, string message, DiscordMessage source)
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

            PlaceholderData data = Chat.GetDefault().AddPlayer(player).AddMessage(source).Add(PlaceholderKeys.Data.PlayerMessage, message);
            message = Chat.ProcessPlaceholders(LangKeys.Server.LinkedMessage, data);
            _server.Broadcast(message);
            Chat.Puts(Formatter.ToPlaintext(message));
        }

        public void SendUnlinkedToServer(DiscordMessage sourceMessage, string message)
        {
            if (_settings.UseBotToDisplayChat)
            {
                Chat.Sends[MessageSource.Server]?.QueueMessage(Chat.ProcessPlaceholders(LangKeys.Discord.Chat.UnlinkedMessage, Chat.GetDefault().AddMessage(sourceMessage).Add(PlaceholderKeys.Data.PlayerMessage, message)));
            }

            string serverMessage = Chat.ProcessPlaceholders(LangKeys.Server.UnlinkedMessage, Chat.GetDefault().AddMessage(sourceMessage).AddGuildMember(Chat.Client.Bot.GetGuild(sourceMessage.GuildId).Members[sourceMessage.Author.Id]).Add(PlaceholderKeys.Data.PlayerMessage, message));
            _server.Broadcast(serverMessage);
            Chat.Puts(Formatter.ToPlaintext(serverMessage));
        }

        private PlaceholderData GetPlaceholders(string message, IPlayer player, DiscordUser user, DiscordMessage sourceMessage)
        {
            return Chat.GetDefault().AddPlayer(player).AddUser(user).AddMessage(sourceMessage).Add(PlaceholderKeys.Data.PlayerMessage, message);
        }
    }
}