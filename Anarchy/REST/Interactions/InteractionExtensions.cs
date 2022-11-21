using System.Threading.Tasks;

namespace Discord
{
    public static class InteractionExtensions
    {
        public static Task RespondToInteractionAsync(this RestClient<IBotClient> client, ulong interactionId, string interactionToken, InteractionCallbackType callbackType, InteractionResponseProperties properties = null) =>
            client.HttpClient.PostAsync($"/interactions/{interactionId}/{interactionToken}/callback", new InteractionResponse() { Type = callbackType, Data = properties });

        public static void RespondToInteraction(this RestClient<IBotClient> client, ulong interactionId, string interactionToken, InteractionCallbackType callbackType, InteractionResponseProperties properties = null) =>
            client.RespondToInteractionAsync(interactionId, interactionToken, callbackType, properties);

        public static Task ModifyInteractionResponseAsync(this RestClient<IBotClient> client, ulong appId, string interactionToken, InteractionResponseProperties changes) =>
            client.HttpClient.PatchAsync($"/webhooks/{appId}/{interactionToken}/messages/@original", changes);
    }
}
