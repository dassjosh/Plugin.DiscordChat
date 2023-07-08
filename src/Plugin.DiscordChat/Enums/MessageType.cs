namespace DiscordChatPlugin.Enums
{
    public enum MessageType : byte
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