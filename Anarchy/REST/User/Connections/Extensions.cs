using System.Text.Json.Serialization;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discord
{
    public static class ConnectedAccountsExtensions
    {
        public static async Task<IReadOnlyList<ClientConnectedAccount>> GetConnectedAccountsAsync(this RestClient<IUserAccount> client)
        {
            return (await client.HttpClient.GetAsync($"/users/@me/connections"))
                                .Deserialize<IReadOnlyList<ClientConnectedAccount>>().SetClientsInList(client);
        }

        public static IReadOnlyList<ClientConnectedAccount> GetConnectedAccounts(this RestClient<IUserAccount> client)
        {
            return client.GetConnectedAccountsAsync().GetAwaiter().GetResult();
        }

        public static async Task<ClientConnectedAccount> ModifyConnectedAccountAsync(this RestClient<IUserAccount> client, ConnectedAccountType type, string connectionId, ConnectionProperties properties)
        {
            return (await client.HttpClient.PatchAsync($"/users/@me/connections/{type.ToString().ToLower()}/{connectionId}", properties))
                                                .Deserialize<ClientConnectedAccount>();
        }

        public static ClientConnectedAccount ModifyConnectedAccount(this RestClient<IUserAccount> client, ConnectedAccountType type, string connectionId, ConnectionProperties properties)
        {
            return client.ModifyConnectedAccountAsync(type, connectionId, properties).GetAwaiter().GetResult();
        }

        public static async Task RemoveConnectedAccountAsync(this RestClient<IUserAccount> client, ConnectedAccountType type, string id)
        {
            await client.HttpClient.DeleteAsync($"/users/@me/connections/{type.ToString().ToLower()}/{id}");
        }

        public static void RemoveConnectedAccount(this RestClient<IUserAccount> client, ConnectedAccountType type, string id)
        {
            client.RemoveConnectedAccountAsync(type, id).GetAwaiter().GetResult();
        }
    }
}

