using System.Text.Json.Serialization;
using System;
using System.Collections.Generic;


namespace Discord
{
    internal static class JsonExtensions
    {
        /*
        internal static bool TryFindTypes(Type type, out Dictionary<int, Type> types)
        {
            if (DeepJsonConverter.RecognizedTypes.TryGetValue(type, out types))
                return true;
            else if (type.BaseType == null)
            {
                types = null;
                return false;
            }
            else
                return TryFindTypes(type.BaseType, out types);
        }

        public static T ParseDeterministic<T>(this JObject obj)
        {
            if (TryFindTypes(typeof(T), out Dictionary<int, Type> types))
            {
                int type = obj.Value<int>("type");
                return (T) obj.ToObject(types.TryGetValue(type, out var t) ? t : typeof(T));
            }
            else
                throw new InvalidCastException("Unable to find any implementations for T");
        }

        public static List<T> MultipleDeterministic<T>(this JArray arr)
        {
            List<T> results = new List<T>();

            foreach (JObject child in arr)
                results.Add(child.ParseDeterministic<T>());

            return results;
        }
        */
    }
}
