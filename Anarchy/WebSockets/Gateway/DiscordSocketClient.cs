using System.Text.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using Anarchy;
using Discord.Commands;
using Discord.Media;
using Discord.WebSockets;

namespace Discord.Gateway
{
    /// <summary>
    /// <see cref="IRestClient"/> with Gateway support
    /// </summary>
    public class DiscordSocketClient : IDisposable
    {
        #region events
        public delegate void ClientEventHandler<T>(DiscordSocketClient client, T args);

        public event ClientEventHandler<LoginEventArgs> OnLoggedIn;
        public event ClientEventHandler<LogoutEventArgs> OnLoggedOut;
        public event ClientEventHandler<DiscordSessionsEventArgs> OnSessionsUpdated;

        public event ClientEventHandler<UserEventArgs> OnUserUpdated;
        public event ClientEventHandler<DiscordSettingsEventArgs> OnSettingsUpdated;

        public event ClientEventHandler<SocketGuildEventArgs> OnJoinedGuild;
        public event ClientEventHandler<GuildEventArgs> OnGuildUpdated;
        public event ClientEventHandler<GuildUnavailableEventArgs> OnLeftGuild;

        public event ClientEventHandler<GuildMemberEventArgs> OnUserJoinedGuild;
        public event ClientEventHandler<MemberRemovedEventArgs> OnUserLeftGuild;
        public event ClientEventHandler<GuildMemberEventArgs> OnGuildMemberUpdated;

        public event ClientEventHandler<EmojisUpdatedEventArgs> OnEmojisUpdated;

        public event ClientEventHandler<InviteCreatedEventArgs> OnInviteCreated;
        public event ClientEventHandler<InviteDeletedEventArgs> OnInviteDeleted;

        internal event ClientEventHandler<GuildMembersEventArgs> OnGuildMembersReceived;
        internal event ClientEventHandler<DiscordMemberListUpdate> OnMemberListUpdate;
        public event ClientEventHandler<PresenceUpdatedEventArgs> OnUserPresenceUpdated;

        public event ClientEventHandler<BanUpdateEventArgs> OnUserBanned;
        public event ClientEventHandler<BanUpdateEventArgs> OnUserUnbanned;

        public event ClientEventHandler<RoleEventArgs> OnRoleCreated;
        public event ClientEventHandler<RoleEventArgs> OnRoleUpdated;
        public event ClientEventHandler<RoleDeletedEventArgs> OnRoleDeleted;

        public event ClientEventHandler<ChannelEventArgs> OnChannelCreated;
        public event ClientEventHandler<ChannelEventArgs> OnChannelUpdated;
        public event ClientEventHandler<ChannelEventArgs> OnChannelDeleted;

        public event ClientEventHandler<ThreadEventArgs> OnThreadCreated;
        public event ClientEventHandler<ThreadEventArgs> OnThreadUpdated;
        public event ClientEventHandler<ThreadEventArgs> OnThreadDeleted;
        public event ClientEventHandler<ThreadMembersEventArgs> OnThreadMembersUpdated;

        public event ClientEventHandler<RingingEventArgs> OnRinging;
        public event ClientEventHandler<CallUpdateEventArgs> OnCallUpdated;
        public event ClientEventHandler<MinimalTextChannel> OnCallEnded;

        public event ClientEventHandler<VoiceStateEventArgs> OnVoiceStateUpdated;
        public event ClientEventHandler<DiscordMediaServer> OnMediaServer;

        public event ClientEventHandler<UserTypingEventArgs> OnUserTyping;
        public event ClientEventHandler<MessageEventArgs> OnMessageReceived;
        public event ClientEventHandler<MessageEventArgs> OnMessageEdited;
        public event ClientEventHandler<MessageDeletedEventArgs> OnMessageDeleted;
        public event ClientEventHandler<UnreadMessagesEventArgs> OnGuildUnreadMessagesUpdated;

        public event ClientEventHandler<ReactionEventArgs> OnMessageReactionAdded;
        public event ClientEventHandler<ReactionEventArgs> OnMessageReactionRemoved;

        public event ClientEventHandler<RelationshipEventArgs> OnRelationshipAdded;
        public event ClientEventHandler<RemovedRelationshipEventArgs> OnRelationshipRemoved;

        public event ClientEventHandler<ChannelRecipientEventArgs> OnChannelRecipientAdded;
        public event ClientEventHandler<ChannelRecipientEventArgs> OnChannelRecipientRemoved;

        public event ClientEventHandler<GiftCodeCreatedEventArgs> OnGiftCodeCreated;
        public event ClientEventHandler<GiftCodeUpdatedEventArgs> OnGiftUpdated;

        public event ClientEventHandler<NitroBoostEventArgs> OnBoostSlotCreated;
        public event ClientEventHandler<NitroBoostEventArgs> OnBoostSlotUpdated;

        public event ClientEventHandler<EntitlementEventArgs> OnEntitlementCreated;
        public event ClientEventHandler<EntitlementEventArgs> OnEntitlementUpdated;

        internal event ClientEventHandler<DiscordInteractionEventArgs> OnInteraction;

        public event ClientEventHandler<VoiceConnectEventArgs> OnJoinedVoiceChannel;
        public event ClientEventHandler<VoiceDisconnectEventArgs> OnLeftVoiceChannel;
        public event ClientEventHandler<VoiceChannelSpeakingEventArgs> OnUserSpeaking;

