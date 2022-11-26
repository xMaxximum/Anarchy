using System.Text.Json;
using Discord.Gateway;

namespace Discord
{
    [TestClass]
    public static class Globals
    {
        internal static Settings.App Settings { get; private set; } = GetAppSettings();

        public static class FileNames
        {
            public const string Setting = @".\appsettings.json";
            public const string SettingDevelopment = @".\appsettings.Development.json";

            public const string Image1 = @".\Resources\image1.png";
            public const string Image2 = @".\Resources\image2.jpg";
            public const string PoetryTxt = @".\Resources\poetry.txt";
        }

        public static DiscordSocketClient? Client { get; set; }
        public static RestClient<IUserAccount>? RestClient { get; set; }

        [AssemblyInitialize()]
        public static void AssemblyInit(TestContext context)
        {
            var autoResetEvent = new AutoResetEvent(false);

            // All tests will use the proxy specified in appsettings.json, which can be added like so:
            //  ,
            //  "Proxy": {
            //    "Host": "127.0.0.1",
            //    "Port": 8888
            //  }

            var restClient = new RestClient<IUserAccount>(Settings.Token);

            var client = new DiscordSocketClient(RestClient, new DiscordSocketConfig()
            {
                Proxy = Settings.Proxy?.CreateProxy()
            });

            client.OnLoggedIn += OnLoggedIn;
            client.Login();

            void OnLoggedIn(DiscordSocketClient client, LoginEventArgs args)
            {
                Console.WriteLine($"Logged into {args.User}");
                autoResetEvent.Set();
            }

            autoResetEvent.WaitOne();

            Client = client;
            RestClient = restClient;
        }

        private static Settings.App GetAppSettings()
        {
            var path = File.Exists(FileNames.SettingDevelopment)
                ? FileNames.SettingDevelopment
                : FileNames.Setting;

            return JsonSerializer.Deserialize<Settings.App>(File.ReadAllText(path))!;
        }
    }
}