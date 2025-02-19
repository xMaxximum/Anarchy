using System.Text.Json.Serialization;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discord
{
    public static class DMChannelExtensions
    {
        public static async Task<IReadOnlyList<PrivateChannel>> GetPrivateChannelsAsync(this IRestClient client)
        {
            return (await client.HttpClient.GetAsync($"/users/@me/channels"))
                                .Deserialize<List<PrivateChannel>>().SetClientsInList(client);
        }

        /// <summary>
        /// Gets the account's private channels
        /// </summary>
        public static IReadOnlyList<PrivateChannel> GetPrivateChannels(this IRestClient client)
        {
            return client.GetPrivateChannelsAsync().GetAwaiter().GetResult();
        }

        public static async Task<PrivateChannel> CreateDMAsync(this IRestClient client, ulong recipientId)
        {
            return (await client.HttpClient.PostAsync($"/users/@me/channels", $"{{\"recipient_id\":\"{recipientId}\"}}"))
                    .Deserialize<PrivateChannel>().SetClient(client);
        }

        /// <summary>
        /// Creates a direct messaging channel
        /// </summary>
        /// <param name="recipientId">ID of the user</param>
        /// <returns>The created <see cref="PrivateChannel"/></returns>
        public static PrivateChannel CreateDM(this IRestClient client, ulong recipientId)
        {
            return client.CreateDMAsync(recipientId).GetAwaiter().GetResult();
        }

        public static async Task ChangePrivateCallRegionAsync(this IRestClient client, ulong channelId, string regionId)
        {
            await client.HttpClient.PatchAsync($"/channels/{channelId}/call", $"{{\"region\":\"{regionId}\"}}");
        }

        /// <summary>
        /// Changes the call region (fx. hongkong) for the private channel
        /// </summary>
        /// <param name="channelId">ID of the private channel</param>
        /// <param name="regionId">The region ID (fx. hongkong)</param>
        public static void ChangePrivateCallRegion(this IRestClient client, ulong channelId, string regionId)
        {
            client.ChangePrivateCallRegionAsync(channelId, regionId).GetAwaiter().GetResult();
        }
    }
}

