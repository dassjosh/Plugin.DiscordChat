using System.Text;
using DiscordChatPlugin.Localization;
using DiscordChatPlugin.Placeholders;
using Oxide.Core.Libraries.Covalence;
using Oxide.Ext.Discord.Libraries;

namespace DiscordChatPlugin.Plugins;

public partial class DiscordChat
{
    public void RegisterPlaceholders()
    {
        _placeholders.RegisterPlaceholder<string>(this, PlaceholderKeys.TemplateMessage, PlaceholderDataKeys.TemplateMessage);
        _placeholders.RegisterPlaceholder<string>(this, PlaceholderKeys.PlayerMessage, PlaceholderDataKeys.PlayerMessage);
        _placeholders.RegisterPlaceholder<string>(this, PlaceholderKeys.DisconnectReason, PlaceholderDataKeys.DisconnectReason);
        _placeholders.RegisterPlaceholder<IPlayer, string>(this, PlaceholderKeys.PlayerName, GetPlayerName);
        _placeholders.RegisterPlaceholder(this, PlaceholderKeys.DiscordTag, _pluginConfig.ChatSettings.DiscordTag);
    }
        
    public string GetPlayerName(IPlayer player)
    {
        string name = Lang(LangKeys.Discord.Chat.PlayerName, GetDefault().AddPlayer(player));
        StringBuilder sb = Pool.GetStringBuilder(name);
        for (int index = 0; index < _plugins.Count; index++)
        {
            _plugins[index].ProcessPlayerName(sb, player);
        }

        return Pool.ToStringAndFree(sb);
    }

    public PlaceholderData GetDefault()
    {
        return _placeholders.CreateData(this);
    }
}