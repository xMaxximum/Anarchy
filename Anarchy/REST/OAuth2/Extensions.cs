using System.Text.Json.Serialization;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Discord
{
    public static class OAuth2Extensions
    {
        public static async Task<IReadOnlyList<AuthorizedApp>> GetAuthorizedAppsAsync(this RestClient<IUserAccount> client)
        {
            return (await client.HttpClient.GetAsync($"/oauth2/tokens"))
                                .Deserialize<IReadOnlyList<AuthorizedApp>>().SetClientsInList(client);
        }

        public static IReadOnlyList<AuthorizedApp> GetAuthorizedApps(this RestClient<IUserAccount> client)
        {
            return client.GetAuthorizedAppsAsync().GetAwaiter().GetResult();
        }

        public static async Task DeauthorizeAppAsync(this RestClient<IUserAccount> client, ulong appId)
        {
            await client.HttpClient.DeleteAsync("/oauth2/tokens/" + appId);
        }

        public static void DeauthorizeApp(this RestClient<IUserAccount> client, ulong appId)
        {
            client.DeauthorizeAppAsync(appId).GetAwaiter().GetResult();
        }

        public static async Task AuthorizeBotAsync(this RestClient<IUserAccount> client, ulong botId, ulong guildId, DiscordPermission permissions, string captchaKey)
        {
            await client.HttpClient.PostAsync($"/oauth2/authorize?client_id={botId}&scope=bot", JsonSerializer.Serialize(new DiscordBotAuthProperties()
            {
                GuildId = guildId,
                Permissions = permissions,
                CaptchaKey = captchaKey
            }));
        }

        /// <summary>
        /// Adds a bot to a server
        /// </summary>
        /// <param name="botId">client_id from the oauth2 url</param>
        /// <param name="guildId">the guild to add the bot to</param>
        /// <param name="permissions">permissions the bot should have</param>
        /// <param name="captchaKey">captcha key used to validate the request</param>
        public static void AuthorizeBot(this RestClient<IUserAccount> client, ulong botId, ulong guildId, DiscordPermission permissions, string captchaKey)
        {
            client.AuthorizeBotAsync(botId, guildId, permissions, captchaKey).GetAwaiter().GetResult();
        }

        public static async Task<string> AuthorizeAppAsync(this RestClient<IUserAccount> client, ulong appId, string scope)
        {
            return (await client.HttpClient.PostAsync($"/oauth2/authorize?client_id={appId}&response_type=code&scope={scope}")).Deserialize<JsonValue>()["location"].GetValue<string>();
        }

        /// <summary>
        /// Authorizes an app to a client
        /// </summary>
        /// <param name="appId">client_id from the oauth2 url</param>
        /// <param name="scope">scope from the oauth2 url</param>
        /// <returns>A redirect url containing the auth code</returns>
        public static string AuthorizeApp(this RestClient<IUserAccount> client, ulong appId, string scope)
        {
            return client.AuthorizeAppAsync(appId, scope).GetAwaiter().GetResult();
        }

        public static async Task<IReadOnlyList<OAuth2Application>> GetApplicationsAsync(this RestClient<IUserAccount> client)
        {
            return (await client.HttpClient.GetAsync("/applications?with_team_applications=true"))
                                .Deserialize<IReadOnlyList<OAuth2Application>>().SetClientsInList(client);
        }

        /// <summary>
        /// Gets all OAuth2 applications the client owns
        /// </summary>
        public static IReadOnlyList<OAuth2Application> GetApplications(this RestClient<IUserAccount> client)
        {
            return client.GetApplicationsAsync().GetAwaiter().GetResult();
        }

        public static async Task<OAuth2Application> GetApplicationAsync(this RestClient<IUserAccount> client, ulong id)
        {
            return (await client.HttpClient.GetAsync("/applications/" + id)).Deserialize<OAuth2Application>().SetClient(client);
        }

        /// <summary>
        /// Gets an OAuth2 application owned by the client
        /// </summary>
        /// <param name="id">The application's ID</param>
        public static OAuth2Application GetApplication(this RestClient<IUserAccount> client, ulong id)
        {
            return client.GetApplicationAsync(id).GetAwaiter().GetResult();
        }

        public static async Task<OAuth2Application> CreateApplicationAsync(this RestClient<IUserAccount> client, string name)
        {
            return (await client.HttpClient.PostAsync("/oauth2/applications", $"{{\"name\":\"{name}\"}}"))
                                .Deserialize<OAuth2Application>().SetClient(client);
        }

        /// <summary>
        /// Creates an OAuth2 application
        /// </summary>
        /// <param name="name">name for the application</param>
        public static OAuth2Application CreateApplication(this RestClient<IUserAccount> client, string name)
        {
            return client.CreateApplicationAsync(name).GetAwaiter().GetResult();
        }

        public static async Task<OAuth2Application> ModifyApplicationAsync(this RestClient<IUserAccount> client, ulong id, DiscordApplicationProperties properties)
        {
            return (await client.HttpClient.PatchAsync("/applications/" + id, properties)).Deserialize<OAuth2Application>().SetClient(client);
        }

        /// <summary>
        /// Modifies an OAuth2 application
        /// </summary>
        /// <param name="id">The application's ID</param>
        /// <param name="properties">Your changes</param>
        public static OAuth2Application ModifyApplication(this RestClient<IUserAccount> client, ulong id, DiscordApplicationProperties properties)
        {
            return client.ModifyApplicationAsync(id, properties).GetAwaiter().GetResult();
        }

        public static async Task<ApplicationBot> AddBotToApplicationAsync(this RestClient<IUserAccount> client, ulong appId)
        {
            return (await client.HttpClient.PostAsync($"/oauth2/applications/{appId}/bot")).Deserialize<ApplicationBot>().SetClient(client);
        }

        /// <summary>
        /// Adds a bot to the application
        /// </summary>
        /// <param name="appId">ID of the OAuth2 application</param>
        /// <returns></returns>
        public static ApplicationBot AddBotToApplication(this RestClient<IUserAccount> client, ulong appId)
        {
            return client.AddBotToApplicationAsync(appId).GetAwaiter().GetResult();
        }

        public static async Task DeleteApplicationAsync(this RestClient<IUserAccount> client, ulong appId)
        {
            await client.HttpClient.PostAsync($"/applications/{appId}/delete");
        }

        /// <summary>
        /// Deletes an OAuth2 application
        /// </summary>
        /// <param name="appId">ID of the application</param>
        public static void DeleteApplication(this RestClient<IUserAccount> client, ulong appId)
        {
            client.DeleteApplicationAsync(appId).GetAwaiter().GetResult();
        }
    }
}

