using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace Discord
{
    public class WelcomeScreen : Controllable
    {
        public WelcomeScreen()
        {
            OnClientUpdated += (s, e) =>
            {
                Channels.SetClientsInList(Client);
            };
        }

        [JsonPropertyName("description")]
        public string Description { get; private set; }

        [JsonPropertyName("welcome_channels")]
        public IReadOnlyList<WelcomeChannel> Channels { get; private set; }
    }
}






