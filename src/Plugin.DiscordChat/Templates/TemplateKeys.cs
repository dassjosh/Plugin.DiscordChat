namespace DiscordChatPlugin.Templates
{
    public static class TemplateKeys
    {
        public static class Player
        {
            private const string Base = nameof(Player) + ".";

            public const string Connecting = Base + nameof(Connecting);
            public const string Connected = Base + nameof(Connected);
            public const string Disconnected = Base + nameof(Disconnected);
        }

        public static class Server
        {
            private const string Base = nameof(Server) + ".";
            
            public const string Online = Base + nameof(Online);
            public const string Shutdown = Base + nameof(Shutdown);
            public const string Booting = Base + nameof(Booting);
        }

        public static class Chat
        {
            private const string Base = nameof(Chat) + ".";
            
            public const string General = Base + nameof(General);
            public const string Teams = Base + nameof(Teams);
            public const string Cards = Base + nameof(Cards);
            public const string Clan = Base + nameof(Clan);

            public static class Clans
            {
                private const string Base = Chat.Base + nameof(Clans) + ".";
                
                public const string Clan = Base + nameof(Clan);
                public const string Alliance = Base + nameof(Alliance);
            }
            
            public static class AdminChat
            {
                private const string Base = Chat.Base + nameof(AdminChat) + ".";

                public const string Message = Base + nameof(Message);
            }
        }

        public static class Error
        {
            private const string Base = nameof(Error) + ".";
            
            public const string NotLinked = Base + nameof(NotLinked);

            public static class AdminChat
            {
                private const string Base = Error.Base + nameof(AdminChat) + ".";

                public const string NotLinked = Base + nameof(NotLinked);
                public const string NoPermission = Base + nameof(NoPermission);
            }
        }
    }
}