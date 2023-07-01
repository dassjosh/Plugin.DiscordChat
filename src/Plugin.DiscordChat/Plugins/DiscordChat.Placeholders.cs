using System.Net;
using System.Text;
using DiscordChatPlugin.Localization;
using DiscordChatPlugin.Placeholders;
using Oxide.Core.Libraries.Covalence;
using Oxide.Ext.Discord.Libraries.Placeholders;

namespace DiscordChatPlugin.Plugins
{
    public partial class DiscordChat
    {
        public void RegisterPlaceholders()
        {
            _placeholders.RegisterPlaceholder<string>(this, PlaceholderKeys.Message, PlaceholderKeys.Data.Message);
            _placeholders.RegisterPlaceholder<string>(this, PlaceholderKeys.PlayerMessage, PlaceholderKeys.Data.PlayerMessage);
            _placeholders.RegisterPlaceholder<string>(this, PlaceholderKeys.DisconnectReason, PlaceholderKeys.Data.DisconnectReason);
            _placeholders.RegisterPlaceholder<IPlayer, string>(this, PlaceholderKeys.PlayerName, GetPlayerName);
            _placeholders.RegisterPlaceholder(this, PlaceholderKeys.DiscordTag, _pluginConfig.ChatSettings.DiscordTag);
            _placeholders.RegisterPlaceholder<IPlayer, string>(this, PlaceholderKeys.CountryLower, GetCountryFlag);
        }
        
        public string GetPlayerName(IPlayer player)
        {
            string name = ProcessPlaceholders(LangKeys.Discord.Chat.PlayerName, GetDefault().AddPlayer(player));
            StringBuilder sb = _pool.GetStringBuilder(name);
            for (int index = 0; index < _plugins.Count; index++)
            {
                _plugins[index].ProcessPlayerName(sb, player);
            }

            return _pool.FreeStringBuilderToString(sb);
        }

        public string GetCountryFlag(IPlayer player)
        {
            string country = _placeholders.ProcessPlaceholders("{player.address.data!country.code}", GetDefault().AddPlayer(player));
            string flag;
            if (_flagCache.TryGetValue(country, out flag))
            {
                return country;
            }
            
            IPAddress address;
            if (string.IsNullOrEmpty(country) || IPAddress.TryParse(country, out address))
            {
                flag = ":signal_strength:";
            }
            else
            {
                flag =  $":flag_{country.ToLower()}:";
            }

            _flagCache[country] = flag;
            return flag;
        }

        public PlaceholderData GetDefault()
        {
            return _placeholders.CreateData(this);
        }
    }
}