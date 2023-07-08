using System;
using DiscordChatPlugin.Configuration.Plugins;
using DiscordChatPlugin.Enums;
using DiscordChatPlugin.Plugins;
using Oxide.Core.Libraries.Covalence;
using Oxide.Core.Plugins;
using Oxide.Ext.Discord.Entities.Users;

namespace DiscordChatPlugin.PluginHandlers
{
    public class TranslationApiHandler : BasePluginHandler
    {
        private readonly ChatTranslatorSettings _settings;

        public TranslationApiHandler(DiscordChat chat, ChatTranslatorSettings settings, Plugin plugin) : base(chat, plugin)
        {
            _settings = settings;
        }

        public override bool HasCallbackMessage() => true;

        public override void ProcessCallbackMessage(string message, IPlayer player, DiscordUser user, MessageType type, Action<string> callback)
        {
            if (CanChatTranslatorSource(type))
            {
                Plugin.Call("Translate", message, _settings.DiscordServerLanguage, "auto", callback);
                return;
            }

            callback.Invoke(message);
        }

        public bool CanChatTranslatorSource(MessageType type)
        {
            if (!_settings.Enabled)
            {
                return false;
            }
            
            switch (type)
            {
                case MessageType.Server:
                    return _settings.ServerMessage;

                case MessageType.Discord:
                    return _settings.DiscordMessage;
                
                case MessageType.Clan:
                case MessageType.Alliance:
                    return _settings.PluginMessage;

#if RUST
                case MessageType.Team:
                    return _settings.TeamMessage;

                case MessageType.Cards:
                    return _settings.CardMessages;
#endif
            }

            return false;
        }
    }
}