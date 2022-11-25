using System.Text.Json.Serialization;
using Discord.WebSockets;

namespace Discord.Gateway
{
    public class LogoutEventArgs : DiscordWebSocketCloseEventArgs
    {
        public LogoutEventArgs(GatewayCloseCode error, string reason) : base((int) error, reason)
        { }
    }
}

