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

        public static IDictionary<string, object> DeepMergedWith(this IDictionary<string, object> dict1, IDictionary<string, object> dict2)
        {
            var result = new Dictionary<string, object>(dict1);

            foreach (var kvp in dict2)
            {
                if (result.TryGetValue(kvp.Key, out var val1) && val1 is IDictionary<string, object> sub1 && kvp.Value is IDictionary<string, object> sub2)
                {
                    result[kvp.Key] = sub1.DeepMergedWith(sub2);
                }
                else
                {
                    result[kvp.Key] = kvp.Value;
                }
            }

            return result;
        }
    }
}
