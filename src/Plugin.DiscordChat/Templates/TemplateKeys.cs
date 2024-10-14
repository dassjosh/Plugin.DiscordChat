using Oxide.Ext.Discord.Libraries;

namespace DiscordChatPlugin.Templates;

public static class TemplateKeys
{
    public static class Player
    {
        private const string Base = nameof(Player) + ".";

        public static readonly TemplateKey Connecting = new(Base + nameof(Connecting));
        public static readonly TemplateKey Connected = new(Base + nameof(Connected));
        public static readonly TemplateKey Disconnected = new(Base + nameof(Disconnected));
    }

    public static class Server
    {
        private const string Base = nameof(Server) + ".";
            
        public static readonly TemplateKey Online = new(Base + nameof(Online));
        public static readonly TemplateKey Shutdown = new(Base + nameof(Shutdown));
        public static readonly TemplateKey Booting = new(Base + nameof(Booting));
    }

    public static class Chat
    {
        private const string Base = nameof(Chat) + ".";
            
        public static readonly TemplateKey General = new(Base + nameof(General));
        public static readonly TemplateKey Teams = new(Base + nameof(Teams));
        public static readonly TemplateKey Cards = new(Base + nameof(Cards));
        public static readonly TemplateKey Clan = new(Base + nameof(Clan));

        public static class Clans
        {
            private const string Base = Chat.Base + nameof(Clans) + ".";
                
            public static readonly TemplateKey Clan = new(Base + nameof(Clan));
            public static readonly TemplateKey Alliance = new(Base + nameof(Alliance));
        }
            
        public static class AdminChat
        {
            private const string Base = Chat.Base + nameof(AdminChat) + ".";

            public static readonly TemplateKey Message = new(Base + nameof(Message));
        }
    }

    public static class Error
    {
        private const string Base = nameof(Error) + ".";
            
        public static readonly TemplateKey NotLinked = new(Base + nameof(NotLinked));

        public static class AdminChat
        {
            private const string Base = Error.Base + nameof(AdminChat) + ".";

            public static readonly TemplateKey NotLinked = new(Base + nameof(NotLinked));
            public static readonly TemplateKey NoPermission = new(Base + nameof(NoPermission));
        }

        public static class BetterChatMute
        {
            private const string Base = Error.Base + nameof(BetterChatMute) + "."; 
            
            public static readonly TemplateKey Muted = new(Base + nameof(Muted));
        }
    }
}