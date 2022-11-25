using System.Text.Json.Serialization;
using System.Collections.Generic;


namespace Discord.Media
{
    internal class GoLiveUpdate
    {
        [JsonPropertyName("stream_key")]
        public string StreamKey { get; private set; }

        [JsonPropertyName("region")]
        public string Region { get; private set; }

        [JsonPropertyName("paused")]
        public bool Paused { get; private set; }

        [JsonPropertyName("viewer_ids")]
        public IReadOnlyList<ulong> ViewerIds { get; private set; }
    }
}

