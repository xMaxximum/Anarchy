using System.Text.Json.Serialization;
using System;

namespace Discord
{
    public class PartialEmoji : Controllable
    {
        public PartialEmoji() { }

        public PartialEmoji(string name)
        {
            Name = name;
        }

        public PartialEmoji(ulong id, string name, bool animated) : this(name)
        {
            Id = id;
            Animated = animated;
        }

        [JsonPropertyName("id")]
        public ulong? Id { get; private set; }

        [JsonPropertyName("name")]
        public string Name { get; protected set; }

        [JsonPropertyName("animated")]
        public bool Animated { get; private set; }

        public DiscordCDNImage Icon
        {
            get
            {
                if (Id.HasValue)
                    return new DiscordCDNImage(CDNEndpoints.Emoji, Id.Value);
                else
                    return null;
            }
        }

        public bool Custom
        {
            get { return Id != null; }
        }

        public string AsMessagable()
        {
            if (Custom)
                return "<" + (Animated ? "a" : "") + $":{Name}:{Id}>";
            else
                throw new NotSupportedException("Emoji must be custom to be converted. To convert standard emojis, get their unicode character");
        }

        public override string ToString()
        {
            return Name;
        }
    }
}

