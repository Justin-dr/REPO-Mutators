using System.Collections;
using System.Collections.Generic;

namespace Mutators.Extensions
{
    internal static class PhotonHashtableExtensions
    {
        internal static ExitGames.Client.Photon.Hashtable ToPhotonHashtable(this IDictionary<string, object> dict)
        {
            var result = new ExitGames.Client.Photon.Hashtable();

            foreach (var kvp in dict)
            {
                if (kvp.Value is IDictionary<string, object> nestedDict)
                {
                    result[kvp.Key] = nestedDict.ToPhotonHashtable();
                }
                else if (kvp.Value is IList list)
                {
                    result[kvp.Key] = list.ToPhotonArray();
                }
                else
                {
                    result[kvp.Key] = kvp.Value;
                }
            }

            return result;
        }

        private static object[] ToPhotonArray(this IList list)
        {
            var result = new object[list.Count];
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] is IDictionary<string, object> nestedDict)
                    result[i] = nestedDict.ToPhotonHashtable();
                else
                    result[i] = list[i];
            }
            return result;
        }

        internal static IDictionary<string, object> FromPhotonHashtable(this ExitGames.Client.Photon.Hashtable hashtable)
        {
            var result = new Dictionary<string, object>();

            foreach (DictionaryEntry entry in hashtable)
            {
                if (entry.Key is not string key) continue;

                object value = entry.Value;

                if (value is ExitGames.Client.Photon.Hashtable nested)
                {
                    result[key] = nested.FromPhotonHashtable();
                }
                else if (value is object[] array)
                {
                    result[key] = array.FromPhotonArray();
                }
                else
                {
                    result[key] = value;
                }
            }

            return result;
        }

        private static IList<object> FromPhotonArray(this object[] array)
        {
            IList<object> result = new List<object>();

            foreach (var item in array)
            {
                if (item is ExitGames.Client.Photon.Hashtable nested)
                    result.Add(nested.FromPhotonHashtable());
                else
                    result.Add(item);
            }

            return result;
        }
    }
}
