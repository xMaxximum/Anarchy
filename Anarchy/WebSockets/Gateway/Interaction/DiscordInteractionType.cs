using System.Text.Json.Serialization;
namespace Discord.Gateway
{
    public enum DiscordInteractionType
    {
        Ping = 1,
        ApplicationCommand,
        MessageComponent
    }
}
