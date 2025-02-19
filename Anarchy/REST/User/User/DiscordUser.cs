namespace Discord
{
    using System;
    using System.Text.Json.Serialization;
    using System.Threading.Tasks;
    using Discord.Commands;

    public class DiscordUser : Controllable, IMentionable
    {
        protected static DiscordBadge HypeBadges = DiscordBadge.HypeBravery | DiscordBadge.HypeBrilliance | DiscordBadge.HypeBalance;

        [JsonPropertyName("id")]
        public ulong Id { get; set; }

        [JsonPropertyName("username")]
        public string Username { get; set; }

        [JsonPropertyName("discriminator")]
        public uint Discriminator { get; set; }

        [JsonPropertyName("avatar")]
        protected string _avatarHash;

        public DiscordCDNImage Avatar
        {
            get
            {
                if (_avatarHash == null)
                    return null;
                else
                    return new DiscordCDNImage(CDNEndpoints.Avatar, Id, _avatarHash);
            }
        }

        // understanding what public flags are is difficult because of the lack of documentation
        [JsonPropertyName("public_flags")]
        private DiscordBadge _publicFlags;

        [JsonPropertyName("flags")]
        private DiscordBadge _flags;

        public DiscordBadge Badges
        {
            get
            {
                if (_flags == DiscordBadge.None)
                    return _publicFlags;
                else
                    return _flags;
            }
            protected set
            {
                _flags = value;
            }
        }

        [JsonPropertyName("bot")]
        private readonly bool _bot;

        public DiscordUserType Type
        {
            get
            {
                if (Discriminator == 0)
                    return DiscordUserType.Webhook;
                else
                    return _bot ? DiscordUserType.Bot : DiscordUserType.User;
            }
        }

        public Hypesquad Hypesquad
        {
            get
            {
                switch (Badges & HypeBadges)
                {
                    case DiscordBadge.HypeBrilliance:
                        return Hypesquad.Brilliance;
                    case DiscordBadge.HypeBravery:
                        return Hypesquad.Bravery;
                    case DiscordBadge.HypeBalance:
                        return Hypesquad.Balance;
                    default:
                        return Hypesquad.None;
                }
            }
        }

        public DateTimeOffset CreatedAt
        {
            get { return DateTimeOffset.FromUnixTimeMilliseconds((long) ((Id >> 22) + 1420070400000UL)); }
        }

        internal void Update(DiscordUser user)
        {
            Username = user.Username;
            Discriminator = user.Discriminator;
            _avatarHash = user._avatarHash;
            Badges = user.Badges;
            _publicFlags = user._publicFlags;
        }

        public async Task UpdateAsync()
        {
            Update(await Client.GetUserAsync(Id));
        }

        /// <summary>
        /// Updates the user's info
        /// </summary>
        public void Update()
        {
            UpdateAsync().GetAwaiter().GetResult();
        }

        public async Task<DiscordProfile> GetProfileAsync()
        {
            return await ((RestClient<IUserAccount>)Client).GetProfileAsync(Id);
        }

        /// <summary>
        /// Gets the user's profile
        /// </summary>
        public DiscordProfile GetProfile()
        {
            return GetProfileAsync().GetAwaiter().GetResult();
        }

        public async Task SendFriendRequestAsync()
        {
            if (Id == Client.User.Id)
                throw new NotSupportedException("Cannot send a friend request to self.");

            await ((RestClient<IUserAccount>)Client).SendFriendRequestAsync(Username, Discriminator);
        }

        /// <summary>
        /// Sends a friend request to the user
        /// </summary>
        public void SendFriendRequest()
        {
            SendFriendRequestAsync().GetAwaiter().GetResult();
        }

        public async Task BlockAsync()
        {
            if (Id == Client.User.Id)
                throw new NotSupportedException("Cannot block self.");

            await ((RestClient<IUserAccount>)Client).BlockUserAsync(Id);
        }

        /// <summary>
        /// Blocks the user
        /// </summary>
        public void Block()
        {
            BlockAsync().GetAwaiter().GetResult();
        }

        public async Task RemoveRelationshipAsync()
        {
            if (Id == Client.User.Id)
                throw new NotSupportedException("Cannot remove relationship from self.");

            await ((RestClient<IUserAccount>)Client).RemoveRelationshipAsync(Id);
        }

        /// <summary>
        /// Removes any relationship (unfriending, unblocking etc.)
        /// </summary>
        public void RemoveRelationship()
        {
            RemoveRelationshipAsync().GetAwaiter().GetResult();
        }

        public string AsMessagable()
        {
            return $"<@{Id}>";
        }

        public override string ToString()
        {
            return $"{Username}#{"0000".Remove(4 - Discriminator.ToString().Length) + Discriminator.ToString()}";
        }

        public static implicit operator ulong(DiscordUser instance)
        {
            return instance.Id;
        }
    }
}
