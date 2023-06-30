using DiscordChatPlugin.Configuration.Plugins;
using DiscordChatPlugin.Plugins;
using Oxide.Core.Plugins;

namespace DiscordChatPlugin.PluginHandlers
{
    public class ClansHandler : BasePluginHandler
    {
        private readonly ClansSettings _settings;

        public ClansHandler(DiscordChat chat, ClansSettings settings, Plugin plugin) : base(chat, plugin)
        {
            _settings = settings;
        }

        // public override void ProcessPlayerName(StringBuilder name, IPlayer player)
        // {
        //     if (player == null)
        //     {
        //         return;
        //     }
        //     
        //     string clanTag = Plugin.Call<string>("GetClanOf", player.Id);
        //     if (!string.IsNullOrEmpty(clanTag))
        //     {
        //         name.Insert(0, DiscordChat.Instance.Lang(LangKeys.Server.ClanTag, player, clanTag));
        //     }
        // }
    }
}