        public event ClientEventHandler<RequiredActionEventArgs> OnRequiredUserAction;
        #endregion

        internal IRestClient RestClient { get; private set; }
        public DiscordHttpClient HttpClient { get => RestClient.HttpClient; }
        public DiscordClientUser User { get => RestClient.User; }

        // caching
        internal ConcurrentDictionary<ulong, SocketGuild> GuildCache { get; private set; }
        internal ConcurrentList<PrivateChannel> PrivateChannels { get; private set; }
        internal ConcurrentDictionary<ulong, DiscordPresence> Presences { get; private set; }
        internal AutoConcurrentDictionary<ulong, DiscordVoiceStateContainer> VoiceStates { get; private set; }
        internal ConcurrentDictionary<ulong, ClientGuildSettings> GuildSettings { get; private set; }
        internal List<DiscordChannelSettings> PrivateChannelSettings { get; private set; }
        internal ConcurrentDictionary<ulong, GuildMember> ClientMembers { get; private set; }


        public CommandHandler CommandHandler { get; private set; }
        private SlashCommandHandler SlashCommandHandler { get; set; }
        public LockedSocketConfig Config { get; private set; }

        internal VoiceClientDictionary VoiceClients { get; private set; }

        public DiscordUserSettings UserSettings { get; private set; }

        // websocket connection
        internal IWebSocketClient<GatewayOpcode> WebSocket { get; private set; }
        public GatewayConnectionState State { get; private set; }
        public bool LoggedIn { get; private set; }
        internal uint? Sequence { get; set; }
        public string SessionId { get; set; }

        internal DateTime Cooldown { get; set; }
        internal object RequestLock { get; private set; }
        internal ulong? Lurking { get; set; }

        private ulong _appId;

        public DiscordSocketClient(IRestClient restClient, DiscordSocketConfig config = null) : base()
        {
            Console.WriteLine($"name: {restClient.GetGuilds()[0].Name}");
            restClient = RestClient;

            RequestLock = new object();

            config ??= new DiscordSocketConfig();

            Config = new LockedSocketConfig(config);

            if (Config.Cache)
            {
                GuildCache = new ConcurrentDictionary<ulong, SocketGuild>();
                PrivateChannels = new ConcurrentList<PrivateChannel>();
                Presences = new ConcurrentDictionary<ulong, DiscordPresence>();
                VoiceStates = new AutoConcurrentDictionary<ulong, DiscordVoiceStateContainer>((userId) => new DiscordVoiceStateContainer(userId));
                GuildSettings = new ConcurrentDictionary<ulong, ClientGuildSettings>();
                PrivateChannelSettings = new List<DiscordChannelSettings>();
                ClientMembers = new ConcurrentDictionary<ulong, GuildMember>();
            }

            WebSocket = DiscordWebSocket<GatewayOpcode>.CreateNew($"wss://gateway.discord.gg/?v={Config.ApiVersion}&encoding=json");

            WebSocket.OnClosed += (s, args) =>
            {
                State = GatewayConnectionState.NotConnected;

                Reset();

                bool lostConnection = 1006 == args.Code || 1001 == args.Code;

                if (lostConnection)
                    Thread.Sleep(200);

                GatewayCloseCode err = (GatewayCloseCode) args.Code;

                if (LoggedIn && (lostConnection || err == GatewayCloseCode.RateLimited || err == GatewayCloseCode.SessionTimedOut || err == GatewayCloseCode.UnknownError))
                {
                    LoggedIn = false;
                    Login();
                }
                else
                {
                    OnLoggedOut?.Invoke(this, new LogoutEventArgs(err, args.Reason));
                }
            };

            WebSocket.OnMessageReceived += WebSocket_OnMessageReceived;

            VoiceClients = new VoiceClientDictionary(this);

            OnMediaServer += (s, e) =>
            {
                if (e.StreamKey == null)
                {
                    if (e.Guild == null) VoiceClients.Private.SetServer(e);
                    else VoiceClients[e.Guild.Id].SetServer(e);
                }
                else
                {
                    var key = new StreamKey(e.StreamKey);
                    VoiceClients[key.GuildId].Livestream.SetSessionServer(key.UserId, e);
                }
            };
        }

        ~DiscordSocketClient()
        {
            Dispose(true);
        }

        public void Login()
        {
            if (RestClient.User.Type == DiscordUserType.Bot && Config.ApiVersion >= 8 && !Config.Intents.HasValue)
                throw new ArgumentNullException(nameof(RestClient.Token), "Gateway intents must be provided as of API v8");

            State = GatewayConnectionState.Connecting;

            WebSocket.ConnectAsync().GetAwaiter().GetResult();
        }

        public void Send<T>(GatewayOpcode op, T requestData)
        {
            lock (RequestLock)
            {
                if (Cooldown > DateTime.Now)
                    Thread.Sleep(Cooldown - DateTime.Now);

                WebSocket.SendMessage(op, requestData);

                Cooldown = DateTime.Now + new TimeSpan(0, 0, 0, 0, 500);
            }
        }

