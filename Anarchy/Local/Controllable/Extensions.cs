using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace Discord
{
    public static class ControllableExtensions
    {
        public static T SetClient<T>(this T @class, IRestClient client) where T : Controllable
        {
            if (@class != null)
                @class.Client = client;
            return @class;
        }

        internal static IReadOnlyList<T> SetClientsInList<T>(this IReadOnlyList<T> classes, IRestClient client) where T : Controllable
        {
            if (classes != null)
            {
                foreach (var @class in classes)
                    @class.Client = client;
            }
            return classes;
        }
    }
}
