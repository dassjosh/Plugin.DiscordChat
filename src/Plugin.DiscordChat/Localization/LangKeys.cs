namespace DiscordChatPlugin.Localization
{
    public static class LangKeys
    {
        public const string Root = "V2.";

            public static class Discord
            {
                private const string Base = Root + nameof(Discord) + ".";

                //public const string NotLinkedError = Base + nameof(NotLinkedError);

                public static class Chat
                {
                    private const string Base = Discord.Base + nameof(Chat) + ".";

                    public const string Server = Base + nameof(Server);
                    public const string LinkedMessage = Base + nameof(LinkedMessage);
                    public const string UnlinkedMessage = Base + nameof(UnlinkedMessage);
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

                public static class Player
                {
                    private const string Base = Discord.Base + nameof(Player) + ".";
                
                    public const string Connected = Base + nameof(Connected);
                    public const string Disconnected = Base + nameof(Disconnected);
                }

                // public static class OnlineOffline
                // {
                //     private const string Base = Discord.Base + nameof(OnlineOffline) + ".";
                //
                //     public const string OnlineMessage = Base + nameof(OnlineMessage);
                //     public const string OfflineMessage = Base + nameof(OfflineMessage);
                //     public const string BootingMessage = Base + nameof(BootingMessage);
                // }

                public static class AdminChat
                {
                    private const string Base = Discord.Base + nameof(AdminChat) + ".";

                    public const string ServerMessage = Base + nameof(ServerMessage);
                    public const string DiscordMessage = Base + nameof(DiscordMessage);
                    //public const string Permission = Base + nameof(Permission);
                }
                
                public static class Clans
                {
                    private const string Base = Discord.Base + nameof(Clans) + ".";

                    public const string ClanMessage = Base + nameof(ClanMessage);
                    public const string AllianceMessage = Base + nameof(AllianceMessage);
                }
            }

            public static class Server
            {
                private const string Base = Root + nameof(Server) + ".";

                public const string LinkedMessage = Base + nameof(LinkedMessage);
                public const string UnlinkedMessage = Base + nameof(UnlinkedMessage);
                public const string DiscordTag = Base + nameof(DiscordTag);
                public const string ClanTag = Base + nameof(ClanTag);
            }
    }
}