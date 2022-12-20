using System.Collections.Generic;
using System.Linq;

namespace MCRA.Utils.Collections {
    /// <summary>
    /// ThreeKeyDictionary
    /// </summary>
    /// <typeparam name="TKey1">Type of 1st part of key</typeparam>
    /// <typeparam name="TKey2">Type of 2nd part of key</typeparam>
    /// <typeparam name="TKey3">Type of 3rd part of key</typeparam>
    /// <typeparam name="TValue">Type of the value to store in the dictionary</typeparam>
    public class ThreeKeyDictionary<TKey1, TKey2, TKey3, TValue> : IEnumerable<KeyValuePair<(TKey1, TKey2, TKey3), TValue>> {
        private readonly Dictionary<(TKey1, TKey2, TKey3), TValue> _dict = new();

        /// <summary>
        /// Initializes a new instance of the System.Collections.Dictionary[TKey,TValue] class that is empty,
        /// has the default concurrency level, has the default initial capacity,
        /// and uses the default comparer for the key type.
        /// </summary>
        public ThreeKeyDictionary() {
        }

        /// <summary>
        /// Gets the number of key/value pairs contained in the System.Collections.Dictionary[TKey,TValue].
        /// </summary>
        public int Count => _dict.Count;

        /// <summary>
        /// Gets a collection containing the keys in the System.Collections.Generic.Dictionary[TKey,TValue].
        /// </summary>
        public ICollection<(TKey1, TKey2, TKey3)> Keys => _dict.Keys;

        /// <summary>
        /// Gets a collection containing the values in the System.Collections.Generic.Dictionary[TKey,TValue].
        /// </summary>
        public ICollection<TValue> Values => _dict.Values;

        /// <summary>
        /// Gets or sets the value associated with the specified key.
        /// </summary>
        /// <param name="key1">Key 1st part</param>
        /// <param name="key2">Key 2nd part</param>
        /// <param name="key3">Key 3rd part</param>
        /// <returns>Returns the Value property of the System.Collections.Generic.KeyValuePair[TKey,TValue]
        /// at the specified index.</returns>
        public TValue this[TKey1 key1, TKey2 key2, TKey3 key3] {
            get {
                return _dict[(key1, key2, key3)];
            }
            set {
                _dict[(key1, key2, key3)] = value;
            }
        }

        /// <summary>
        /// Removes all keys and values from the System.Collections.Dictionary[TKey,TValue].
        /// </summary>
        public void Clear() {
            _dict.Clear();
        }

        /// <summary>
        /// Determines whether the System.Collections.Concurrent.ConcurrentDictionary[TKey,TValue]
        /// contains the specified key.
        /// </summary>
        /// <param name="key1">Key 1st part</param>
        /// <param name="key2">Key 2nd part</param>
        /// <param name="key3">Key 3rd part</param>
        /// <returns> true if the System.Collections.Concurrent.ConcurrentDictionary[TKey,TValue]
        /// contains an element with the specified key; otherwise, false.</returns>
        public bool ContainsKey(TKey1 key1, TKey2 key2, TKey3 key3) {
            return _dict.ContainsKey((key1, key2, key3));
        }

        /// <summary>
        /// Returns an enumerator that iterates through the System.Collections.Concurrent.ConcurrentDictionary[TKey,TValue].
        /// </summary>
        /// <returns>An enumerator for the System.Collections.Concurrent.ConcurrentDictionary[TKey,TValue].</returns>
        public IEnumerator<KeyValuePair<(TKey1, TKey2, TKey3), TValue>> GetEnumerator() {
            return this.GetEnumerator();
        }

        /// <summary>
        /// Copies the key and value pairs stored in the System.Collections.Concurrent.ConcurrentDictionary[TKey,TValue] to a new array.
        /// </summary>
        /// <returns>A new array containing a snapshot of key and value pairs copied from the
        /// System.Collections.Concurrent.ConcurrentDictionary[TKey,TValue].</returns>
        public KeyValuePair<(TKey1, TKey2, TKey3), TValue>[] ToArray() {
            return _dict.ToArray();
        }

        /// <summary>
        /// Attempts to get the value associated with the specified key from the System.Collections.Concurrent.ConcurrentDictionary[TKey,TValue].
        /// </summary>
        /// <param name="key1">Key 1st part</param>
        /// <param name="key2">Key 2nd part</param>
        /// <param name="key3">Key 3rd part</param>
        /// <param name="value">When this method returns, value contains the object from the
        /// System.Collections.Concurrent.ConcurrentDictionary[TKey,TValue] with the specified key
        /// or the default value of , if the operation failed.</param>
        /// <returns>true if the key was found in the System.Collections.Concurrent.ConcurrentDictionary[TKey,TValue]
        /// otherwise, false.</returns>
        public bool TryGetValue(TKey1 key1, TKey2 key2, TKey3 key3, out TValue value) {
            return _dict.TryGetValue((key1, key2, key3), out value);
        }

        /// <summary>
        /// Add the specified key and value to the System.Collections.Concurrent.ConcurrentDictionary[TKey,TValue].
        /// </summary>
        /// <param name="key1">Key 1st part</param>
        /// <param name="key2">Key 2nd part</param>
        /// <param name="key3">Key 3rd part</param>
        /// <param name="value">the value to be added, if the key does not already exist</param>
        public void Add(TKey1 key1, TKey2 key2, TKey3 key3, TValue value) {
            _dict.Add((key1, key2, key3), value);

        }

        /// <summary>
        /// Remove the value with the specified key from the System.Collections.Concurrent.ConcurrentDictionary[TKey,TValue].
        /// </summary>
        /// <param name="key1">Key 1st part</param>
        /// <param name="key2">Key 2nd part</param>
        /// <param name="key3">Key 3rd part</param>
        public void Remove(TKey1 key1, TKey2 key2, TKey3 key3) {
            _dict.Remove((key1, key2, key3));
        }

        /// <summary>
        /// System.Collections.IEnumerable.GetEnumerator()
        /// </summary>
        /// <returns>Enumerator for the dictionary</returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return ((System.Collections.IEnumerable)_dict).GetEnumerator();
        }
    }
}
