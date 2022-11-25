using System.Text.Json.Serialization;
namespace Discord.Media
{
    public enum DiscordMediaOpcode
    {
        Identify,
        SelectProtocol,
        Ready,
        Heartbeat,
        SessionDescription,
        Speaking,
        HeartbeatAck,
        Resume,
        Hello,
        Resumed,
        SSRCUpdate = 12,
        UserDisconnect,
        ChangeCodecs
    }
}

