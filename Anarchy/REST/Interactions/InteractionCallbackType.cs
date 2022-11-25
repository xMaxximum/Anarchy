using System.Text.Json.Serialization;
namespace Discord
{
    public enum InteractionCallbackType
    {
        Pong = 1,
        RespondWithMessage = 4,
        DelayedMessage,
        DelayedMessageUpdate,
        UpdateMessage
    }
}

