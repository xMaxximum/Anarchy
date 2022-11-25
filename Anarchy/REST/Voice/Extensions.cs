using System.Text.Json.Serialization;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Discord
{
    public static class VoiceExtensions
    {
        public static async Task RingAsync(this RestClient<IUserAccount> client, ulong channelId, List<ulong> recipients)
        {
            await client.HttpClient.PostAsync($"/channels/{channelId}/call/ring", new JsonObject
            {
                ["recipients"] = recipients == null ? null : JsonSerializer.Serialize(recipients)
            });
        }

        /// <summary>
        /// Rings the specified recipients
        /// </summary>
        public static void Ring(this RestClient<IUserAccount> client, ulong channelId, List<ulong> recipients)
        {
            client.RingAsync(channelId, recipients).GetAwaiter().GetResult();
        }

        public static async Task StartCallAsync(this RestClient<IUserAccount> client, ulong channelId)
        {
            await client.RingAsync(channelId, null);
        }

        /// <summary>
        /// Opens a call on the specified channel
        /// </summary>
        public static void StartCall(this RestClient<IUserAccount> client, ulong channelId)
        {
            client.StartCallAsync(channelId).GetAwaiter().GetResult();
        }

        public static async Task StopRingingAsync(this RestClient<IUserAccount> client, ulong channelId, List<ulong> recipients)
        {
            await client.HttpClient.PostAsync($"/channels/{channelId}/call/stop-ringing", new JsonObject
            {
                ["recipients"] = recipients == null ? null : JsonSerializer.Serialize(recipients)
            });
        }

        /// <summary>
        /// Stops ringing the specified recipients
        /// </summary>
        public static void StopRinging(this RestClient<IUserAccount> client, ulong channelId, List<ulong> recipients)
        {
            client.StopRingingAsync(channelId, recipients).GetAwaiter().GetResult();
        }

        public static async Task DeclineCallAsync(this RestClient<IUserAccount> client, ulong channelId)
        {
            await client.StopRingingAsync(channelId, null);
        }

        /// <summary>
        /// Declines the current incoming call from the specified channel
        /// </summary>
        public static void DeclineCall(this RestClient<IUserAccount> client, ulong channelId)
        {
            client.DeclineCallAsync(channelId).GetAwaiter().GetResult();
        }

        public static async Task<IReadOnlyList<VoiceRegion>> GetVoiceRegionsAsync(this RestClient<IUserAccount> client)
        {
            return (await client.HttpClient.GetAsync("/voice/regions"))
                                .Deserialize<IReadOnlyList<VoiceRegion>>();
        }

        /// <summary>
        /// Gets all available voice regions
        /// </summary>
        public static IReadOnlyList<VoiceRegion> GetVoiceRegions(this RestClient<IUserAccount> client)
        {
            return client.GetVoiceRegionsAsync().GetAwaiter().GetResult();
        }
    }
}

