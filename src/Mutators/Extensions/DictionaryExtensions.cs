using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Mutators.Mutators;
using Mutators.Settings;
//TODO: Don't want Harmony to be a dependency here.

namespace Mutators.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="IDictionary{TKey,TValue}"/> and <see cref="IReadOnlyDictionary{TKey,TValue}"/>.
    /// <para>
    /// These methods are intended to be used to read metadata supplied to an <see cref="IMutator"/>.
    /// </para>
    /// </summary>
    public static class DictionaryExtensions
    {
        /// <summary>
        /// Gets a typed value from a mutable metadata dictionary.
        /// </summary>
        /// <param name="dictionary">The dictionary to read from.</param>
        /// <param name="key">The key of the value to retrieve.</param>
        /// <typeparam name="T">The expected value type.</typeparam>
        /// <returns>The value cast to <typeparamref name="T"/>, or the default value of <typeparamref name="T"/> when the key is not present.</returns>
        public static T? Get<T>(this IDictionary<string, object> dictionary, string key)
        {
            if (dictionary.TryGetValue(key, out object? value))
            {
                return (T)value;
            }
            return default;
        }

        /// <summary>
        /// Gets a typed value from a readonly metadata dictionary.
        /// </summary>
        /// <param name="dictionary">The dictionary to read from.</param>
        /// <param name="key">The key of the value to retrieve.</param>
        /// <typeparam name="T">The expected value type.</typeparam>
        /// <returns>The value cast to <typeparamref name="T"/>, or the default value of <typeparamref name="T"/> when the key is not present.</returns>
        public static T? Get<T>(this IReadOnlyDictionary<string, object> dictionary, string key)
        {
            if (dictionary.TryGetValue(key, out object? value))
            {
                return (T)value;
            }
            return default;
        }

        /// <summary>
        /// Gets a metadata value from a mutable dictionary as a list.
        /// </summary>
        /// <param name="metadata">The metadata dictionary to read from.</param>
        /// <param name="key">The key of the value to retrieve.</param>
        /// <typeparam name="T">The expected list element type.</typeparam>
        /// <returns>A list containing the converted values, or an empty list when the key is not present or the value cannot be converted.</returns>
        public static List<T> GetAsList<T>(this IDictionary<string, object> metadata, string key) => GetAsList<T>((IReadOnlyDictionary<string, object>)metadata, key);

        /// <summary>
        /// Gets a metadata value from a readonly dictionary as a list.
        /// </summary>
        /// <param name="metadata">The metadata dictionary to read from.</param>
        /// <param name="key">The key of the value to retrieve.</param>
        /// <typeparam name="T">The expected list element type.</typeparam>
        /// <returns>A list containing the converted values, or an empty list when the key is not present or the value cannot be converted.</returns>
        public static List<T> GetAsList<T>(this IReadOnlyDictionary<string, object> metadata, string key)
        {
            if (!metadata.TryGetValue(key, out object? value)) return [];
            return value switch
            {
                List<T> list => list,
                IEnumerable<object> objEnumerable => objEnumerable
                    .Select(o => o is T t ? t : (T)Convert.ChangeType(o, typeof(T)))
                    .ToList(),
                T singleItem => [singleItem],
                _ => []
            };
        }

        /// <summary>
        /// Creates a recursive merge of two metadata dictionaries.
        /// </summary>
        /// <remarks>
        /// Null values in the incoming dictionary serve as a remove marker. A key with a null value will be removed from the resulting dictionary.
        /// </remarks>
        /// <param name="dict1">The base dictionary.</param>
        /// <param name="dict2">The dictionary whose values override or extend the base dictionary.</param>
        /// <returns>A new dictionary containing values from both dictionaries, with nested dictionaries merged recursively.</returns>
        public static IDictionary<string, object> DeepMergedWith(this IDictionary<string, object> dict1, IDictionary<string, object> dict2)
        {
            Dictionary<string, object> result = new Dictionary<string, object>(dict1);
            
            foreach (KeyValuePair<string, object> kvp in dict2)
            {
                if (kvp.Value is null)
                {
                    result.Remove(kvp.Key);
                    continue;
                }
            
                if (result.TryGetValue(kvp.Key, out object? val1) && val1 is IDictionary<string, object> sub1 && kvp.Value is IDictionary<string, object> sub2)
                {
                    result[kvp.Key] = sub1.DeepMergedWith(sub2);
                    continue;
                }
            
                result[kvp.Key] = kvp.Value;
            }

            return result;
        }

        /// <summary>
        /// Wraps a dictionary in a dictionary with a single key-value pair.
        /// </summary>
        /// <param name="dictionary">The dictionary to wrap.</param>
        /// <param name="mutatorNamespacedName">The <see cref="AbstractMutatorSettings.NamespacedName">NamespacedName</see> that the dictionary will be wrapped in.</param>
        /// <returns>A new dictionary with a key matching the mutatorNamespacedName parameter, whose value is the original dictionary. Or the original dictionary if it was already keyed.</returns>
        public static IDictionary<string, object> WithMutator(this IDictionary<string, object> dictionary, string mutatorNamespacedName)
        {
            if (dictionary.ContainsKey(mutatorNamespacedName))
            {
                return dictionary;
            }

            return new Dictionary<string, object> { { mutatorNamespacedName, dictionary } };
        }

        /// <summary>
        /// Wraps a dictionary in a dictionary with a single key-value pair.
        /// </summary>
        /// <param name="dictionary">The dictionary to wrap.</param>
        /// <param name="mutator">The Mutator whose <see cref="AbstractMutatorSettings.NamespacedName">NamespacedName</see> will be used to wrap the dictionary.</param>
        /// <returns>A new dictionary with a key matching <see cref="AbstractMutatorSettings.NamespacedName"/>, whose value is the original dictionary. Or the original dictionary if it was already keyed.</returns>
        public static IDictionary<string, object> WithMutator(this IDictionary<string, object> dictionary, IMutator mutator)
        {
            return dictionary.WithMutator(mutator.NamespacedName);
        }
        
        internal static void LogMetadata(this IDictionary<string, object> metadata, int level = 0)
        {
            string indentation = new string(' ', level);
            foreach (KeyValuePair<string, object> item in metadata)
            {
                switch (item.Value)
                {
                    case string s:
                        RepoMutators.Logger.LogDebug($"{indentation}{item.Key}: {s}");
                        break;
                    case IDictionary<string, object> nestedMetadata:
                        RepoMutators.Logger.LogDebug($"{indentation}{item.Key}:");
                        nestedMetadata.LogMetadata(level + 1);
                        break;
                    case IEnumerable<object> enumerable:
                        RepoMutators.Logger.LogDebug($"{indentation}{item.Key}: [{enumerable.Join()}]");
                        break;
                    default:
                        RepoMutators.Logger.LogDebug($"{indentation}{item.Key}: {item.Value}");
                        break;
                }
            }
        }
    }
}
