using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Platform;
using Newtonsoft.Json.Linq;

namespace Discord
{
    public static class GuildExtensions
    {
        #region management
        public static async Task<DiscordGuild> CreateGuildAsync(this IRestClient client, string name, DiscordImage icon = null, string region = null)
        {
            return (await client.HttpClient.PostAsync("/guilds", new GuildCreationProperties()
            {
                Name = name,
                Icon = icon,
                Region = region
            })).Deserialize<DiscordGuild>().SetClient(client);
        }

        /// <summary>
        /// Creates a guild
        /// </summary>
        /// <param name="properties">Options for creating the guild</param>
        /// <returns>The created <see cref="DiscordGuild"/></returns>
        public static DiscordGuild CreateGuild(this IRestClient client, string name, DiscordImage icon = null, string region = null)
        {
            return client.CreateGuildAsync(name, icon, region).GetAwaiter().GetResult();
        }

        public static async Task<DiscordGuild> ModifyGuildAsync(this IRestClient client, ulong guildId, GuildProperties properties)
        {
            if (properties.VanityProperty.Set)
                await client.SetGuildVanityUrlAsync(guildId, properties.VanityUrlCode);

            return (await client.HttpClient.PatchAsync($"/guilds/{guildId}", properties)).Deserialize<DiscordGuild>().SetClient(client);
        }

        /// <summary>
        /// Modifies a guild
        /// </summary>
        /// <param name="guildId">ID of the group</param>
        /// <param name="properties">Options for modifying the guild</param>
        /// <returns>The modified <see cref="DiscordGuild"/></returns>
        public static DiscordGuild ModifyGuild(this IRestClient client, ulong guildId, GuildProperties properties)
        {
            return client.ModifyGuildAsync(guildId, properties).GetAwaiter().GetResult();
        }

        public static async Task DeleteGuildAsync(this IRestClient client, ulong guildId)
        {
            await client.HttpClient.DeleteAsync($"/guilds/{guildId}");
        }

        /// <summary>
        /// Deletes a guild
        /// </summary>
        /// <param name="guildId">ID of the guild</param>
        public static void DeleteGuild(this IRestClient client, ulong guildId)
        {
            client.DeleteGuildAsync(guildId).GetAwaiter().GetResult();
        }

        public static async Task SetGuildVanityUrlAsync(this IRestClient client, ulong guildId, string vanityCode)
        {
            await client.HttpClient.PatchAsync($"/guilds/{guildId}/vanity-url", $"{{\"code\":\"{vanityCode}\"}}");
        }

        public static void SetGuildVanityUrl(this IRestClient client, ulong guildId, string vanityCode)
        {
            client.SetGuildVanityUrlAsync(guildId, vanityCode).GetAwaiter().GetResult();
        }

        public static async Task<IReadOnlyList<DiscordBan>> GetGuildBansAsync(this IRestClient client, ulong guildId)
        {
            IReadOnlyList<DiscordBan> bans = (await client.HttpClient.GetAsync($"/guilds/{guildId}/bans"))
                                                    .Deserialize<IReadOnlyList<DiscordBan>>().SetClientsInList(client);
            foreach (var ban in bans)
                ban.GuildId = guildId;
            return bans;
        }

        /// <summary>
        /// Gets the guild's banned users
        /// </summary>
        /// <param name="guildId">ID of the guild</param>
        public static IReadOnlyList<DiscordBan> GetGuildBans(this IRestClient client, ulong guildId)
        {
            return client.GetGuildBansAsync(guildId).GetAwaiter().GetResult();
        }

        public static async Task<DiscordBan> GetGuildBanAsync(this IRestClient client, ulong guildId, ulong userId)
        {
            DiscordBan ban = (await client.HttpClient.GetAsync($"/guilds/{guildId}/bans/{userId}"))
                                   .Deserialize<DiscordBan>().SetClient(client);
            ban.GuildId = guildId;
            return ban;
        }

        /// <summary>
        /// Gets a guild's banned user
        /// </summary>
        /// <param name="guildId">ID of the guild</param>
        /// <param name="userId">ID of the user</param>
        /// <returns></returns>
        public static DiscordBan GetGuildBan(this IRestClient client, ulong guildId, ulong userId)
        {
            return client.GetGuildBanAsync(guildId, userId).GetAwaiter().GetResult();
        }

        public static async Task UnbanGuildMemberAsync(this IRestClient client, ulong guildId, ulong userId)
        {
            await client.HttpClient.DeleteAsync($"/guilds/{guildId}/bans/{userId}");
        }

        /// <summary>
        /// Unbans a user from a guild
        /// </summary>
        /// <param name="guildId">ID of the guild</param>
        /// <param name="userId">ID of the user</param>
        public static void UnbanGuildMember(this IRestClient client, ulong guildId, ulong userId)
        {
            client.UnbanGuildMemberAsync(guildId, userId).GetAwaiter().GetResult();
        }
        #endregion

        public static async Task<ClientGuildSettings> ModifyGuildSettingsAsync(this IRestClient client, ulong guildId, GuildSettingsProperties properties)
        {
            return (await client.HttpClient.PatchAsync($"/users/@me/guilds/{guildId}/settings", properties)).Deserialize<ClientGuildSettings>().SetClient(client);
        }

