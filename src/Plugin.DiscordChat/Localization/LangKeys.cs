namespace DiscordChatPlugin.Localization;

public static class LangKeys
{
    public const string Root = "V3.";

    public static class Discord
    {
        private const string Base = Root + nameof(Discord) + ".";

        public static class Chat
        {
            private const string Base = Discord.Base + nameof(Chat) + ".";

            public const string Server = Base + nameof(Server);
            public const string LinkedMessage = Base + nameof(LinkedMessage);
            public const string UnlinkedMessage = Base + nameof(UnlinkedMessage);
            public const string PlayerName = Base + nameof(PlayerName);
        }

        public static class Team
        {
            private const string Base = Discord.Base + nameof(Team) + ".";

            public const string Message = Base + nameof(Message);
        }

        public static class Cards
        {
            private const string Base = Discord.Base + nameof(Cards) + ".";

            public const string Message = Base + nameof(Message);
        }
            
        public static class Clans
        {
            private const string Base = Discord.Base + nameof(Clans) + ".";

            public const string Message = Base + nameof(Message);
        }

        public static class Player
        {
            private const string Base = Discord.Base + nameof(Player) + ".";
                
            public const string Connecting = Base + nameof(Connecting);
            public const string Connected = Base + nameof(Connected);
            public const string Disconnected = Base + nameof(Disconnected);
        }

        public static class AdminChat
        {
            private const string Base = Discord.Base + nameof(AdminChat) + ".";

            public const string ServerMessage = Base + nameof(ServerMessage);
            public const string DiscordMessage = Base + nameof(DiscordMessage);
        }
                
        public static class PluginClans
        {
            private const string Base = Discord.Base + nameof(PluginClans) + ".";

            public const string ClanMessage = Base + nameof(ClanMessage);
            public const string AllianceMessage = Base + nameof(AllianceMessage);
        }
    }

    public static class Server
    {
        private const string Base = Root + nameof(Server) + ".";

        public const string LinkedMessage = Base + nameof(LinkedMessage);
        public const string UnlinkedMessage = Base + nameof(UnlinkedMessage);
    }
}