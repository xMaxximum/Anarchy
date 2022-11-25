using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;

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

            if (error.Fields.Any())
                InvalidFields = FindErrors(error.Fields);
        }

        private static FieldErrorDictionary FindErrors(JsonObject obj)
        {
            var dict = new FieldErrorDictionary();

            foreach (var child in obj)
            {
                if (child.Key == "_errors") dict.Errors = child.Value.Deserialize<List<DiscordFieldError>>();
                else dict[child.Key] = FindErrors(child.Value.AsObject());
            }

            return dict;
        }
    }
}
