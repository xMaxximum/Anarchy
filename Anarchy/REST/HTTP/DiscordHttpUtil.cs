namespace Discord
{
    using System.Net.Http;
    using System.Text.Json;
    using System.Text.Json.Nodes;

    internal static class DiscordHttpUtil
    {
        public static string BuildBaseUrl(uint apiVersion, DiscordReleaseChannel releaseChannel) =>
            $"https://{(releaseChannel == DiscordReleaseChannel.Stable ? "" : releaseChannel.ToString().ToLower() + ".")}discord.com/api/v{apiVersion}";

        public static void ValidateResponse(HttpResponseMessage response)
        {
            string content = response.Content.ReadAsStringAsync().Result;
            JsonValue body = (content != null && content.Length != 0) ? JsonNode.Parse(content).Deserialize<JsonValue>() : throw new System.Exception("Couldn't validate response");

            ValidateResponse(response, body);
        }

        public static void ValidateResponse(HttpResponseMessage response, JsonNode body)
        {
            int statusCode = (int) response.StatusCode;

            if (statusCode >= 400)
            {
                if (statusCode == 429)
                    throw new RateLimitException(body["retry_after"].GetValue<int>());
                else
                    throw new DiscordHttpException(body.Deserialize<DiscordHttpError>());
            }
        }
    }
}
