﻿using System;
using DiscordChatPlugin.Configuration.Plugins;
using DiscordChatPlugin.Enums;
using DiscordChatPlugin.Plugins;
using Oxide.Core.Libraries.Covalence;
using Oxide.Core.Plugins;
using Oxide.Ext.Discord.Entities;

namespace DiscordChatPlugin.PluginHandlers;

public class TranslationApiHandler : BasePluginHandler
{
    private readonly ChatTranslatorSettings _settings;

    public TranslationApiHandler(DiscordChat chat, ChatTranslatorSettings settings, Plugin plugin) : base(chat, plugin)
    {
        _settings = settings;
    }

    public override bool HasCallbackMessage() => true;

    public override void ProcessCallbackMessage(string message, IPlayer player, DiscordUser user, MessageSource source, Action<string> callback)
    {
        if (CanChatTranslatorSource(source))
        {
            Plugin.Call("Translate", message, _settings.DiscordServerLanguage, "auto", callback);
            return;
        }

        callback.Invoke(message);
    }

    public bool CanChatTranslatorSource(MessageSource source)
    {
        if (!_settings.Enabled)
        {
            return false;
        }
            
        switch (source)
        {
            case MessageSource.Server:
                return _settings.ServerMessage;

            case MessageSource.Discord:
                return _settings.DiscordMessage;
                
            case MessageSource.PluginClan:
            case MessageSource.PluginAlliance:
                return _settings.PluginMessage;

#if RUST
            case MessageSource.Team:
                return _settings.TeamMessage;

            case MessageSource.Cards:
                return _settings.CardMessages;
                
            case MessageSource.Clan:
                return _settings.ClanMessages;
#endif
        }

        return false;
    }
}