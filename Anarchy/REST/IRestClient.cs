namespace Discord
{
    public interface IRestClient
    {
        DiscordClientUser User { get; set; }
        LockedDiscordConfig Config { get; }
        DiscordHttpClient HttpClient { get; }

        string Token { get; set; }
    }
}
