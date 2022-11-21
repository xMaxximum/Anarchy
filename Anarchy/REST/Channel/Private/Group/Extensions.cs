using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Discord
{
    public static class GroupExtensions
    {
        public static async Task<DiscordInvite> JoinGroupAsync(this RestClient<IUserClient> client, string inviteCode)
        {
            return (await client.HttpClient.PostAsync($"/invites/{inviteCode}"))
                                    .ParseDeterministic<DiscordInvite>().SetClient(client);
        }

        /// <summary>
        /// Joins a group
        /// </summary>
        /// <param name="inviteCode">Invite for the group</param>
        /// <returns>The invite used</returns>
        public static DiscordInvite JoinGroup(this RestClient<IUserClient> client, string inviteCode)
        {
            return client.JoinGroupAsync(inviteCode).GetAwaiter().GetResult();
        }

        public static async Task<DiscordGroup> CreateGroupAsync(this RestClient<IUserClient> client, List<ulong> recipients)
        {
            return (await client.HttpClient.PostAsync($"/users/@me/channels", new JObject()
            {
                ["recipients"] = JArray.FromObject(recipients)
            })).Deserialize<DiscordGroup>().SetClient(client);
        }

        /// <summary>
        /// Creates a group
        /// </summary>
        /// <param name="recipients">The IDs of the recipients to add</param>
        /// <returns>The created <see cref="DiscordGroup"/></returns>
        public static DiscordGroup CreateGroup(this RestClient<IUserClient> client, List<ulong> recipients)
        {
            return client.CreateGroupAsync(recipients).GetAwaiter().GetResult();
        }

        public static async Task<DiscordGroup> LeaveGroupAsync(this RestClient<IUserClient> client, ulong groupId)
        {
            return (DiscordGroup) await client.DeleteChannelAsync(groupId);
        }

        public static DiscordGroup LeaveGroup(this RestClient<IUserClient> client, ulong groupId)
        {
            return client.LeaveGroupAsync(groupId).GetAwaiter().GetResult();
        }

        public static async Task AddUserToGroupAsync(this RestClient<IUserClient> client, ulong groupId, ulong userId)
        {
            await client.HttpClient.PutAsync($"/channels/{groupId}/recipients/{userId}");
        }

        /// <summary>
        /// Adds a user to a group
        /// </summary>
        /// <param name="groupId">ID of the group</param>
        /// <param name="userId">ID of the user</param>
        public static void AddUserToGroup(this RestClient<IUserClient> client, ulong groupId, ulong userId)
        {
            client.AddUserToGroupAsync(groupId, userId).GetAwaiter().GetResult();
        }

        public static async Task RemoveUserFromGroupAsync(this RestClient<IUserClient> client, ulong groupId, ulong userId)
        {
            await client.HttpClient.DeleteAsync($"/channels/{groupId}/recipients/{userId}");
        }

        /// <summary>
        /// Removes a user from a group
        /// </summary>
        /// <param name="groupId">ID of the group</param>
        /// <param name="userId">ID of the user</param>
        public static void RemoveUserFromGroup(this RestClient<IUserClient> client, ulong groupId, ulong userId)
        {
            client.RemoveUserFromGroupAsync(groupId, userId).GetAwaiter().GetResult();
        }
    }
}
