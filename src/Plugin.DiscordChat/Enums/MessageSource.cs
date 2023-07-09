namespace DiscordChatPlugin.Enums
{
    public enum MessageSource : byte
    {
        Connecting,
        Connected,
        Disconnected,
        ServerBooting,
        ServerOnline,
        ServerShutdown,
        Server,
        Discord,
        Team,
        Cards,
        Clan,
        Alliance,
        AdminChat
    }
}