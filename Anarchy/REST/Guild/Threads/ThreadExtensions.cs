using System.Text.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;


namespace Discord
{
    public static class ThreadExtensions
    {
        private static async Task<DiscordThread> createThreadAsync(this IRestClient client, ulong channelId, string name, TimeSpan ttl, ulong? msgId)
        {
            string msgStr = msgId.HasValue ? "/messages/" + msgId : "";

            return (await client.HttpClient.PostAsync($"/channels/{channelId}{msgStr}/threads", new ThreadCreationProperties()
            {
                TTL = ttl,
                Location = msgId.HasValue ? "Message" : "Thread Browser Toolbar",
                Name = name,
                Type = ChannelType.GuildPublicThread
            })).Deserialize<DiscordThread>().SetClient(client);
        }

        public static Task<DiscordThread> CreateThreadAsync(this IRestClient client, ulong channelId, ulong messageId, string name, TimeSpan ttl)
                => client.createThreadAsync(channelId, name, ttl, messageId);

        public static DiscordThread CreateThread(this IRestClient client, ulong channelId, ulong messageId, string name, TimeSpan ttl)
                => client.CreateThreadAsync(channelId, messageId, name, ttl).GetAwaiter().GetResult();

        public static Task<DiscordThread> CreateThreadAsync(this IRestClient client, ulong channelId, string name, TimeSpan ttl)
                => client.createThreadAsync(channelId, name, ttl, null);

        public static DiscordThread CreateThread(this IRestClient client, ulong channelId, string name, TimeSpan ttl)
                => client.CreateThreadAsync(channelId, name, ttl).GetAwaiter().GetResult();

        public static async Task<DiscordThread> ModifyThreadAsync(this IRestClient client, ulong threadId, ThreadProperties changes)
                => (await client.HttpClient.PatchAsync("/channels/" + threadId, changes)).Deserialize<DiscordThread>().SetClient(client);

        public static DiscordThread ModifyThread(this IRestClient client, ulong threadId, ThreadProperties changes)
                => client.ModifyThreadAsync(threadId, changes).GetAwaiter().GetResult();

        public static Task JoinThreadAsync(this IRestClient client, ulong threadId)
                => client.HttpClient.PostAsync($"/channels/{threadId}/thread-members/@me?location=Context Menu");

        public static void JoinThread(this IRestClient client, ulong threadId)
                => client.JoinThreadAsync(threadId).GetAwaiter().GetResult();

        public static Task LeaveThreadAsync(this IRestClient client, ulong threadId)
                => client.HttpClient.DeleteAsync($"/channels/{threadId}/thread-members/@me?location=Context Menu");

        public static void LeaveThread(this IRestClient client, ulong threadId)
                => client.LeaveThreadAsync(threadId).GetAwaiter().GetResult();

        public static async Task<IReadOnlyList<DiscordThreadMember>> GetThreadMembersAsync(this IRestClient client, ulong threadId)
                => (await client.HttpClient.GetAsync($"/channels/{threadId}/thread-members")).Deserialize<List<DiscordThreadMember>>().SetClientsInList(client);

        public static IReadOnlyList<DiscordThreadMember> GetThreadMembers(this IRestClient client, ulong threadId)
                => client.GetThreadMembersAsync(threadId).GetAwaiter().GetResult();

        public static async Task<IReadOnlyList<DiscordThread>> GetChannelActiveThreadsAsync(this IRestClient client, ulong channelId)
                => (await client.HttpClient.GetAsync($"/channels/{channelId}/threads/active")).Body.GetProperty("threads").Deserialize<List<DiscordThread>>();

        public static IReadOnlyList<DiscordThread> GetChannelActiveThreads(this IRestClient client, ulong channelId)
                => client.GetChannelActiveThreadsAsync(channelId).GetAwaiter().GetResult();
    }
}

