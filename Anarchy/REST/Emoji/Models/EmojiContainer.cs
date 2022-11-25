using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace Discord.Gateway
{
    internal class EmojiContainer : Controllable
    {
        public EmojiContainer()
        {
            OnClientUpdated += (sender, e) =>
            {
                Emojis.SetClientsInList(Client);

                foreach (var emoji in Emojis)
                    emoji.GuildId = GuildId;
            };
        }

        [JsonPropertyName("guild_id")]
        public ulong GuildId { get; private set; }

        [JsonPropertyName("emojis")]
        public IReadOnlyList<DiscordEmoji> Emojis { get; private set; }
    }
}

