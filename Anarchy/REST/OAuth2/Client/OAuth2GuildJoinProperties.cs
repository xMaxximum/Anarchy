using System.Text.Json.Serialization;
using System.Collections.Generic;


namespace Discord
{
    public class OAuth2GuildJoinProperties
    {
        [JsonPropertyName("access_token")]
        internal string AccessToken { get; set; }

        private DiscordParameter<string> _nickParam = new DiscordParameter<string>();
        [JsonPropertyName("nick")]
        public string Nickname
        {
            get { return _nickParam; }
            set { _nickParam.Value = value; }
        }

        public bool ShouldSerializeNickname() => _nickParam.Set;

        private DiscordParameter<List<ulong>> _roleParam = new DiscordParameter<List<ulong>>();
        [JsonPropertyName("roles")]
        public List<ulong> Roles
        {
            get { return _roleParam; }
            set { _roleParam.Value = value; }
        }

        public bool ShouldSerializeRoles() => _roleParam.Set;

        private DiscordParameter<bool> _muteParam = new DiscordParameter<bool>();
        [JsonPropertyName("mute")]
        public bool Mute
        {
            get { return _muteParam; }
            set { _muteParam.Value = value; }
        }

        public bool ShouldSerializeMute() => _muteParam.Set;

        private DiscordParameter<bool> _deafParam = new DiscordParameter<bool>();
        [JsonPropertyName("deaf")]
        public bool Deaf
        {
            get { return _deafParam; }
            set { _deafParam.Value = value; }
        }

        public bool ShouldSerializeDeaf() => _deafParam.Set;
    }
}

