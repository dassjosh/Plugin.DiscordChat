using DiscordChatPlugin.Configuration.Plugins;
using DiscordChatPlugin.Enums;
using DiscordChatPlugin.Templates;
using Oxide.Core.Libraries.Covalence;
using Oxide.Core.Plugins;
using Oxide.Ext.Discord.Entities.Messages;
using Oxide.Ext.Discord.Extensions;

namespace DiscordChatPlugin.Plugins
{
    public partial class DiscordChat
    {
        private AdminChatSettings _adminChatSettings;
        
        private const string AdminChatPermission = "adminchat.use";
        
        //Hook called from AdminChat Plugin
        [HookMethod(nameof(OnAdminChat))]
        public void OnAdminChat(IPlayer player, string message)
        {
            if (IsAdminChatEnabled())
            {
                HandleMessage(message, player, player.GetDiscordUser(), MessageSource.AdminChat, null);
            }
        }
        
        //Message sent in admin chat channel. Process bot replace and sending to server
        public void HandleAdminChatDiscordMessage(DiscordMessage message)
        {
            IPlayer player = message.Author.Player;
            if (player == null)
            {
                message.ReplyWithGlobalTemplate(Client, this, TemplateKeys.Error.AdminChat.NotLinked, null, GetDefault().AddMessage(message));
                return;
            }

            if (!CanPlayerAdminChat(player))
            {
                message.ReplyWithGlobalTemplate(Client, this, TemplateKeys.Error.AdminChat.NoPermission, null, GetDefault().AddPlayer(player).AddMessage(message));
                return;
            }

            HandleMessage(message.Content, player, player.GetDiscordUser(), MessageSource.AdminChat, message);
        }

        public bool IsAdminChatEnabled() => _adminChatSettings.Enabled && Sends.ContainsKey(MessageSource.AdminChat);
        public bool CanPlayerAdminChat(IPlayer player) => _adminChatSettings.Enabled && player.HasPermission(AdminChatPermission);

    }
}