using System.Text.Json.Serialization;
namespace Discord
{
    public interface IRestClient
    {
        DiscordClientUser User { get; set; }
        LockedDiscordConfig Config { get; }
        DiscordHttpClient HttpClient { get; }

        string Token { get; set; }
    }

    /// <summary>
    /// Discord client that only supports HTTP
    /// </summary>
    public class RestClient<TClient> : IRestClient
    {
        public DiscordClientUser User { get; set; }
        public LockedDiscordConfig Config { get; set; }
        public DiscordHttpClient HttpClient { get; private set; }

        private string _token;
        public string Token
        {
            get
            {
                return _token;
            }
            set
            {
                string previousToken = Token;

                _token = value;

                try
                {
                    this.GetClientUser();
                }
                catch (DiscordHttpException ex)
                {
                    _token = previousToken;

                    if (ex.Code == DiscordError.MessageOnlyError && ex.ErrorMessage == "401: Unauthorized")
                        throw new InvalidTokenException(value);
                    else
                        throw;
                }
            }
        }

        internal RestClient()
        {
            HttpClient = new DiscordHttpClient(this);
        }

        public RestClient(string token, ApiConfig config = null) : this()
        {
            config ??= new ApiConfig();
            Config = new LockedDiscordConfig(config);

            Token = token;
            User = this.GetClientUser();
        }

        public override string ToString()
        {
            return User.ToString();
        }
    }

    public interface IBotAccount
    {
    }

    public interface IUserAccount
    {
    }
}
