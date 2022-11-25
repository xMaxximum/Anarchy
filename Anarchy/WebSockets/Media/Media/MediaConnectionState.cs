using System.Text.Json.Serialization;
namespace Discord.Media
{
    public enum MediaConnectionState
    {
        NotConnected,
        Connecting,
        Connected,
        Ready
    }
}

