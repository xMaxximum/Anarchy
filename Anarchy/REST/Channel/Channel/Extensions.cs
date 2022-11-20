using System.Threading.Tasks;

namespace Discord
{
    public static class ChannelExtensions
    {
        public static async Task<DiscordChannel> GetChannelAsync(this IRestClient client, ulong channelId)
        {
            return (await client.HttpClient.GetAsync($"/channels/{channelId}"))
                                    .ParseDeterministic<DiscordChannel>().SetClient(client);
        }

        /// <summary>
        /// Gets a channel
        /// </summary>
        /// <param name="channelId">ID of the channel</param>
        public static DiscordChannel GetChannel(this IRestClient client, ulong channelId)
        {
            return client.GetChannelAsync(channelId).GetAwaiter().GetResult();
        }

        public static async Task<DiscordGroup> ModifyGroupAsync(this IRestClient client, ulong groupId, GroupProperties properties)
        {
            return (await client.HttpClient.PatchAsync($"/channels/{groupId}", properties))
                                      .ParseDeterministic<DiscordGroup>().SetClient(client);
        }

        public static DiscordGroup ModifyGroup(this IRestClient client, ulong groupId, GroupProperties properties)
        {
            return client.ModifyGroupAsync(groupId, properties).GetAwaiter().GetResult();
        }

        public static async Task<GuildChannel> ModifyGuildChannelAsync(this IRestClient client, ulong channelId, GuildChannelProperties properties)
        {
            return (await client.HttpClient.PatchAsync($"/channels/{channelId}", properties))
                                    .ParseDeterministic<GuildChannel>().SetClient(client);
        }

        public static GuildChannel ModifyGuildChannel(this IRestClient client, ulong channelId, GuildChannelProperties properties)
        {
            return client.ModifyGuildChannelAsync(channelId, properties).GetAwaiter().GetResult();
        }

        public static async Task<DiscordChannel> DeleteChannelAsync(this IRestClient client, ulong channelId)
        {
            return (await client.HttpClient.DeleteAsync($"/channels/{channelId}"))
                                        .ParseDeterministic<DiscordChannel>().SetClient(client);
        }

        /// <summary>
        /// Deletes a channel
        /// </summary>
        /// <param name="channelId">ID of the channel</param>
        /// <returns>The deleted channel</returns>
        public static DiscordChannel DeleteChannel(this IRestClient client, ulong channelId)
        {
            return client.DeleteChannelAsync(channelId).GetAwaiter().GetResult();
        }
    }
}
