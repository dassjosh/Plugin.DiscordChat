using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using DiscordChatPlugin.Configuration.Plugins;
using DiscordChatPlugin.Plugins;
using Oxide.Core.Libraries.Covalence;
using Oxide.Core.Plugins;

namespace DiscordChatPlugin.PluginHandlers;

public class BetterChatHandler : BasePluginHandler
{
    private readonly BetterChatSettings _settings;

    private readonly Regex _rustRegex = new(@"<b>|<\/b>|<i>|<\/i>|<\/size>|<\/color>|<color=.+?>|<size=.+?>", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    
    public BetterChatHandler(DiscordChat chat, BetterChatSettings settings, Plugin plugin) : base(chat, plugin)
    {
        _settings = settings;
    }

    public override void ProcessPlayerName(StringBuilder name, IPlayer player)
    {
        Dictionary<string, object> data = Chat.GetBetterChatMessageData(player, string.Empty);
        List<string> titles = Chat.GetBetterChatTags(data);

        int addedTitles = 0;
        for (int i = titles.Count - 1; i >= 0; i--)
        {
            if (addedTitles >= _settings.DiscordMaxTags)
            {
                return;
            }
            
            string title = titles[i];
            title = Formatter.ToPlaintext(title);
#if RUST
            title = _rustRegex.Replace(title, string.Empty);
#endif
            name.Insert(0, ' ');
            name.Insert(0, title);
            addedTitles++;
        }
    }
}