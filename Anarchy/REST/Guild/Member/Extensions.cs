using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading.Tasks;


namespace Discord
{
    public static class GuildMemberExtensions
    {
        public static async Task<GuildMember> GetGuildMemberAsync(this IRestClient client, ulong guildId, ulong userId)
        {
            GuildMember member = (await client.HttpClient.GetAsync($"/guilds/{guildId}/members/{userId}"))
                                            .Deserialize<GuildMember>().SetClient(client);
            member.GuildId = guildId;
            return member;
        }

        /// <summary>
        /// Gets a member of a guild
        /// </summary>
        /// <param name="guildId">ID of the guild</param>
        /// <param name="userId">ID of the user</param>
        public static GuildMember GetGuildMember(this IRestClient client, ulong guildId, ulong userId)
        {
            return client.GetGuildMemberAsync(guildId, userId).GetAwaiter().GetResult();
        }

        public static async Task ModifyGuildMemberAsync(this IRestClient client, ulong guildId, ulong userId, GuildMemberProperties properties)
        {
            await client.HttpClient.PatchAsync($"/guilds/{guildId}/members/{userId}", properties);
        }

        public static void ModifyGuildMember(this IRestClient client, ulong guildId, ulong userId, GuildMemberProperties properties)
        {
            client.ModifyGuildMemberAsync(guildId, userId, properties).GetAwaiter().GetResult();
        }

        public static async Task SetClientNicknameAsync(this IRestClient client, ulong guildId, string nickname)
        {
            await client.HttpClient.PatchAsync($"/guilds/{guildId}/members/@me/nick", $"{{\"nick\":\"{nickname}\"}}");
        }

        public static void SetClientNickname(this IRestClient client, ulong guildId, string nickname)
        {
            client.SetClientNicknameAsync(guildId, nickname).GetAwaiter().GetResult();
        }

        public static async Task KickGuildMemberAsync(this IRestClient client, ulong guildId, ulong userId)
        {
            await client.HttpClient.DeleteAsync($"/guilds/{guildId}/members/{userId}");
        }

        /// <summary>
        /// Kicks a member from a guild
        /// </summary>
        /// <param name="guildId">ID of the guild</param>
        /// <param name="userId">ID of the member</param>
        public static void KickGuildMember(this IRestClient client, ulong guildId, ulong userId)
        {
            client.KickGuildMemberAsync(guildId, userId).GetAwaiter().GetResult();
        }

        public static async Task<uint> GetGuildPrunableMembersAsync(this IRestClient client, ulong guildId, MemberPruneProperties properties)
        {
            string url = $"/guilds/{guildId}/prune?days={properties.Days}";

            foreach (var role in properties.IncludedRoles)
                url += "&include_roles=" + role;

            return (await client.HttpClient.GetAsync(url)).Deserialize<JsonValue>()["pruned"].GetValue<uint>();
        }

        public static uint GetGuildPrunableMembers(this IRestClient client, ulong guildId, MemberPruneProperties properties)
        {
            return client.GetGuildPrunableMembersAsync(guildId, properties).GetAwaiter().GetResult();
        }

        public static async Task<uint> PruneGuildMembersAsync(this IRestClient client, ulong guildId, MemberPruneProperties properties)
        {
            return (await client.HttpClient.PostAsync($"/guilds/{guildId}/prune", properties))
                                    .Deserialize<JsonValue>()["pruned"].GetValue<uint>();
        }

        public static uint PruneGuildMembers(this IRestClient client, ulong guildId, MemberPruneProperties properties)
        {
            return client.PruneGuildMembersAsync(guildId, properties).GetAwaiter().GetResult();
        }

        public static async Task BanGuildMemberAsync(this IRestClient client, ulong guildId, ulong userId, string reason = null, uint deleteMessageDays = 0)
        {
            await client.HttpClient.PutAsync($"/guilds/{guildId}/bans/{userId}?delete-message-days={deleteMessageDays}&reason={reason}");
        }

        /// <summary>
        /// Bans a member from a guild
        /// </summary>
        /// <param name="guildId">ID of the guild</param>
        /// <param name="userId">ID of the member</param>
        /// <param name="reason">Reason for banning the member</param>
        /// <param name="deleteMessageDays">Amount of days to purge messages for</param>
        public static void BanGuildMember(this IRestClient client, ulong guildId, ulong userId, string reason = null, uint deleteMessageDays = 0)
        {
            client.BanGuildMemberAsync(guildId, userId, reason, deleteMessageDays).GetAwaiter().GetResult();
        }
    }
}