        private void ApplyGuild(SocketGuild guild)
        {
            if (!guild.Unavailable)
            {
                foreach (var member in guild.Members)
                {
                    if (member.User.Id == RestClient.User.Id)
                    {
                        ClientMembers[guild.Id] = member;
                        break;
                    }
                }

                foreach (var state in guild.VoiceStates)
                    VoiceStates[state.UserId].GuildStates[guild.Id] = state;

                foreach (var presence in guild.Presences)
                    Presences[presence.UserId] = presence;
            }
        }

        internal void TriggerVCConnect(DiscordVoiceClient client)
        {
            if (OnJoinedVoiceChannel != null)
                Task.Run(() => OnJoinedVoiceChannel.Invoke(this, new VoiceConnectEventArgs(client)));
        }

        internal void TriggerVCDisconnect(ulong? guildId, ulong channelId, DiscordWebSocketCloseEventArgs args)
        {
            if (OnLeftVoiceChannel != null)
                Task.Run(() => OnLeftVoiceChannel.Invoke(this, new VoiceDisconnectEventArgs(this, guildId, channelId, args)));
        }

        internal void TriggerVCSpeaking(DiscordVoiceClient client, IncomingVoiceStream stream)
        {
            if (OnUserSpeaking != null)
                Task.Run(() => OnUserSpeaking.Invoke(this, new VoiceChannelSpeakingEventArgs(client, stream)));
        }

