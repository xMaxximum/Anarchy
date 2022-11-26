namespace Discord
{
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using System.Threading.Tasks;

    public static class UserExtensions
    {
        // !!!!!!!!!!!!!!!!!!!!!!!!
        // TODO This definately doesn't belong here. It's here only to get GeClientUserAsync working.
        private static readonly JsonSerializerOptions Options = new()
        {
            PropertyNameCaseInsensitive = true,
            NumberHandling = JsonNumberHandling.AllowReadingFromString
        };

        public static async Task<DiscordUser> GetUserAsync(this IRestClient client, ulong userId)
        {
            return (await client.HttpClient.GetAsync($"/users/{userId}")).Deserialize<DiscordUser>().SetClient(client);
        }

        /// <summary>
        /// Gets a user
        /// </summary>
        /// <param name="userId">ID of the user</param>
        public static DiscordUser GetUser(this IRestClient client, ulong userId)
        {
            return client.GetUserAsync(userId).GetAwaiter().GetResult();
        }

        public static async Task<DiscordClientUser> GetClientUserAsync(this IRestClient client)
        {
            try
            {
                return client.User = (await client.HttpClient.GetAsync("/users/@me"))
                    .Body.Deserialize<DiscordClientUser>(Options).SetClient(client);
            }
            catch (DiscordHttpException)
            {
                client.User = null;
                throw;
            }
        }

        /// <summary>
        /// Gets the account's user
        /// </summary>
        public static DiscordClientUser GetClientUser(this IRestClient client)
        {
            return client.GetClientUserAsync().GetAwaiter().GetResult();
        }

        public static Task ReportUserAsync(this RestClient<IUserAccount> client, DiscordReportReason reason, UserReportIdentification identification)
        {
            identification.Reason = reason;
            return client.HttpClient.PostAsync("/report", identification);
        }

        public static void ReportUser(this RestClient<IUserAccount> client, DiscordReportReason reason, UserReportIdentification identification) => client.ReportUserAsync(reason, identification);
    }
}
