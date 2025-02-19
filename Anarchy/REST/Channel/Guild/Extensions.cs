using System.Text.Json.Serialization;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using System.Text.Json.Nodes;

namespace Discord
{
    public static class GuildChannelExtensions
    {
        public static async Task<IReadOnlyList<GuildChannel>> GetGuildChannelsAsync(this IRestClient client, ulong guildId)
        {
            var channels = (await client.HttpClient.GetAsync($"/guilds/{guildId}/channels")).MultipleDeterministic<GuildChannel>().SetClientsInList(client);

            foreach (var channel in channels)
                channel.GuildId = guildId;

            return channels;
        }

        /// <summary>
        /// Gets a guild's channels
        /// </summary>
        /// <param name="guildId">ID of the guild</param>
        public static IReadOnlyList<GuildChannel> GetGuildChannels(this IRestClient client, ulong guildId)
        {
            return client.GetGuildChannelsAsync(guildId).GetAwaiter().GetResult();
        }

        public static async Task<GuildChannel> CreateGuildChannelAsync(this IRestClient client, ulong guildId, string name, ChannelType type, ulong? parentId = null)
        {
            var channel = (await client.HttpClient.PostAsync($"/guilds/{guildId}/channels", new GuildChannelCreationProperties() { Name = name, Type = type, ParentId = parentId }))
                                            .ParseDeterministic<GuildChannel>().SetClient(client);

            channel.GuildId = guildId;

            return channel;
        }

        /// <summary>
        /// Creates a guild channel
        /// </summary>
        /// <param name="guildId">ID of the guild</param>
        /// <returns>The created <see cref="GuildChannel"/></returns>
        public static GuildChannel CreateGuildChannel(this IRestClient client, ulong guildId, string name, ChannelType type, ulong? parentId = null)
        {
            return client.CreateGuildChannelAsync(guildId, name, type, parentId).GetAwaiter().GetResult();
        }

        public static async Task<DiscordPermissionOverwrite> AddPermissionOverwriteAsync(this IRestClient client, ulong channelId, ulong affectedId, PermissionOverwriteType type, DiscordPermission allow, DiscordPermission deny)
        {
            var overwrite = new DiscordPermissionOverwrite() { AffectedId = affectedId, Type = type, Allow = allow, Deny = deny };

            await client.HttpClient.PutAsync($"/channels/{channelId}/permissions/{overwrite.AffectedId}", overwrite);

            return overwrite;
        }

        /// <summary>
        /// Adds/edits a permission overwrite to a channel
        /// </summary>
        /// <param name="channelId">ID of the channel</param>
        /// <param name="overwrite">The permission overwrite to add/edit</param>
        public static DiscordPermissionOverwrite AddPermissionOverwrite(this IRestClient client, ulong channelId, ulong affectedId, PermissionOverwriteType type, DiscordPermission allow, DiscordPermission deny)
        {
            return client.AddPermissionOverwriteAsync(channelId, affectedId, type, allow, deny).GetAwaiter().GetResult();
        }

        public static async Task RemovePermissionOverwriteAsync(this IRestClient client, ulong channelId, ulong affectedId)
        {
            await client.HttpClient.DeleteAsync($"/channels/{channelId}/permissions/{affectedId}");
        }

        /// <summary>
        /// Removes a permission overwrite from a channel
        /// </summary>
        /// <param name="channelId">ID of the channel</param>
        /// <param name="id">ID of the role or member affected by the overwrite</param>
        public static void RemovePermissionOverwrite(this IRestClient client, ulong channelId, ulong affectedId)
        {
            client.RemovePermissionOverwriteAsync(channelId, affectedId).GetAwaiter().GetResult();
        }

        public static async Task<ulong> FollowChannelAsync(this IRestClient client, ulong channelToFollowId, ulong crosspostChannelId)
        {
            return (await client.HttpClient.PostAsync($"/channels/{channelToFollowId}/followers", $"{{\"webhook_channel_id\":{crosspostChannelId}}}")).Deserialize<JsonValue>()["webhook_od"].GetValue<ulong>();
        }

        public static ulong FollowChannel(this IRestClient client, ulong channelToFollowId, ulong crosspostChannelId)
        {
            return client.FollowChannelAsync(channelToFollowId, crosspostChannelId).GetAwaiter().GetResult();
        }

        public static async Task<DiscordStageInstance> CreateStageInstanceAsync(this IRestClient client, ulong channelId, string topic, StagePrivacyLevel privacyLevel = StagePrivacyLevel.GuildOnly) =>
            (await client.HttpClient.PostAsync("/stage-instances", new { channel_id = channelId, topic, privacy_level = privacyLevel })).Deserialize<DiscordStageInstance>().SetClient(client);

        public static DiscordStageInstance CreateStageInstance(this IRestClient client, ulong channelId, string topic, StagePrivacyLevel privacyLevel = StagePrivacyLevel.GuildOnly) =>
            client.CreateStageInstanceAsync(channelId, topic, privacyLevel).GetAwaiter().GetResult();

        public static Task DeleteStageInstanceAsync(this IRestClient client, ulong channelId) => client.HttpClient.DeleteAsync("/stage-instances/" + channelId);
        public static void DeleteStageInstance(this IRestClient client, ulong channelId) => client.DeleteStageInstanceAsync(channelId).GetAwaiter().GetResult();

        private static Task setStageSpeakingAsync(this IRestClient client, ulong guildId, ulong channelId, string user, bool speaker) =>
            client.HttpClient.PatchAsync($"/guilds/{guildId}/voice-states/" + user, new { channel_id = channelId, suppress = !speaker });

        public static Task SetClientStageSpeakingAsync(this IRestClient client, ulong guildId, ulong channelId, bool speaker) => client.setStageSpeakingAsync(guildId, channelId, "@me", speaker);
        public static void SetClientStageSpeaking(this IRestClient client, ulong guildId, ulong channelId, bool speaker) => client.SetClientStageSpeakingAsync(guildId, channelId, speaker).GetAwaiter().GetResult();

        public static Task SetStageSpeakingAsync(this IRestClient client, ulong guildId, ulong channelId, ulong userId, bool speaker) => client.setStageSpeakingAsync(guildId, channelId, userId.ToString(), speaker);
        public static void SetStageSpeaking(this IRestClient client, ulong guildId, ulong channelId, ulong userId, bool speaker) => client.SetStageSpeakingAsync(guildId, channelId, userId, speaker).GetAwaiter().GetResult();

        public static async Task<IReadOnlyList<StageDiscoveryItem>> GetDiscoverableStagesAsync(this IRestClient client) =>
            (await client.HttpClient.GetAsync("/stage-instances")).Deserialize<List<StageDiscoveryItem>>().SetClientsInList(client);

        public static IReadOnlyList<StageDiscoveryItem> GetDiscoverableStages(this IRestClient client) => client.GetDiscoverableStagesAsync().GetAwaiter().GetResult();
    }
}

