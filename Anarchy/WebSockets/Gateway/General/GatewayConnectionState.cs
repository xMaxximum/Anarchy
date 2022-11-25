using System.Text.Json.Serialization;
namespace Discord.Gateway
{
    public enum GatewayConnectionState
    {
        NotConnected,
        Connecting,
        Connected
    }
}

