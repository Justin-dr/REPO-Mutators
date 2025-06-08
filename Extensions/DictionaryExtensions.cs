using System;
using System.Collections.Generic;
using System.Linq;

namespace Mutators.Extensions
{
    public static class DictionaryExtensions
    {
        public static T? Get<T>(this IDictionary<string, object> dictionary, string key)
        {
            if (dictionary.TryGetValue(key, out var value))
            {
                return (T)value;
            }
            return default;
        }

        public static T? Get<T>(this IReadOnlyDictionary<string, object> dictionary, string key)
        {
            if (dictionary.TryGetValue(key, out var value))
            {
                return (T)value;
            }
            return default;
        }

        public static List<T> GetAsList<T>(this IDictionary<string, object> metadata, string key) => GetAsList<T>((IReadOnlyDictionary<string, object>)metadata, key);

        public static List<T> GetAsList<T>(this IReadOnlyDictionary<string, object> metadata, string key)
        {
            if (metadata.TryGetValue(key, out var value))
            {
                if (value is List<T> list)
                    return list;

                if (value is IEnumerable<object> objEnumerable)
                    return objEnumerable
                        .Select(o => o is T t ? t : (T)Convert.ChangeType(o, typeof(T)))
                        .ToList();

                if (value is T singleItem)
                    return new List<T> { singleItem };
            }

            return new List<T>();
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
