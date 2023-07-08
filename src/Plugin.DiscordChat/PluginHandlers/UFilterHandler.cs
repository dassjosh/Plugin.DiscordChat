using System.Text;
using DiscordChatPlugin.Configuration.Plugins;
using DiscordChatPlugin.Enums;
using DiscordChatPlugin.Plugins;
using Oxide.Core.Libraries.Covalence;
using Oxide.Core.Plugins;
using Oxide.Ext.Discord.Entities.Users;

namespace DiscordChatPlugin.PluginHandlers
{
    public class UFilterHandler : BasePluginHandler
    {
        private readonly UFilterSettings _settings;

        public UFilterHandler(DiscordChat chat, UFilterSettings settings, Plugin plugin) : base(chat, plugin)
        {
            _settings = settings;
        }

        public override void ProcessPlayerName(StringBuilder name, IPlayer player)
        {
            if (_settings.PlayerNames)
            {
                UFilterText(name);
            }
        }

        public override void ProcessMessage(StringBuilder message, IPlayer player, DiscordUser user, MessageType type)
        {
            if (CanFilterMessage(type))
            {
                UFilterText(message);
            }
        }

        private bool CanFilterMessage(MessageType type)
        {
            switch (type)
            {
                case MessageType.Discord:
                    return _settings.DiscordMessages;
                case MessageType.Server:
                    return _settings.ServerMessage;
                case MessageType.Team:
                    return _settings.TeamMessage;
                case MessageType.Cards:
                    return _settings.CardMessage;
                case MessageType.Clan:
                case MessageType.Alliance:
                    return _settings.PluginMessages;
            }

            return false;
        }

        private void UFilterText(StringBuilder text)
        {
            string[] profanities = Plugin.Call<string[]>("Profanities", text.ToString());
            for (int index = 0; index < profanities.Length; index++)
            {
                string profanity = profanities[index];
                text.Replace(profanity, new string(_settings.ReplacementCharacter, profanity.Length));
            }
        }
    }
}