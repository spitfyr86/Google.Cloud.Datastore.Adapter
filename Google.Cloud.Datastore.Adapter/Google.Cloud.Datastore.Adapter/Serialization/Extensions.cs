using System.Collections.Generic;

namespace Google.Cloud.Datastore.Adapter.Serialization
{
    public static class Extensions
    {
        public static bool IsNullOrEmpty(this string s)
        {
            return string.IsNullOrEmpty(s);
        }

        public static void AddRange<K, V>(this IDictionary<K, V> baseDictionary, IDictionary<K, V> copiedDictionary)
        {
            foreach (var metadata in copiedDictionary)
            {
                if (!baseDictionary.ContainsKey(metadata.Key))
                {
                    baseDictionary.Add(metadata.Key, metadata.Value);
                }
            }
        }
    }
}