        private void WebSocket_OnMessageReceived(object sender, DiscordWebSocketMessage<GatewayOpcode> message)
        {
            Sequence = message.Sequence;

            switch (message.Opcode)
            {
                case GatewayOpcode.Event:
                    /*
                    Console.WriteLine(message.EventName);
                    
                    File.AppendAllText("Debug.log", $"{message.EventName}: {message.Data}\n");
                    */

                    switch (message.EventName)
                    {
                        case "READY":
                            LoginEventArgs login = message.Data.Deserialize<LoginEventArgs>().SetClient(this.RestClient);

                            if (login.Application != null) _appId = login.Application["id"].GetValue<ulong>();

                            this.RestClient.User = login.User;
                            this.UserSettings = RestClient.User.Type == DiscordUserType.User ? login.Settings : null;
                            this.SessionId = login.SessionId;

                            if (Config.Cache && this.RestClient.User.Type == DiscordUserType.User)
                            {
                                PrivateChannels.AddRange(login.PrivateChannels);

                                foreach (var presence in login.Presences)
                                    Presences[presence.UserId] = presence;

                                foreach (var guild in login.Guilds)
                                {
                                    ApplyGuild(GuildCache[guild.Id] = (SocketGuild) guild);
                                    VoiceClients[guild.Id] = new DiscordVoiceClient(this, guild.Id);
                                }

                                foreach (var settings in login.ClientGuildSettings)
                                {
                                    if (settings.GuildId.HasValue)
                                        GuildSettings.Add(settings.Guild.Id, settings);
                                    else
                                        PrivateChannelSettings = settings.ChannelOverrides.ToList();
                                }
                            }

                            LoggedIn = true;
                            State = GatewayConnectionState.Connected;

                            if (OnLoggedIn != null)
                                Task.Run(() => OnLoggedIn.Invoke(this, login));
                            break;
                        case "USER_SETTINGS_UPDATE":
                            UserSettings.Update(message.Data.AsObject());

                            if (OnSettingsUpdated != null)
                                Task.Run(() => OnSettingsUpdated.Invoke(this, new DiscordSettingsEventArgs(UserSettings)));
                            break;
                        case "USER_GUILD_SETTINGS_UPDATE":
                            if (Config.Cache)
                            {
                                ClientGuildSettings settings = message.Data.Deserialize<ClientGuildSettings>();

                                if (settings.GuildId.HasValue)
                                    GuildSettings[settings.Guild.Id] = settings;
                                else
                                    PrivateChannelSettings = settings.ChannelOverrides.ToList();
                            }
                            break;
                        case "USER_UPDATE":
                            DiscordUser user = message.Data.Deserialize<DiscordUser>().SetClient(this.RestClient);

                            if (user.Id == RestClient.User.Id)
                                RestClient.User.Update(user);

                            if (Config.Cache)
                            {
                                lock (PrivateChannels.Lock)
                                {
                                    foreach (var dm in PrivateChannels)
                                    {
                                        foreach (var recipient in dm.Recipients)
                                        {
                                            if (recipient.Id == user.Id)
                                            {
                                                recipient.Update(user);

                                                break;
                                            }
                                        }
                                    }
                                }
                            }

                            if (OnUserUpdated != null)
                                Task.Run(() => OnUserUpdated.Invoke(this, new UserEventArgs(user)));
                            break;
                        case "GUILD_MEMBER_LIST_UPDATE":
                            OnMemberListUpdate?.Invoke(this, message.Data.Deserialize<DiscordMemberListUpdate>().SetClient(this.RestClient));
                            break;
                        case "GUILD_CREATE":
                            if (Config.Cache || OnJoinedGuild != null)
                            {
                                var guild = message.Data.Deserialize<SocketGuild>().SetClient(this.RestClient);

                                VoiceClients[guild.Id] = new DiscordVoiceClient(this, guild.Id);

                                if (Config.Cache)
                                    ApplyGuild(GuildCache[guild.Id] = guild);

                                if (OnJoinedGuild != null)
                                    Task.Run(() => OnJoinedGuild.Invoke(this, new SocketGuildEventArgs(guild, Lurking.HasValue && Lurking.Value == guild.Id)));
                            }
                            break;
                        case "GUILD_UPDATE":
                            if (Config.Cache || OnGuildUpdated != null)
                            {
                                DiscordGuild guild = message.Data.Deserialize<DiscordGuild>().SetClient(this.RestClient);

                                if (Config.Cache)
                                    GuildCache[guild.Id].Update(guild);

                                Task.Run(() => OnGuildUpdated?.Invoke(this, new GuildEventArgs(guild)));
                            }
                            break;
                        case "GUILD_DELETE":
                            {
                                UnavailableGuild guild = message.Data.Deserialize<UnavailableGuild>();

                                VoiceClients.Remove(guild.Id);

                                if (Lurking.HasValue && Lurking.Value == guild.Id)
                                    Lurking = null;

                                if (Config.Cache)
                                {
                                    if (guild.Unavailable)
                                        GuildCache[guild.Id].Unavailable = true;
                                    else
                                    {
                                        GuildCache.Remove(guild.Id);
                                        GuildSettings.Remove(guild.Id);
                                    }
                                }

                                if (OnLeftGuild != null)
                                    Task.Run(() => OnLeftGuild.Invoke(this, new GuildUnavailableEventArgs(guild)));
                            }
                            break;
                        case "GUILD_MEMBER_ADD":
                            if (Config.Cache || OnUserJoinedGuild != null)
                            {
                                var member = message.Data.Deserialize<GuildMember>().SetClient(this.RestClient);

                                if (Config.Cache)
                                    GuildCache[member.GuildId].MemberCount++;

                                Task.Run(() => OnUserJoinedGuild?.Invoke(this, new GuildMemberEventArgs(member)));
                            }
                            break;
                        case "GUILD_MEMBER_REMOVE":
                            if (Config.Cache || OnUserLeftGuild != null)
                            {
                                var member = message.Data.Deserialize<PartialGuildMember>().SetClient(this.RestClient);

                                if (Config.Cache && GuildCache.ContainsKey(member.Guild.Id))
                                    GuildCache[member.Guild.Id].MemberCount--;

                                Task.Run(() => OnUserLeftGuild?.Invoke(this, new MemberRemovedEventArgs(member)));
                            }
                            break;
                        case "GUILD_MEMBER_UPDATE":
                            if (Config.Cache || OnGuildMemberUpdated != null)
                            {
                                GuildMember member = message.Data.Deserialize<GuildMember>().SetClient(this.RestClient);

                                if (Config.Cache && member.User.Id == RestClient.User.Id)
                                {
                                    SocketGuild guild = this.GetCachedGuild(member.GuildId);

                                    // Discord doesn't send us the user's JoinedAt on updates
                                    member.JoinedAt = guild.ClientMember.JoinedAt;
                                    ClientMembers[guild.Id] = member;

                                    break;
                                }

                                if (OnGuildMemberUpdated != null)
                                    Task.Run(() => OnGuildMemberUpdated.Invoke(this, new GuildMemberEventArgs(member)));
                            }
                            break;
                        case "GUILD_MEMBERS_CHUNK":
                            Task.Run(() => OnGuildMembersReceived?.Invoke(this, new GuildMembersEventArgs(message.Data.Deserialize<GuildMemberList>().SetClient(this.RestClient))));
                            break;
                        case "GIFT_CODE_CREATE":
                            if (OnGiftCodeCreated != null)
                                Task.Run(() => OnGiftCodeCreated.Invoke(this, message.Data.Deserialize<GiftCodeCreatedEventArgs>()));
                            break;
                        case "GIFT_CODE_UPDATE":
                            if (OnGiftUpdated != null)
                            {
                                var gift = message.Data.Deserialize<GiftCodeUpdatedEventArgs>().SetClient(this.RestClient);
                                gift.Json = message.Data;

                                Task.Run(() => OnGiftUpdated.Invoke(this, gift));
                            }
                            break;
                        case "PRESENCE_UPDATE":
                            if (Config.Cache || OnUserPresenceUpdated != null)
                            {
                                var presence = message.Data.Deserialize<DiscordPresence>().SetClient(this.RestClient);

                                if (Config.Cache)
                                {
                                    if (Presences.TryGetValue(presence.UserId, out DiscordPresence existingPresence))
                                    {
                                        existingPresence.Update(presence);
                                        presence = existingPresence;
                                    }
                                    else
                                        Presences[presence.UserId] = presence;
                                }

                                if (OnUserPresenceUpdated != null)
                                    Task.Run(() => OnUserPresenceUpdated.Invoke(this, new PresenceUpdatedEventArgs(presence)));
                            }
                            break;
                        case "VOICE_STATE_UPDATE":
                            try
                            {
                                DiscordVoiceState newState = message.Data.Deserialize<DiscordVoiceState>().SetClient(this.RestClient);

                                if (Config.Cache)
                                {
                                    if (newState.Guild == null)
                                        VoiceStates[newState.UserId].PrivateChannelVoiceState = newState;
                                    else
                                        VoiceStates[newState.UserId].GuildStates[newState.Guild.Id] = newState;

                                    // we also store voice states within SocketGuilds, so make sure to update those.
                                    foreach (var guild in this.GetCachedGuilds())
                                    {
                                        if (!guild.Unavailable)
                                        {
                                            if (newState.Guild == null || guild.Id != newState.Guild.Id)
                                                guild._voiceStates.RemoveFirst(s => s.UserId == newState.UserId);
                                            else
                                            {
                                                int i = guild._voiceStates.FindIndex(s => s.UserId == newState.UserId);

                                                if (i > -1)
                                                    guild._voiceStates[i] = newState;
                                                else
                                                    guild._voiceStates.Add(newState);
                                            }
                                        }
                                    }
                                }

                                if (newState.UserId == RestClient.User.Id)
                                {
                                    if (newState.Guild == null) VoiceClients.Private.SetSessionId(newState.SessionId);
                                    else VoiceClients[newState.Guild.Id].SetSessionId(newState.SessionId);
                                }

                                if (OnVoiceStateUpdated != null)
                                    Task.Run(() => OnVoiceStateUpdated.Invoke(this, new VoiceStateEventArgs(newState)));
                            }
                            catch (JsonException) { } // very lazy fix for joined_at sometimes being null
                            break;
                        case "VOICE_SERVER_UPDATE":
                            OnMediaServer?.Invoke(this, message.Data.Deserialize<DiscordMediaServer>().SetClient(this.RestClient));
                            break;
                        case "GUILD_ROLE_CREATE":
                            if (Config.Cache || OnRoleCreated != null)
                            {
                                DiscordRole role = message.Data.Deserialize<RoleUpdate>().Role.SetClient(this.RestClient);

                                if (Config.Cache)
                                    GuildCache[role.GuildId]._roles.Add(role);

                                if (OnRoleCreated != null)
                                    Task.Run(() => OnRoleCreated.Invoke(this, new RoleEventArgs(role)));
                            }
                            break;
                        case "GUILD_ROLE_UPDATE":
                            if (Config.Cache || OnRoleUpdated != null)
                            {
                                DiscordRole role = message.Data.Deserialize<RoleUpdate>().Role.SetClient(this.RestClient);

                                if (Config.Cache)
                                    GuildCache[role.GuildId]._roles.ReplaceFirst(r => r.Id == role.Id, role);

                                if (OnRoleUpdated != null)
                                    Task.Run(() => OnRoleUpdated.Invoke(this, new RoleEventArgs(role)));
                            }
                            break;
                        case "GUILD_ROLE_DELETE":
                            if (Config.Cache || OnRoleDeleted != null)
                            {
                                DeletedRole role = message.Data.Deserialize<DeletedRole>().SetClient(this.RestClient);

                                if (Config.Cache)
                                    GuildCache[role.Guild]._roles.RemoveFirst(r => r.Id == role.Id);

                                if (OnRoleDeleted != null)
                                    Task.Run(() => OnRoleDeleted.Invoke(this, new RoleDeletedEventArgs(role)));
                            }
                            break;
                        case "GUILD_EMOJIS_UPDATE":
                            if (Config.Cache || OnEmojisUpdated != null)
                            {
                                var emojis = message.Data.Deserialize<EmojiContainer>().SetClient(this.RestClient);

                                if (Config.Cache)
                                    GuildCache[emojis.GuildId]._emojis = emojis.Emojis.ToList();

                                if (OnEmojisUpdated != null)
                                    Task.Run(() => OnEmojisUpdated.Invoke(this, new EmojisUpdatedEventArgs(emojis)));
                            }
                            break;
                        case "CHANNEL_CREATE":
                            if (Config.Cache || OnChannelCreated != null)
                            {
                                var channel = message.Data.Deserialize<DiscordChannel>();

                                if (Config.Cache)
                                {
                                    if (channel.Type == ChannelType.DM || channel.Type == ChannelType.Group)
                                        PrivateChannels.Add((PrivateChannel) channel);
                                    else
                                    {
                                        GuildChannel guildChannel = (GuildChannel) channel;

                                        GuildCache[guildChannel.GuildId].ChannelsConcurrent.Add(guildChannel);
                                    }
                                }

                                if (OnChannelCreated != null)
                                    Task.Run(() => OnChannelCreated.Invoke(this, new ChannelEventArgs(channel)));
                            }
                            break;
                        case "CHANNEL_UPDATE":
                            if (Config.Cache || OnChannelUpdated != null)
                            {
                                var channel = message.Data.Deserialize<DiscordChannel>();

                                if (Config.Cache)
                                {
                                    if (channel.Type == ChannelType.DM || channel.Type == ChannelType.Group)
                                        PrivateChannels.ReplaceFirst(c => c.Id == channel.Id, (PrivateChannel) channel);
                                    else
                                    {
                                        GuildChannel guildChannel = (GuildChannel) channel;
                                        GuildCache[guildChannel.GuildId].ChannelsConcurrent.ReplaceFirst(c => c.Id == guildChannel.Id, guildChannel);
                                    }
                                }

                                if (OnChannelUpdated != null)
                                    Task.Run(() => OnChannelUpdated.Invoke(this, new ChannelEventArgs(channel)));
                            }
                            break;
                        case "CHANNEL_DELETE":
                            if (Config.Cache || OnChannelDeleted != null)
                            {
                                var channel = message.Data.Deserialize<DiscordChannel>();

                                if (Config.Cache)
                                {
                                    if (channel.Type == ChannelType.DM || channel.Type == ChannelType.Group)
                                        PrivateChannels.RemoveFirst(c => c.Id == channel.Id);
                                    else
                                        GuildCache[((GuildChannel) channel).GuildId].ChannelsConcurrent.RemoveFirst(c => c.Id == channel.Id);
                                }

                                if (OnChannelDeleted != null)
                                    Task.Run(() => OnChannelDeleted.Invoke(this, new ChannelEventArgs(channel)));
                            }
                            break;
                        case "TYPING_START":
                            if (OnUserTyping != null)
                                Task.Run(() => OnUserTyping.Invoke(this, new UserTypingEventArgs(message.Data.Deserialize<UserTyping>().SetClient(this.RestClient))));
                            break;
                        case "MESSAGE_CREATE":
                            if (Config.Cache || OnMessageReceived != null)
                            {
                                var newMessage = message.Data.Deserialize<DiscordMessage>().SetClient(this.RestClient);

                                if (Config.Cache)
                                {
                                    try
                                    {
                                        this.GetChannel(newMessage.Channel.Id).SetLastMessageId(newMessage.Id);
                                    }
                                    catch (DiscordHttpException) { }
                                }

                                if (OnMessageReceived != null)
                                    Task.Run(() => OnMessageReceived.Invoke(this, new MessageEventArgs(newMessage)));
                            }
                            break;
                        case "MESSAGE_UPDATE":
                            if (OnMessageEdited != null)
                                Task.Run(() => OnMessageEdited.Invoke(this, new MessageEventArgs(message.Data.Deserialize<DiscordMessage>().SetClient(this.RestClient))));
                            break;
                        case "MESSAGE_DELETE":
                            if (OnMessageDeleted != null)
                                Task.Run(() => OnMessageDeleted.Invoke(this, new MessageDeletedEventArgs(message.Data.Deserialize<DeletedMessage>().SetClient(this.RestClient))));
                            break;
                        case "MESSAGE_REACTION_ADD":
                            if (OnMessageReactionAdded != null)
                                Task.Run(() => OnMessageReactionAdded.Invoke(this, new ReactionEventArgs(message.Data.Deserialize<MessageReactionUpdate>().SetClient(this.RestClient))));
                            break;
                        case "MESSAGE_REACTION_REMOVE":
                            if (OnMessageReactionRemoved != null)
                                Task.Run(() => OnMessageReactionRemoved.Invoke(this, new ReactionEventArgs(message.Data.Deserialize<MessageReactionUpdate>().SetClient(this.RestClient))));
                            break;
                        case "GUILD_BAN_ADD":
                            if (OnUserBanned != null)
                                Task.Run(() => OnUserBanned.Invoke(this, message.Data.Deserialize<BanUpdateEventArgs>().SetClient(this.RestClient)));
                            break;
                        case "GUILD_BAN_REMOVE":
                            if (OnUserUnbanned != null)
                                Task.Run(() => OnUserUnbanned.Invoke(this, message.Data.Deserialize<BanUpdateEventArgs>().SetClient(this.RestClient)));
                            break;
                        case "INVITE_CREATE":
                            if (OnInviteCreated != null)
                                Task.Run(() => OnInviteCreated.Invoke(this, message.Data.Deserialize<InviteCreatedEventArgs>().SetClient(this.RestClient)));
                            break;
                        case "INVITE_DELETE":
                            if (OnInviteDeleted != null)
                                Task.Run(() => OnInviteDeleted.Invoke(this, message.Data.Deserialize<InviteDeletedEventArgs>().SetClient(this.RestClient)));
                            break;
                        case "RELATIONSHIP_ADD":
                            if (OnRelationshipAdded != null)
                                Task.Run(() => OnRelationshipAdded.Invoke(this, new RelationshipEventArgs(message.Data.Deserialize<DiscordRelationship>().SetClient(this.RestClient))));
                            break;
                        case "RELATIONSHIP_REMOVE":
                            if (OnRelationshipRemoved != null)
                                Task.Run(() => OnRelationshipRemoved.Invoke(this, message.Data.Deserialize<RemovedRelationshipEventArgs>()));
                            break;
                        case "CHANNEL_RECIPIENT_ADD":
                            if (Config.Cache || OnChannelRecipientAdded != null)
                            {
                                var recipUpdate = message.Data.Deserialize<ChannelRecipientEventArgs>().SetClient(this.RestClient);

                                if (Config.Cache)
                                    ((PrivateChannel) this.GetChannel(recipUpdate.Channel.Id))._recipients.Add(recipUpdate.User);

                                if (OnChannelRecipientAdded != null)
                                    Task.Run(() => OnChannelRecipientAdded.Invoke(this, recipUpdate));
                            }
                            break;
                        case "CHANNEL_RECIPIENT_REMOVE":
                            if (Config.Cache || OnChannelRecipientAdded != null)
                            {
                                var recipUpdate = message.Data.Deserialize<ChannelRecipientEventArgs>().SetClient(this.RestClient);

                                if (Config.Cache)
                                    ((PrivateChannel) this.GetChannel(recipUpdate.Channel.Id))._recipients.RemoveFirst(u => u.Id == recipUpdate.User.Id);

                                if (OnChannelRecipientRemoved != null)
                                    Task.Run(() => OnChannelRecipientRemoved.Invoke(this, recipUpdate));
                            }
                            break;
                        case "MESSAGE_ACK": // triggered whenever another person logged into the account acknowledges a message
                            break;
                        case "SESSIONS_REPLACE":
                            if (OnSessionsUpdated != null)
                                Task.Run(() => OnSessionsUpdated.Invoke(this, new DiscordSessionsEventArgs(message.Data.Deserialize<List<DiscordSession>>())));
                            break;
                        case "CALL_CREATE":
                            if (Config.Cache || OnRinging != null)
                            {
                                var call = message.Data.Deserialize<DiscordCall>().SetClient(this.RestClient);
                                var voiceStates = message.Data["voice_states"].Deserialize<IReadOnlyList<DiscordVoiceState>>().SetClientsInList(this.RestClient);

                                if (Config.Cache)
                                {
                                    foreach (var state in voiceStates)
                                        VoiceStates[state.UserId].PrivateChannelVoiceState = state;
                                }

                                if (OnRinging != null)
                                    Task.Run(() => OnRinging.Invoke(this, new RingingEventArgs(call, voiceStates)));
                            }
                            break;
                        case "CALL_UPDATE":
                            if (OnCallUpdated != null)
                                Task.Run(() => OnCallUpdated.Invoke(this, new CallUpdateEventArgs(message.Data.Deserialize<DiscordCall>().SetClient(this.RestClient))));
                            break;
                        case "CALL_DELETE":
                            if (Config.Cache || OnCallEnded != null)
                            {
                                ulong channelId = message.Data["channel_id"].GetValue<ulong>();

                                if (Config.Cache)
                                {
                                    foreach (var state in VoiceStates.CreateCopy().Values)
                                    {
                                        var privState = state.PrivateChannelVoiceState;

                                        if (privState != null && privState.Channel != null && privState.Channel.Id == channelId)
                                            state.PrivateChannelVoiceState = null;
                                    }
                                }

                                if (OnCallEnded != null)
                                    Task.Run(() => OnCallEnded.Invoke(this, new MinimalTextChannel(channelId).SetClient(this.RestClient)));
                            }
                            break;
                        case "ENTITLEMENT_CREATE":
                            if (OnEntitlementCreated != null)
                                Task.Run(() => OnEntitlementCreated.Invoke(this, new EntitlementEventArgs(message.Data.Deserialize<DiscordEntitlement>())));
                            break;
                        case "ENTITLEMENT_UPDATE":
                            if (OnEntitlementUpdated != null)
                                Task.Run(() => OnEntitlementUpdated.Invoke(this, new EntitlementEventArgs(message.Data.Deserialize<DiscordEntitlement>())));
                            break;
                        case "USER_PREMIUM_GUILD_SUBSCRIPTION_SLOT_CREATE":
                            if (OnBoostSlotCreated != null)
                                Task.Run(() => OnBoostSlotCreated.Invoke(this, new NitroBoostEventArgs(message.Data.Deserialize<DiscordBoostSlot>().SetClient(this.RestClient))));
                            break;
                        case "USER_PREMIUM_GUILD_SUBSCRIPTION_SLOT_UPDATE":
                            if (OnBoostSlotUpdated != null)
                                Task.Run(() => OnBoostSlotUpdated.Invoke(this, new NitroBoostEventArgs(message.Data.Deserialize<DiscordBoostSlot>().SetClient(this.RestClient))));
                            break;
                        case "STREAM_SERVER_UPDATE":
                            OnMediaServer?.Invoke(this, message.Data.Deserialize<DiscordMediaServer>().SetClient(this.RestClient));
                            break;
                        case "STREAM_CREATE":
                            var create = message.Data.Deserialize<GoLiveCreate>();
                            GetVoiceClient(new StreamKey(create.StreamKey).GuildId).Livestream.CreateSession(create);
                            break;
                        case "STREAM_UPDATE":
                            var update = message.Data.Deserialize<GoLiveUpdate>();
                            GetVoiceClient(new StreamKey(update.StreamKey).GuildId).Livestream.UpdateSession(update);
                            break;
                        case "STREAM_DELETE":
                            var delete = message.Data.Deserialize<GoLiveDelete>();
                            GetVoiceClient(new StreamKey(delete.StreamKey).GuildId).Livestream.KillSession(delete);
                            break;
                        case "CHANNEL_UNREAD_UPDATE":
                            if (Config.Cache || OnGuildUnreadMessagesUpdated != null)
                            {
                                var unread = message.Data.Deserialize<GuildUnreadMessages>().SetClient(this.RestClient);

                                if (Config.Cache)
                                {
                                    foreach (var unreadChannel in unread.Channels)
                                        this.GetChannel(unreadChannel.Channel.Id).SetLastMessageId(unreadChannel.LastMessageId);
                                }

                                if (OnGuildUnreadMessagesUpdated != null)
                                    Task.Run(() => OnGuildUnreadMessagesUpdated.Invoke(this, new UnreadMessagesEventArgs(unread)));
                            }
                            break;
                        case "INTERACTION_CREATE":
                            if (OnInteraction != null)
                                Task.Run(() => OnInteraction.Invoke(this, new DiscordInteractionEventArgs(message.Data.Deserialize<DiscordInteraction>().SetClient(this.RestClient))));
                            break;
                        case "USER_REQUIRED_ACTION_UPDATE":
                            if (OnRequiredUserAction != null)
                                Task.Run(() => OnRequiredUserAction.Invoke(this, message.Data.Deserialize<RequiredActionEventArgs>()));
                            break;
                        case "THREAD_CREATE":
                            if (Config.Cache || OnThreadCreated != null)
                            {
                                var thread = message.Data.Deserialize<DiscordThread>();

                                if (Config.Cache)
                                {
                                    var list = new List<DiscordThread>(GuildCache[thread.Guild.Id].Threads)
                                    {
                                        thread
                                    };

                                    GuildCache[thread.Guild.Id].Threads = list;
                                }

                                if (OnThreadCreated != null)
                                    Task.Run(() => OnThreadCreated.Invoke(this, new ThreadEventArgs(thread)));
                            }
                            break;
                        case "THREAD_UPDATE":
                            if (Config.Cache || OnThreadUpdated != null)
                            {
                                var thread = message.Data.Deserialize<DiscordThread>();

                                if (Config.Cache)
                                {
                                    var list = new List<DiscordThread>(GuildCache[thread.Guild.Id].Threads);

                                    for (int i = 0; i < list.Count; i++)
                                    {
                                        if (list[i].Id == thread.Id)
                                        {
                                            list[i] = thread;
                                            break;
                                        }
                                    }

                                    GuildCache[thread.Guild.Id].Threads = list;
                                }

                                if (OnThreadUpdated != null)
                                    Task.Run(() => OnThreadUpdated.Invoke(this, new ThreadEventArgs(thread)));
                            }
                            break;
                        case "THREAD_DELETE":
                            if (Config.Cache || OnThreadDeleted != null)
                            {
                                var thread = message.Data.Deserialize<DiscordThread>();

                                if (Config.Cache)
                                {
                                    var list = new List<DiscordThread>(GuildCache[thread.Guild.Id].Threads);
                                    list.RemoveAll(t => t.Id == thread.Id);
                                    GuildCache[thread.Guild.Id].Threads = list;
                                }

                                if (OnThreadUpdated != null)
                                    Task.Run(() => OnThreadUpdated.Invoke(this, new ThreadEventArgs(thread)));
                            }
                            break;
                        case "THREAD_MEMBERS_UPDATE":
                            if (Config.Cache || OnThreadMembersUpdated != null)
                            {
                                var ev = message.Data.Deserialize<ThreadMembersEventArgs>();

                                if (Config.Cache)
                                {
                                    lock (GuildCache.Lock)
                                    {
                                        foreach (var guild in GuildCache.Values)
                                        {
                                            foreach (var thread in guild.Threads)
                                            {
                                                if (thread.Id == ev.Id)
                                                {
                                                    thread.MemberCount = ev.MemberCount;

                                                    var members = new List<ulong>(thread.MemberPreview);

                                                    if (ev.AddedMembers != null)
                                                    {
                                                        foreach (var added in ev.AddedMembers)
                                                            members.Add(added.UserId);
                                                    }

                                                    if (ev.RemovedMembers != null)
                                                        members.RemoveAll(id => ev.RemovedMembers.Contains(id));
                                                }
                                            }
                                        }
                                    }
                                }

                                if (OnThreadMembersUpdated != null)
                                    Task.Run(() => OnThreadMembersUpdated.Invoke(this, ev));
                            }
                            break;
                    }
                    break;
                case GatewayOpcode.InvalidSession:
                    LoggedIn = false;

                    this.LoginToGateway();
                    break;
                case GatewayOpcode.Connected:
                    this.LoginToGateway();

                    Task.Run(() =>
                    {
                        int interval = message.Data["heartbeat_interval"].GetValue<int>() - 1000;
                        try
                        {
                            while (true)
                            {
                                this.Send(GatewayOpcode.Heartbeat, this.Sequence);
                                Thread.Sleep(interval);
                            }
                        }
                        catch { }
                    });

                    break;
            }
        }

