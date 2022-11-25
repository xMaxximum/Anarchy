using System.Text.Json.Serialization;
using System.Collections.Generic;


namespace Discord.Media
{
    internal class DiscordMediaReady
    {
        [JsonPropertyName("ssrc")]
        public uint SSRC { get; private set; }

        [JsonPropertyName("ip")]
        public string IP { get; private set; }

        [JsonPropertyName("port")]
        public int Port { get; private set; }

        [JsonPropertyName("modes")]
        public List<string> EncryptionModes { get; private set; }

        [JsonPropertyName("streams")]
        public List<StreamSSRC> Streams { get; private set; }
    }
}

