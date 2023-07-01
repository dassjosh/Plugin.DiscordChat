using System.Collections.Generic;
using Oxide.Core.Libraries.Covalence;

namespace DiscordChatPlugin.Plugins
{
    public partial class DiscordChat
    {
        public bool SendBetterChatMessage(IPlayer player, string message)
        {
            if (IsPluginLoaded(BetterChat))
            {
                Dictionary<string, object> data = BetterChat.Call<Dictionary<string, object>>("API_GetMessageData", player, message);
                BetterChat.Call("API_SendMessage", data);
                return true;
            }

            return false;
        }

        // public string GetBetterChatConsoleMessage(IPlayer player, string message)
        // {
        //     return BetterChat.Call<string>("API_GetFormattedMessage", player, message, _true);
        // }
    }
}