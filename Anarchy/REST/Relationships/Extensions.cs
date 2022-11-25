using System.Text.Json.Serialization;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discord
{
    public static class RelationshipsExtensions
    {
        public static async Task<IReadOnlyList<DiscordRelationship>> GetRelationshipsAsync(this RestClient<IUserAccount> client)
        {
            return (await client.HttpClient.GetAsync($"/users/@me/relationships"))
                                .Deserialize<IReadOnlyList<DiscordRelationship>>().SetClientsInList(client);
        }

        /// <summary>
        /// Gets the account's relationships (friends, blocked etc.)
        /// </summary>
        public static IReadOnlyList<DiscordRelationship> GetRelationships(this RestClient<IUserAccount> client)
        {
            return client.GetRelationshipsAsync().GetAwaiter().GetResult();
        }

        public static async Task SendFriendRequestAsync(this RestClient<IUserAccount> client, ulong userId)
        {
            await client.HttpClient.PutAsync("/users/@me/relationships/" + userId);
        }

        public static void SendFriendRequest(this RestClient<IUserAccount> client, ulong userId)
        {
            client.SendFriendRequestAsync(userId).GetAwaiter().GetResult();
        }

        public static async Task SendFriendRequestAsync(this RestClient<IUserAccount> client, string username, uint discriminator)
        {
            await client.HttpClient.PostAsync("/users/@me/relationships", $"{{\"username\":\"{username}\",\"discriminator\":{discriminator}}}");
        }

        /// <summary>
        /// Sends a friend request to a user
        /// </summary>
        public static void SendFriendRequest(this RestClient<IUserAccount> client, string username, uint discriminator)
        {
            client.SendFriendRequestAsync(username, discriminator).GetAwaiter().GetResult();
        }

        public static async Task BlockUserAsync(this RestClient<IUserAccount> client, ulong userId)
        {
            await client.HttpClient.PutAsync($"/users/@me/relationships/{userId}", new DiscordRelationship() { Type = RelationshipType.Blocked });
        }

        /// <summary>
        /// Blocks a user
        /// </summary>
        /// <param name="userId">ID of the user</param>
        public static void BlockUser(this RestClient<IUserAccount> client, ulong userId)
        {
            client.BlockUserAsync(userId).GetAwaiter().GetResult();
        }

        public static async Task RemoveRelationshipAsync(this RestClient<IUserAccount> client, ulong userId)
        {
            await client.HttpClient.DeleteAsync($"/users/@me/relationships/{userId}");
        }

        /// <summary>
        /// Removes any relationship (unfriending, unblocking etc.)
        /// </summary>
        /// <param name="userId">ID of the user</param>
        public static void RemoveRelationship(this RestClient<IUserAccount> client, ulong userId)
        {
            client.RemoveRelationshipAsync(userId).GetAwaiter().GetResult();
        }

        public static async Task<DiscordProfile> GetProfileAsync(this RestClient<IUserAccount> client, ulong userId)
        {
            return (await client.HttpClient.GetAsync($"/users/{userId}/profile?with_mutual_guilds=true"))
                                .Deserialize<DiscordProfile>().SetClient(client);
        }

        /// <summary>
        /// Gets a user's profile
        /// </summary>
        /// <param name="userId">ID of the user</param>
        public static DiscordProfile GetProfile(this RestClient<IUserAccount> client, ulong userId)
        {
            return client.GetProfileAsync(userId).GetAwaiter().GetResult();
        }

        public static async Task<IReadOnlyList<DiscordUser>> GetMutualFriendsAsync(this RestClient<IUserAccount> client, ulong userId)
        {
            return (await client.HttpClient.GetAsync($"/users/{userId}/relationships"))
                                .Deserialize<IReadOnlyList<DiscordUser>>().SetClientsInList(client);
        }

        /// <summary>
        /// Gets the mutual friends between our user and the other user
        /// </summary>
        /// <param name="userId">ID of the user</param>
        /// <returns>List of mutual friends.</returns>
        public static IReadOnlyList<DiscordUser> GetMutualFriends(this RestClient<IUserAccount> client, ulong userId)
        {
            return client.GetMutualFriendsAsync(userId).GetAwaiter().GetResult();
        }
    }
}
