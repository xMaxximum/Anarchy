using System.Text.Json.Serialization;
namespace Discord.Media
{
    public enum DiscordLivestreamError
    {
        Unknown,
        Unauthorized,
        StreamNotFound,
        StreamEnded,
        UserRequested
    }
}

