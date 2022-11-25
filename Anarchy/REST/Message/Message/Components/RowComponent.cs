using System.Text.Json.Serialization;
using System.Collections.Generic;


namespace Discord
{
    public class RowComponent : MessageComponent
    {
        internal RowComponent()
        {
            Type = MessageComponentType.Row;
        }

        public RowComponent(List<MessageInputComponent> children) : this()
        {
            Components = children;
        }

        [JsonPropertyName("components")]
        //[JsonConverter(typeof(DeepJsonConverter<MessageInputComponent>))]
        public List<MessageInputComponent> Components { get; private set; }
    }
}

