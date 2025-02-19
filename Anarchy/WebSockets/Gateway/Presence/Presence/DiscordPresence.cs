using System.Text.Json.Serialization;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Discord.Gateway
{
    public class DiscordPresence : Controllable
    {
        public DiscordPresence()
        {
            OnClientUpdated += (sender, e) =>
            {
                Activities.SetClientsInList(Client);
            };
        }

        [JsonPropertyName("user")]
        private readonly JsonValue _user;

        public ulong UserId
        {
            get { return _user["id"].GetValue<ulong>(); }
        }

        private readonly DiscordParameter<List<DiscordActivity>> _activitiesParam = new();
        [JsonPropertyName("activities")]
        //[JsonConverter(typeof(DeepJsonConverter<DiscordActivity>))]
        private List<DiscordActivity> _activities
        {
            get { return _activitiesParam; }
            set { _activitiesParam.Value = value; }
        }

        public IReadOnlyList<DiscordActivity> Activities
        {
            get { return _activities; }
        }

        public bool ActivitiesSet
        {
            get { return _activitiesParam.Set; }
        }

        private readonly DiscordParameter<UserStatus> _statusParam = new();
        [JsonPropertyName("status")]
        public UserStatus Status
        {
            get { return _statusParam; }
            private set { _statusParam.Value = value; }
        }

        public bool StatusSet
        {
            get { return _statusParam.Set; }
        }

        private readonly DiscordParameter<ActiveSessionPlatforms> _platformsParam = new();
        [JsonPropertyName("client_status")]
        public ActiveSessionPlatforms ActivePlatforms
        {
            get { return _platformsParam; }
            set { _platformsParam.Value = value; }
        }

        public bool ActivePlatformsSet
        {
            get { return _platformsParam.Set; }
        }

        internal void Update(DiscordPresence presence)
        {
            if (presence.ActivePlatformsSet)
                ActivePlatforms = presence.ActivePlatforms;
            if (presence.ActivitiesSet)
                _activities = presence.Activities.ToList();
            if (presence.StatusSet)
                Status = presence.Status;
        }

        public override string ToString()
        {
            return UserId.ToString();
        }
    }
}

