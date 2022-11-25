using System.Text.Json.Serialization;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;


namespace Discord
{
    public static class GroupExtensions
    {
        public static async Task<DiscordInvite> JoinGroupAsync(this RestClient<IUserAccount> client, string inviteCode)
        {
            return (await client.HttpClient.PostAsync($"/invites/{inviteCode}"))
                                    .ParseDeterministic<DiscordInvite>().SetClient(client);
        }

        /// <summary>
        /// Joins a group
        /// </summary>
        /// <param name="inviteCode">Invite for the group</param>
        /// <returns>The invite used</returns>
        public static DiscordInvite JoinGroup(this RestClient<IUserAccount> client, string inviteCode)
        {
            return client.JoinGroupAsync(inviteCode).GetAwaiter().GetResult();
        }

        public static async Task<DiscordGroup> CreateGroupAsync(this RestClient<IUserAccount> client, List<ulong> recipients)
        {
            //return (await client.HttpClient.PostAsync($"/users/@me/channels", new JObject()
            //{
            //    ["recipients"] = JArray.FromObject(recipients)
            //})).Deserialize<DiscordGroup>().SetClient(client);
            return (await client.HttpClient.PostAsync($"/users/@me/channels", JsonSerializer.Serialize(recipients))).Deserialize<DiscordGroup>().SetClient(client);
        }

        /// <summary>
        /// Creates a group
        /// </summary>
        /// <param name="recipients">The IDs of the recipients to add</param>
        /// <returns>The created <see cref="DiscordGroup"/></returns>
        public static DiscordGroup CreateGroup(this RestClient<IUserAccount> client, List<ulong> recipients)
        {
            return client.CreateGroupAsync(recipients).GetAwaiter().GetResult();
        }

        public static async Task<DiscordGroup> LeaveGroupAsync(this RestClient<IUserAccount> client, ulong groupId)
        {
            return (DiscordGroup) await client.DeleteChannelAsync(groupId);
        }

        public static DiscordGroup LeaveGroup(this RestClient<IUserAccount> client, ulong groupId)
        {
            return client.LeaveGroupAsync(groupId).GetAwaiter().GetResult();
        }

        public static async Task AddUserToGroupAsync(this RestClient<IUserAccount> client, ulong groupId, ulong userId)
        {
            await client.HttpClient.PutAsync($"/channels/{groupId}/recipients/{userId}");
        }

        /// <summary>
        /// Adds a user to a group
        /// </summary>
        /// <param name="groupId">ID of the group</param>
        /// <param name="userId">ID of the user</param>
        public static void AddUserToGroup(this RestClient<IUserAccount> client, ulong groupId, ulong userId)
        {
            client.AddUserToGroupAsync(groupId, userId).GetAwaiter().GetResult();
        }

        public static async Task RemoveUserFromGroupAsync(this RestClient<IUserAccount> client, ulong groupId, ulong userId)
        {
            await client.HttpClient.DeleteAsync($"/channels/{groupId}/recipients/{userId}");
        }

        /// <summary>
        /// Removes a user from a group
        /// </summary>
        /// <param name="groupId">ID of the group</param>
        /// <param name="userId">ID of the user</param>
        public static void RemoveUserFromGroup(this RestClient<IUserAccount> client, ulong groupId, ulong userId)
        {
            client.RemoveUserFromGroupAsync(groupId, userId).GetAwaiter().GetResult();
        }
    }
}

