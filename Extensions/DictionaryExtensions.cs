using System.Collections.Generic;

namespace Mutators.Extensions
{
    public static class DictionaryExtensions
    {
        public static T Get<T>(this IDictionary<string, object> dictionary, string key)
        {
            return (T)dictionary[key];
        }

        public static T Get<T>(this IReadOnlyDictionary<string, object> dictionary, string key)
        {
            return (T)dictionary[key];
        }
    }
}
