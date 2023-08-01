﻿using System.Text;
using DiscordChatPlugin.Configuration.Plugins;
using DiscordChatPlugin.Enums;
using DiscordChatPlugin.Plugins;
using Oxide.Core.Libraries.Covalence;
using Oxide.Core.Plugins;
using Oxide.Ext.Discord.Entities.Users;
using Oxide.Plugins;

namespace DiscordChatPlugin.PluginHandlers
{
    public class UFilterHandler : BasePluginHandler
    {
        private readonly UFilterSettings _settings;
        private readonly Hash<int, string> _replacements = new Hash<int, string>();

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

        public override void ProcessMessage(StringBuilder message, IPlayer player, DiscordUser user, MessageSource source)
        {
            if (CanFilterMessage(source))
            {
                UFilterText(message);
            }
        }

        private bool CanFilterMessage(MessageSource source)
        {
            switch (source)
            {
                case MessageSource.Discord:
                    return _settings.DiscordMessages;
                case MessageSource.Server:
                    return _settings.ServerMessage;
                case MessageSource.Team:
                    return _settings.TeamMessage;
                case MessageSource.Cards:
                    return _settings.CardMessage;
                case MessageSource.Clan:
                    return _settings.ClanMessage;
                case MessageSource.PluginClan:
                case MessageSource.PluginAlliance:
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
                text.Replace(profanity, GetProfanityReplacement(profanity));
            }
        }

        private string GetProfanityReplacement(string profanity)
        {
            if (string.IsNullOrEmpty(profanity))
            {
                return string.Empty;
            }
            
            string replacement = _replacements[profanity.Length];
            if (replacement == null)
            {
                replacement = new string(_settings.ReplacementCharacter, profanity.Length);
                _replacements[profanity.Length] = replacement;
            }

            return replacement;
        }
    }
}