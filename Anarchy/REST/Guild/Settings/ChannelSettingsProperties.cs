using System.Text.Json.Serialization;


namespace Discord
{
    public class ChannelSettingsProperties
    {
        // cba to implement this rn lol
        [JsonPropertyName("mute_config")]
#pragma warning disable IDE0052
        private readonly GuildMuteConfig _muteConfig = new GuildMuteConfig()
        {
            EndTime = null,
            SelectedTimeWindow = -1
        };
#pragma warning restore

        public bool ShouldSerialize_muteConfig()
        {
            return Muted;
        }

        private readonly DiscordParameter<bool> _mutedProperty = new DiscordParameter<bool>();
        [JsonPropertyName("muted")]
        public bool Muted
        {
            get { return _mutedProperty; }
            set { _mutedProperty.Value = value; }
        }

        public bool ShouldSerializeMuted()
        {
            return _mutedProperty.Set;
        }

        private readonly DiscordParameter<ClientNotificationLevel> _notifsProperty = new DiscordParameter<ClientNotificationLevel>();
        [JsonPropertyName("message_notifications")]
        public ClientNotificationLevel Notifications
        {
            get { return _notifsProperty; }
            set { _notifsProperty.Value = value; }
        }

        public bool ShouldSerializeNotifications()
        {
            return _notifsProperty.Set;
        }
    }
}

