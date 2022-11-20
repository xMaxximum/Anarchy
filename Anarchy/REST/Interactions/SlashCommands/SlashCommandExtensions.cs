using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discord
{
    public static class SlashCommandExtensions
    {
        public static async Task<IReadOnlyList<ApplicationCommand>> GetGlobalCommandsAsync(this IRestClient client, ulong appId) =>
            (await client.HttpClient.GetAsync($"/applications/{appId}/commands")).Deserialize<List<ApplicationCommand>>().SetClientsInList(client);

        public static IReadOnlyList<ApplicationCommand> GetGlobalCommands(this IRestClient client, ulong appId) => client.GetGlobalCommandsAsync(appId).GetAwaiter().GetResult();

        public static async Task<ApplicationCommand> GetGlobalCommandAsync(this IRestClient client, ulong appId, ulong commandId) =>
            (await client.HttpClient.GetAsync($"/applications/{appId}/commands/{commandId}")).Deserialize<ApplicationCommand>().SetClient(client);

        public static ApplicationCommand GetApplicationCommand(this IRestClient client, ulong appId, ulong commandId) => client.GetGlobalCommandAsync(appId, commandId).GetAwaiter().GetResult();

        public static async Task<ApplicationCommand> CreateGlobalCommandAsync(this IRestClient client, ulong appId, ApplicationCommandProperties properties) =>
            (await client.HttpClient.PostAsync($"/applications/{appId}/commands", properties)).Deserialize<ApplicationCommand>().SetClient(client);

        public static ApplicationCommand CreateGlobalCommand(this IRestClient client, ulong appId, ApplicationCommandProperties properties) => client.CreateGlobalCommandAsync(appId, properties).GetAwaiter().GetResult();

        public static async Task<IReadOnlyList<ApplicationCommand>> SetGlobalApplicationCommandsAsync(this IRestClient client, ulong appId, List<ApplicationCommandProperties> commands) =>
            (await client.HttpClient.PutAsync($"/applications/{appId}/commands", commands)).Deserialize<List<ApplicationCommand>>().SetClientsInList(client);

        public static IReadOnlyList<ApplicationCommand> SetGlobalApplicationCommands(this IRestClient client, ulong appId, List<ApplicationCommandProperties> commands) => client.SetGlobalApplicationCommandsAsync(appId, commands).GetAwaiter().GetResult();

        public static async Task<ApplicationCommand> ModifyGlobalCommandAsync(this IRestClient client, ulong appId, ulong commandId, ApplicationCommandProperties properties) =>
            (await client.HttpClient.PatchAsync($"/applications/{appId}/commands/{commandId}", properties)).Deserialize<ApplicationCommand>().SetClient(client);

        public static ApplicationCommand ModifyGlobalCommand(this IRestClient client, ulong appId, ulong commandId, ApplicationCommandProperties properties) => client.ModifyGlobalCommandAsync(appId, commandId, properties).GetAwaiter().GetResult();

        public static Task DeleteGlobalCommandAsync(this IRestClient client, ulong appId, ulong commandId) => client.HttpClient.DeleteAsync($"/applications/{appId}/commands/{commandId}");
        public static void DeleteGlobalCommand(this IRestClient client, ulong appId, ulong commandId) => client.DeleteGlobalCommandAsync(appId, commandId).GetAwaiter().GetResult();
    }
}
