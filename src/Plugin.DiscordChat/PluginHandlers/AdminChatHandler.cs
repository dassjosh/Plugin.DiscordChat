using DiscordChatPlugin.Configuration.Plugins;
using DiscordChatPlugin.Enums;
using DiscordChatPlugin.Localization;
using DiscordChatPlugin.Plugins;
using Oxide.Core.Libraries.Covalence;
using Oxide.Core.Plugins;
using Oxide.Ext.Discord;
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

        public override bool CanSendMessage(string message, IPlayer player, DiscordUser user, MessageSource source, DiscordMessage sourceMessage)
        {
            return source == MessageSource.AdminChat ? !_settings.Enabled : !IsAdminChatMessage(player, message);
        }

        public override bool SendMessage(string message, IPlayer player, DiscordUser user, MessageSource source, DiscordMessage sourceMessage)
        {
            if (source != MessageSource.AdminChat)
            {
                return false;
            }
            
            PlaceholderData placeholders = Chat.GetDefault().AddPlayer(player).Add(DiscordChat.PlayerMessagePlaceholder, message);
            
            if (sourceMessage != null)
            {
                if (_settings.ReplaceWithBot)
                {
                    Chat.Timer.In(.25f, () => { sourceMessage.DeleteMessage(_client); });
                    Chat.Sends[source]?.QueueMessage(Chat.Lang(LangKeys.Discord.AdminChat.DiscordMessage, null, placeholders));
                }
                    
                Plugin.Call("SendAdminMessage", player, message);
            }
            else
            {
                Chat.Sends[source]?.QueueMessage(Chat.Lang(LangKeys.Discord.AdminChat.ServerMessage, null, placeholders));
            }

            return true;
        }
        
        private bool IsAdminChatMessage(IPlayer player, string message) => Chat.CanPlayerAdminChat(player) && (message.StartsWith(_settings.AdminChatPrefix) || Plugin.Call<bool>("HasAdminChatEnabled", player));
    }
}