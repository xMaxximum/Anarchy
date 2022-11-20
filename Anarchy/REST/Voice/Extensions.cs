﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Discord
{
    public static class VoiceExtensions
    {
        public static async Task RingAsync(this IRestClient client, ulong channelId, List<ulong> recipients)
        {
            await client.HttpClient.PostAsync($"/channels/{channelId}/call/ring", new JObject
            {
                ["recipients"] = recipients == null ? null : JArray.FromObject(recipients)
            });
        }

        /// <summary>
        /// Rings the specified recipients
        /// </summary>
        public static void Ring(this IRestClient client, ulong channelId, List<ulong> recipients)
        {
            client.RingAsync(channelId, recipients).GetAwaiter().GetResult();
        }

        public static async Task StartCallAsync(this IRestClient client, ulong channelId)
        {
            await client.RingAsync(channelId, null);
        }

        /// <summary>
        /// Opens a call on the specified channel
        /// </summary>
        public static void StartCall(this IRestClient client, ulong channelId)
        {
            client.StartCallAsync(channelId).GetAwaiter().GetResult();
        }

        public static async Task StopRingingAsync(this IRestClient client, ulong channelId, List<ulong> recipients)
        {
            await client.HttpClient.PostAsync($"/channels/{channelId}/call/stop-ringing", new JObject
            {
                ["recipients"] = recipients == null ? null : JArray.FromObject(recipients)
            });
        }

        /// <summary>
        /// Stops ringing the specified recipients
        /// </summary>
        public static void StopRinging(this IRestClient client, ulong channelId, List<ulong> recipients)
        {
            client.StopRingingAsync(channelId, recipients).GetAwaiter().GetResult();
        }

        public static async Task DeclineCallAsync(this IRestClient client, ulong channelId)
        {
            await client.StopRingingAsync(channelId, null);
        }

        /// <summary>
        /// Declines the current incoming call from the specified channel
        /// </summary>
        public static void DeclineCall(this IRestClient client, ulong channelId)
        {
            client.DeclineCallAsync(channelId).GetAwaiter().GetResult();
        }

        public static async Task<IReadOnlyList<VoiceRegion>> GetVoiceRegionsAsync(this IRestClient client)
        {
            return (await client.HttpClient.GetAsync("/voice/regions"))
                                .Deserialize<IReadOnlyList<VoiceRegion>>();
        }

        /// <summary>
        /// Gets all available voice regions
        /// </summary>
        public static IReadOnlyList<VoiceRegion> GetVoiceRegions(this IRestClient client)
        {
            return client.GetVoiceRegionsAsync().GetAwaiter().GetResult();
        }
    }
}
