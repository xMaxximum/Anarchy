using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace Discord
{
    public class GuildVerificationForm
    {
        [JsonPropertyName("description")]
        public string Description { get; private set; }

        [JsonPropertyName("version")]
        public string Version { get; private set; }

        [JsonPropertyName("form_fields")]
        public IReadOnlyList<GuildVerificationFormField> Fields { get; private set; }
    }
}


