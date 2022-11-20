using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discord
{
    public static class WebhookExtensions
    {
        public static async Task<DiscordDefaultWebhook> CreateWebhookAsync(this IRestClient client, ulong channelId, DiscordWebhookProperties properties)
        {
            properties.ChannelId = channelId;
            DiscordDefaultWebhook hook = (await client.HttpClient.PostAsync($"/channels/{channelId}/webhooks", properties)).Deserialize<DiscordDefaultWebhook>().SetClient(client);
            hook.Modify(properties);
            return hook;
        }

        /// <summary>
        /// Creates a webhook
        /// </summary>
        /// <param name="channelId">ID of the channel</param>
        /// <param name="properties">Options for creating/modifying the webhook</param>
        /// <returns>The created webhook</returns>
        public static DiscordDefaultWebhook CreateWebhook(this IRestClient client, ulong channelId, DiscordWebhookProperties properties)
        {
            return client.CreateWebhookAsync(channelId, properties).Result;
        }

        public static async Task<DiscordWebhook> ModifyWebhookAsync(this IRestClient client, ulong webhookId, DiscordWebhookProperties properties)
        {
            return (await client.HttpClient.PatchAsync($"/webhooks/{webhookId}", properties)).ParseDeterministic<DiscordWebhook>().SetClient(client);
        }

        public static DiscordWebhook ModifyWebhook(this IRestClient client, ulong webhookId, DiscordWebhookProperties properties)
        {
            return client.ModifyWebhookAsync(webhookId, properties).Result;
        }

        public static async Task SendWebhookMessageAsync(this IRestClient client, ulong webhookId, string webhookToken, string content, DiscordEmbed embed = null, DiscordWebhookProfile profile = null)
        {
            var properties = new WebhookMessageProperties() { Content = content, Embed = embed };

            if (profile != null)
            {
                if (profile.NameProperty.Set)
                    properties.Username = profile.Username;
                if (profile.AvatarProperty.Set)
                    properties.AvatarUrl = profile.AvatarUrl;
            }

            await client.HttpClient.PostAsync($"/webhooks/{webhookId}/{webhookToken}", properties);
        }

        /// <summary>
        /// Sends a message through the webhook
        /// </summary>
        /// <param name="webhookId">ID of the webhook</param>
        /// <param name="webhookToken">The webhook's token</param>
        /// <param name="content">The message to send</param>
        /// <param name="embed">Embed to include in the message</param>
        /// <param name="profile">Custom Username and Avatar url (both are optional)</param>
        public static void SendWebhookMessage(this IRestClient client, ulong webhookId, string webhookToken, string content, DiscordEmbed embed = null, DiscordWebhookProfile profile = null)
        {
            client.SendWebhookMessageAsync(webhookId, webhookToken, content, embed, profile).GetAwaiter().GetResult();
        }

        public static async Task DeleteWebhookAsync(this IRestClient client, ulong webhookId, string token = null)
        {
            await client.HttpClient.DeleteAsync($"/webhooks/{webhookId}/{token}");
        }

        /// <summary>
        /// Deletes a webhook
        /// </summary>
        /// <param name="webhookId">ID of the webhook</param>
        public static void DeleteWebhook(this IRestClient client, ulong webhookId, string token = null)
        {
            client.DeleteWebhookAsync(webhookId, token).GetAwaiter().GetResult();
        }

        public static async Task<DiscordWebhook> GetWebhookAsync(this IRestClient client, ulong webhookId, string token = null)
        {
            string url = "/webhooks/" + webhookId;

            if (token != null)
                url += "/" + token;

            return (await client.HttpClient.GetAsync(url)).ParseDeterministic<DiscordWebhook>().SetClient(client);
        }

        /// <summary>
        /// Gets a webhook (if <paramref name="token"/> is set the client does not have to be authenticated)
        /// </summary>
        /// <param name="webhookId">ID of the webhook</param>
        /// <param name="token">The webhooks's token</param>
        public static DiscordWebhook GetWebhook(this IRestClient client, ulong webhookId, string token = null)
        {
            return client.GetWebhookAsync(webhookId, token).GetAwaiter().GetResult();
        }

        public static async Task<IReadOnlyList<DiscordWebhook>> GetGuildWebhooksAsync(this IRestClient client, ulong guildId)
        {
            return (await client.HttpClient.GetAsync($"/guilds/{guildId}/webhooks"))
                                    .MultipleDeterministic<DiscordWebhook>().SetClientsInList(client);
        }

        /// <summary>
        /// Gets a guild's webhooks
        /// </summary>
        /// <param name="guildId">ID of the guild</param>
        public static IReadOnlyList<DiscordWebhook> GetGuildWebhooks(this IRestClient client, ulong guildId)
        {
            return client.GetGuildWebhooksAsync(guildId).Result;
        }

        public static async Task<IReadOnlyList<DiscordWebhook>> GetChannelWebhooksAsync(this IRestClient client, ulong channelId)
        {
            return (await client.HttpClient.GetAsync($"/channels/{channelId}/webhooks"))
                                    .MultipleDeterministic<DiscordWebhook>().SetClientsInList(client);
        }

        /// <summary>
        /// Gets a channel's webhooks
        /// </summary>
        /// <param name="channelId">ID of the channel</param>
        public static IReadOnlyList<DiscordWebhook> GetChannelWebhooks(this IRestClient client, ulong channelId)
        {
            return client.GetChannelWebhooksAsync(channelId).Result;
        }
    }
}