        public static ClientGuildSettings ModifyGuildSettings(this IRestClient client, ulong guildId, GuildSettingsProperties properties)
        {
            return client.ModifyGuildSettingsAsync(guildId, properties).GetAwaiter().GetResult();
        }

        public static async Task<IReadOnlyList<DiscordChannelSettings>> SetPrivateChannelSettingsAsync(this IRestClient client, Dictionary<ulong, ChannelSettingsProperties> channels)
        {
            JObject container = new JObject
            {
                ["channel_overrides"] = JObject.FromObject(channels)
            };

            return (await client.HttpClient.PatchAsync($"/users/@me/guilds/@me/settings", container)).Deserialize<JObject>()["channel_overrides"].ToObject<List<DiscordChannelSettings>>();
        }

        public static IReadOnlyList<DiscordChannelSettings> SetPrivateChannelSettings(this IRestClient client, Dictionary<ulong, ChannelSettingsProperties> channels)
        {
            return client.SetPrivateChannelSettingsAsync(channels).GetAwaiter().GetResult();
        }

        public static async Task<IReadOnlyList<PartialGuild>> GetGuildsAsync(this IRestClient client, uint limit = 100, ulong afterId = 0)
        {
            return (await client.HttpClient.GetAsync($"/users/@me/guilds?limit={limit}&after={afterId}"))
                            .Deserialize<IReadOnlyList<PartialGuild>>().SetClientsInList(client);
        }

        /// <summary>
        /// Gets the guilds the account is in
        /// </summary>
        /// <param name="limit">Max amount of guild to receive</param>
        /// <param name="afterId">Guild ID to offset from</param>
        public static IReadOnlyList<PartialGuild> GetGuilds(this IRestClient client, uint limit = 100, ulong afterId = 0)
        {
            return client.GetGuildsAsync(limit, afterId).GetAwaiter().GetResult();
        }

        public static async Task<DiscordGuild> GetGuildAsync(this IRestClient client, ulong guildId)
        {
            return (await client.HttpClient.GetAsync("/guilds/" + guildId))
                                .Deserialize<DiscordGuild>().SetClient(client);
        }

        /// <summary>
        /// Gets a guild
        /// </summary>
        /// <param name="guildId">ID of the guild</param>
        public static DiscordGuild GetGuild(this IRestClient client, ulong guildId)
        {
            return client.GetGuildAsync(guildId).GetAwaiter().GetResult();
        }

        public static async Task<GuildInvite> JoinGuildAsync(this RestClient<IUserAccount> client, string invCode, string captchaKey = null)
        {
            return (await client.HttpClient.PostAsync($"/invites/{invCode}", captchaKey != null ? $"{{\"captcha_key\":\"{captchaKey}\"}}" : null))
                                .Deserialize<GuildInvite>().SetClient(client);
        }

        /// <summary>
        /// Joins a guild
        /// </summary>
        /// <returns>The invite used to join the guild</returns>
        public static GuildInvite JoinGuild(this RestClient<IUserAccount> client, string invCode)
        {
            return client.JoinGuildAsync(invCode).GetAwaiter().GetResult();
        }

        public static async Task LeaveGuildAsync(this IRestClient client, ulong guildId, bool lurking = false)
        {
            await client.HttpClient.DeleteAsync($"/users/@me/guilds/{guildId}", $"{{\"lurking\":{lurking.ToString().ToLower()}}}");
        }

        /// <summary>
        /// Leaves a guild
        /// </summary>
        /// <param name="guildId">ID of the guild</param>
        public static void LeaveGuild(this IRestClient client, ulong guildId, bool lurking = false)
        {
            client.LeaveGuildAsync(guildId, lurking).GetAwaiter().GetResult();
        }

        public static async Task AcknowledgeGuildMessagesAsync(this IRestClient client, ulong guildId)
        {
            await client.HttpClient.PostAsync($"/guilds/{guildId}/ack");
        }

        /// <summary>
        /// Acknowledges all messages and pings in a guild
        /// </summary>
        /// <param name="guildId">ID of the guild</param>
        public static void AcknowledgeGuildMessages(this IRestClient client, ulong guildId)
        {
            client.AcknowledgeGuildMessagesAsync(guildId).GetAwaiter().GetResult();
        }

        public static async Task<IImage> GetGoLivePreviewAsync(this IRestClient client, ulong guildId, ulong channelId, ulong userId)
        {
            return PlatformImage.FromStream(
                new MemoryStream(
                    await new HttpClient().GetByteArrayAsync(
                        (await client.HttpClient.GetAsync($"https://discordapp.com/api/v6/streams/guild:{guildId}:{channelId}:{userId}/preview?version=1589053944368"))
                            .Deserialize<JObject>().Value<string>("url")
                    )
                )
            );
        }

        public static IImage GetGoLivePreview(this IRestClient client, ulong guildId, ulong channelId, ulong userId)
        {
            return client.GetGoLivePreviewAsync(guildId, channelId, userId).GetAwaiter().GetResult();
        }
    }
}