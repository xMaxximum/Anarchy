using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Discord
{
    public class DiscordHttpResponse
    {
        public int StatusCode { get; private set; }
        public JsonValue Body { get; private set; }

        public DiscordHttpResponse(int statusCode, string content)
        {
            StatusCode = statusCode;
            if (content != null && content.Length != 0)
                Body = JsonSerializer.Deserialize<JsonValue>(content);

        }

        public T Deserialize<T>()
        {
            return Body.Deserialize<T>();
        }

        public T ParseDeterministic<T>()
        {
            return Body.Deserialize<T>();
        }

        public List<T> MultipleDeterministic<T>()
        {
            return Body.Deserialize<List<T>>();
        }
    }
}

