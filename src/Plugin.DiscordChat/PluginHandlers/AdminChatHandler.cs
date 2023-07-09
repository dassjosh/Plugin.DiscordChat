using DiscordChatPlugin.Configuration.Plugins;
using DiscordChatPlugin.Localization;
using DiscordChatPlugin.Placeholders;
using DiscordChatPlugin.Plugins;
using Oxide.Core.Libraries.Covalence;
using Oxide.Core.Plugins;
using Oxide.Ext.Discord.Clients;
using Oxide.Ext.Discord.Entities.Messages;
using Oxide.Ext.Discord.Entities.Users;
using Oxide.Ext.Discord.Libraries.Placeholders;

namespace DiscordChatPlugin.PluginHandlers
{
    public class AdminChatHandler : BasePluginHandler
    {
        private readonly AdminChatSettings _settings;
        private readonly DiscordClient _client;

        public AdminChatHandler(DiscordClient client, DiscordChat chat, AdminChatSettings settings, Plugin plugin) : base(chat, plugin)
        {
            _client = client;
            _settings = settings;
        }

        public override bool CanSendMessage(string message, IPlayer player, DiscordUser user, Enums.MessageSource source, DiscordMessage sourceMessage)
        {
            return source == Enums.MessageSource.AdminChat ? !_settings.Enabled : !IsAdminChatMessage(player, message);
        }

        public override bool SendMessage(string message, IPlayer player, DiscordUser user, Enums.MessageSource source, DiscordMessage sourceMessage)
        {
            if (source != Enums.MessageSource.AdminChat)
            {
                return false;
            }
            
            PlaceholderData placeholders = Chat.GetDefault().AddPlayer(player).Add(PlaceholderKeys.Data.PlayerMessage, message);
            
            if (sourceMessage != null)
            {
                if (_settings.ReplaceWithBot)
                {
                    sourceMessage.Delete(_client);
                    Chat.Sends[source]?.QueueMessage(Chat.Lang(LangKeys.Discord.AdminChat.DiscordMessage, placeholders));
                }
                    
                Plugin.Call("SendAdminMessage", player, message);
            }
            else
            {
                Chat.Sends[source]?.QueueMessage(Chat.Lang(LangKeys.Discord.AdminChat.ServerMessage, placeholders));
            }

            return true;
        }
        
        private bool IsAdminChatMessage(IPlayer player, string message) => Chat.CanPlayerAdminChat(player) && (message.StartsWith(_settings.AdminChatPrefix) || Plugin.Call<bool>("HasAdminChatEnabled", player));
    }
}