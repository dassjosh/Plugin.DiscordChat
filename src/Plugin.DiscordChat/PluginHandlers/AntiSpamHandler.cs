using System.Text;
using DiscordChatPlugin.Configuration.Plugins;
using DiscordChatPlugin.Enums;
using DiscordChatPlugin.Plugins;
using Oxide.Core.Libraries.Covalence;
using Oxide.Core.Plugins;
using Oxide.Ext.Discord.Entities.Users;

namespace DiscordChatPlugin.PluginHandlers
{
    public class AntiSpamHandler : BasePluginHandler
    {
        private readonly AntiSpamSettings _settings;

        public AntiSpamHandler(DiscordChat chat, AntiSpamSettings settings, Plugin plugin) : base(chat, plugin)
        {
            _settings = settings;
        }

        public override void ProcessPlayerName(StringBuilder name, IPlayer player)
        {
            if (!_settings.PlayerName || player == null)
            {
                return;
            }
            
            name.Length = 0;
            name.Append(Plugin.Call<string>("GetClearName", player));
        }

        public override void ProcessMessage(StringBuilder message, IPlayer player, DiscordUser user, MessageSource source)
        {
            if (CanFilterMessage(source))
            {
                string clearMessage = Plugin.Call<string>("GetSpamFreeText", message.ToString());
                message.Length = 0;
                message.Append(clearMessage);
            }
        }
        
        private bool CanFilterMessage(MessageSource source)
        {
            switch (source)
            {
                case MessageSource.Discord:
                    return _settings.DiscordMessage;
                case MessageSource.Server:
                    return _settings.ServerMessage;
                case MessageSource.Team:
                    return _settings.TeamMessage;
                case MessageSource.Cards:
                    return _settings.CardMessages;
                case MessageSource.ClanChat:
                case MessageSource.AllianceChat:
                    return _settings.PluginMessage;
            }

            return false;
        }
    }
}