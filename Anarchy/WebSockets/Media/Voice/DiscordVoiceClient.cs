using System.Text.Json.Serialization;
using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using Discord.Gateway;
using Discord.WebSockets;


namespace Discord.Media
{
    public class DiscordVoiceClient
    {
        private readonly DiscordSocketClient _client;
        internal DiscordMediaConnection Connection { get; private set; }

        public MediaConnectionState State => Connection == null ? MediaConnectionState.NotConnected : Connection.State;

        public string SessionId { get; private set; }
        public DiscordMediaServer Server { get; private set; }

        private readonly ulong? _guildId;
        public MinimalGuild Guild => _guildId.HasValue ? new MinimalGuild(_guildId.Value).SetClient(_client.RestClient) : null;

        private ulong? _channelId;
        public MinimalChannel Channel => _channelId.HasValue ? new MinimalChannel(_channelId.Value).SetClient(_client.RestClient) : null;

        public DiscordVoiceInput Microphone { get; private set; }
        public DiscordLivestreamClient Livestream { get; private set; }

        private OpusDecoder _decoder;
        private readonly Anarchy.ConcurrentDictionary<ulong, IncomingVoiceStream> _receivers = new();
        private readonly Anarchy.ConcurrentDictionary<uint, ulong> _ssrcToUserDictionary = new();
        private static readonly byte[] _silenceFrame = new byte[] { 0xF8, 0xFF, 0xFE };

        public DiscordVoiceClient(DiscordSocketClient client, ulong? guildId)
        {
            _client = client;
            _guildId = guildId;
        }

        internal void SetSessionId(string newSessionId) => SessionId = newSessionId;

        internal void SetServer(DiscordMediaServer server)
        {
            Server = server;

            _ssrcToUserDictionary.Clear();
            _receivers.Clear();
            if (_guildId.HasValue) Livestream = new DiscordLivestreamClient(_client, _guildId.Value, _channelId.Value);

            Connection = new DiscordMediaConnection(_client, server.Guild == null ? _channelId.Value : server.Guild.Id, server);

            Connection.OnReady += (c) =>
            {
                Microphone = new DiscordVoiceInput(this);

                Connection.SetSSRC(Connection.SSRC.Audio);
                _client.TriggerVCConnect(this);
            };

            Connection.OnMessage += Connection_OnMessage;
            Connection.OnUdpPacket += Connection_OnUdpPacket;
            Connection.OnDead += Connection_OnDead;

            Connection.ConnectAsync().GetAwaiter().GetResult();
        }

        private void Connection_OnDead(DiscordMediaConnection connection, DiscordWebSocketCloseEventArgs args)
        {
            ulong prevChannel = _channelId.Value;
            _channelId = null;
            _client.TriggerVCDisconnect(_guildId, prevChannel, args);
        }

        private void Connection_OnMessage(DiscordMediaConnection connection, DiscordWebSocketMessage<DiscordMediaOpcode> message)
        {
            switch (message.Opcode)
            {
                case DiscordMediaOpcode.Speaking:
                    var state = message.Data.Deserialize<DiscordSpeakingState>();

                    if (state.UserId.HasValue)
                        _ssrcToUserDictionary[state.SSRC] = state.UserId.Value;
                    break;
                case DiscordMediaOpcode.SSRCUpdate:
                    SSRCUpdate update = message.Data.Deserialize<SSRCUpdate>();
                    _ssrcToUserDictionary[update.Audio] = update.UserId;
                    break;
                case DiscordMediaOpcode.UserDisconnect:
                    ulong userId = message.Data["user_id"].GetValue<ulong>();

                    if (_ssrcToUserDictionary.TryGetKey(userId, out uint ssrc))
                        _ssrcToUserDictionary.Remove(ssrc);
                    break;
            }
        }

        private void Connection_OnUdpPacket(DiscordMediaConnection connection, MediaPacketEventArgs args)
        {
            if (_decoder != null && args.Header.Type == DiscordMediaConnection.SupportedCodecs["opus"].PayloadType && _ssrcToUserDictionary.TryGetValue(args.Header.SSRC, out ulong userId))
            {
                if (!_receivers.TryGetValue(userId, out IncomingVoiceStream receiver))
                {
                    receiver = _receivers[userId] = new IncomingVoiceStream(Connection, userId);
                    _client.TriggerVCSpeaking(this, receiver);
                }

                if (args.Payload.SequenceEqual(_silenceFrame))
                {
                    receiver.SilenceFramesReceived++;

                    if (receiver.SilenceFramesReceived >= 10)
                    {
                        receiver.Close();
                        _receivers.Remove(receiver.UserId);
                    }
                }
                else
                {
                    try
                    {
                        byte[] decoded = new byte[OpusConverter.FrameBytes];
                        int length = _decoder.DecodeFrame(args.Payload, 0, args.Payload.Length, decoded, 0, false);

                        receiver.Enqueue(new DiscordVoicePacket(decoded));
                    }
                    catch (OpusException) { }
                }
            }
        }

        private void KillPreviousConnection()
        {
            foreach (var client in _client.VoiceClients)
            {
                if (client.Key != _guildId && client.Value.State > MediaConnectionState.NotConnected)
                    client.Value.Disconnect();
            }
        }

        public void Connect(ulong channelId, VoiceConnectionProperties properties = null)
        {
            if (!File.Exists("libsodium.dll"))
                throw new FileNotFoundException("libsodium.dll was not found");
            else if (!File.Exists("opus.dll"))
                throw new FileNotFoundException("opus.dll was not found");
            else if (!File.Exists("libsodium.dll"))
                throw new FileNotFoundException("libsodium.dll was not found");

            _decoder = new OpusDecoder();

            if (_client.User.Type == DiscordUserType.User) KillPreviousConnection();

            var state = new VoiceStateProperties() { ChannelId = channelId, GuildId = _guildId };

            if (properties != null)
            {
                if (properties.Muted.HasValue) state.Muted = properties.Muted.Value;
                if (properties.Deafened.HasValue) state.Deafened = properties.Deafened.Value;
                if (properties.Video.HasValue) state.Video = properties.Video.Value;
            }

            _channelId = channelId;
            _client.ChangeVoiceState(state);
        }

        public void Disconnect()
        {
            if (Connection != null && Connection.State > MediaConnectionState.NotConnected)
            {
                _client.ChangeVoiceState(new VoiceStateProperties() { GuildId = _guildId, ChannelId = null });
                Connection.DisconnectAsync(1000, "Closed by user").GetAwaiter().GetResult();
            }
        }

        public void SetSSRC(uint audioSsrc) => Connection.SetSSRC(audioSsrc);
    }
}