        public void Logout()
        {
            if (LoggedIn)
            {
                LoggedIn = false;

                WebSocket.DisconnectAsync((int) GatewayCloseCode.ClosedByClient, "Closed by client").GetAwaiter().GetResult();
            }
        }

        public void CreateCommandHandler(string prefix)
        {
            CommandHandler = new CommandHandler(prefix, this);
        }

        public void RegisterSlashCommands(ulong? guildId = null)
        {
            if (!LoggedIn) throw new InvalidOperationException("You must be logged in to register slash commands");
            if (SlashCommandHandler == null || SlashCommandHandler.ApplicationId != _appId) SlashCommandHandler = new SlashCommandHandler(this, _appId, guildId);
        }

        public DiscordVoiceClient GetVoiceClient(ulong guildId) => VoiceClients[guildId];
        public DiscordVoiceClient GetPrivateVoiceClient() => VoiceClients.Private;

        private void Reset()
        {
            SessionId = null;

            if (Config.Cache)
            {
                GuildCache.Clear();
                PrivateChannels.Clear();
                VoiceStates.Clear();
                GuildSettings.Clear();
                PrivateChannelSettings.Clear();
                VoiceClients.Clear();
                ClientMembers.Clear();
            }
        }

        internal void Dispose(bool destructor)
        {
            Logout();

            if (!destructor)
            {
                WebSocket.Dispose();
                Reset();
            }
        }

        public void Dispose()
        {
            Dispose(false);
            GC.SuppressFinalize(this);
        }
    }
}
