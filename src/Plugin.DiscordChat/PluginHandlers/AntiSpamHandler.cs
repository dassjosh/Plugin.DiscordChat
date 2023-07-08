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

            string builtName = name.ToString();
            builtName = Plugin.Call<string>("GetSpamFreeText", builtName);
            builtName = Plugin.Call<string>("GetImpersonationFreeText", builtName);
            name.Length = 0;
            name.Append(builtName);
        }

        public override void ProcessMessage(StringBuilder message, IPlayer player, DiscordUser user, MessageType type)
        {
            if (CanFilterMessage(type))
            {
                string clearMessage = Plugin.Call<string>("GetSpamFreeText", message.ToString());
                message.Length = 0;
                message.Append(clearMessage);
            }
        }
        
        private bool CanFilterMessage(MessageType type)
        {
            switch (type)
            {
                case MessageType.Discord:
                    return _settings.DiscordMessage;
                case MessageType.Server:
                    return _settings.ServerMessage;
                case MessageType.Team:
                    return _settings.TeamMessage;
                case MessageType.Cards:
                    return _settings.CardMessages;
                case MessageType.Clan:
                case MessageType.Alliance:
                    return _settings.PluginMessage;
            }

            return false;
        }
    }
}