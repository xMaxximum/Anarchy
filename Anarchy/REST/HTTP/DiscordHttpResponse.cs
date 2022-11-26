namespace Discord
{
    using System.Collections.Generic;
    using System.Text.Json;
    using System.Text.Json.Nodes;

    public class DiscordHttpResponse
    {
        public int StatusCode { get; private set; }
        public JsonNode Body { get; private set; }

        public DiscordHttpResponse(int statusCode, string content)
        {
            StatusCode = statusCode;
            if (content != null && content.Length != 0)
                Body = JsonNode.Parse(content);
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

