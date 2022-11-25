using System.Text.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Linq;

namespace Discord
{
    public class DiscordHttpException : Exception
    {
        public DiscordError Code { get; private set; }
        public string ErrorMessage { get; private set; }

        public FieldErrorDictionary InvalidFields { get; private set; }

        public DiscordHttpException(DiscordHttpError error) : base($"{(int) error.Code} {error.Message}")
        {
            Code = error.Code;
            ErrorMessage = error.Message;

            if (error.Fields.EnumerateObject().Any())
                InvalidFields = FindErrors(error.Fields);
        }

        private static FieldErrorDictionary FindErrors(JsonDocument obj)
        {
            var dict = new FieldErrorDictionary();

            foreach (JsonObject child in obj.Deserialize<List<JsonObject>>())
            {
                if (child.First().Key == "_errors") dict.Errors = child.First().Value.Deserialize<List<DiscordFieldError>>();
                else dict[child.First().Key] = FindErrors(child.First().Value);
            }

            return dict;
        }
    }
}
