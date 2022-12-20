using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace MCRA.Utils.Collections {
    /// <summary>
    /// TwoKeyConcurrentDictionary: Concurrent Dictionary with composite key consisting of 2 values
    /// </summary>
    /// <typeparam name="TKey1">Type of 1st part of key</typeparam>
    /// <typeparam name="TKey2">Type of 2nd part of key</typeparam>
    /// <typeparam name="TValue">Type of the value to store in the dictionary</typeparam>
    public class TwoKeyConcurrentDictionary<TKey1, TKey2, TValue> : IEnumerable<KeyValuePair<(TKey1, TKey2), TValue>> {

        private readonly ConcurrentDictionary<(TKey1, TKey2), TValue> _dict;

        /// <summary>
        /// Initializes a new instance of the System.Collections.Concurrent.ConcurrentDictionary[TKey,TValue]
        /// class that is empty, has the default concurrency level, has the default initial
        /// capacity, and uses the default comparer for the key type.
        /// </summary>
        public TwoKeyConcurrentDictionary() {
            _dict = new ConcurrentDictionary<(TKey1, TKey2), TValue>();
        }

        /// <summary>
        /// Initializes the two key concurrent dictionary with the specified capacity and concurrency level.
        /// </summary>
        /// <param name="concurrencyLevel"></param>
        /// <param name="capacity"></param>
        public TwoKeyConcurrentDictionary(int concurrencyLevel, int capacity) {
            _dict = new ConcurrentDictionary<(TKey1, TKey2), TValue>(concurrencyLevel, capacity);
        }

        /// <summary>
        /// Gets the number of key/value pairs contained in the System.Collections.Concurrent.ConcurrentDictionary[TKey,TValue].
        /// </summary>
        public int Count => _dict.Count;

        /// <summary>
        ///  Gets a value that indicates whether the System.Collections.Concurrent.ConcurrentDictionary[TKey,TValue]
        ///  is empty.
        /// </summary>
        public bool IsEmpty => _dict.IsEmpty;

        /// <summary>
        /// Gets a collection containing the keys in the System.Collections.Generic.Dictionary[TKey,TValue].
        /// </summary>
        public ICollection<(TKey1, TKey2)> Keys => _dict.Keys;

        /// <summary>
        /// Gets a collection containing the values in the System.Collections.Generic.Dictionary[TKey,TValue].
        /// </summary>
        public ICollection<TValue> Values => _dict.Values;

        /// <summary>
        /// Gets or sets the value associated with the specified composite key.
        /// </summary>
        /// <param name="key1">Key 1st part</param>
        /// <param name="key2">Key 2nd part</param>
        /// <returns></returns>
        public TValue this[TKey1 key1, TKey2 key2] {
            get {
                return _dict[(key1, key2)];
            }
            set {
                _dict[(key1, key2)] = value;
            }
        }

        /// <summary>
        /// Adds a key/value pair to the System.Collections.Concurrent.ConcurrentDictionary[TKey,TValue]
        /// if the key does not already exist, or updates a key/value pair in the
        /// System.Collections.Concurrent.ConcurrentDictionary[TKey,TValue] if the key already exists.
        /// </summary>
        /// <param name="key1">Key 1st part</param>
        /// <param name="key2">Key 2nd part</param>
        /// <param name="addValueFactory">The function used to generate a value for an absent key</param>
        /// <param name="updateValueFactory">The function used to generate a new value for an
        /// existing key based on the key's existing value</param>
        /// <returns>The new value for the key. This will be either be the result of addValueFactory
        /// (if the key was absent) or the result of updateValueFactory (if the key was present).</returns>
        public TValue AddOrUpdate(TKey1 key1, TKey2 key2, Func<TKey1, TKey2, TValue> addValueFactory, Func<TKey1, TKey2, TValue, TValue> updateValueFactory) {
            var addFactory = new Func<(TKey1, TKey2), TValue>(t => addValueFactory(t.Item1, t.Item2));
            var updateFactory = new Func<(TKey1, TKey2), TValue, TValue>((t, v) => updateValueFactory(t.Item1, t.Item2, v));
            return _dict.AddOrUpdate((key1, key2), addFactory, updateFactory);
        }

        /// <summary>
        /// Adds a key/value pair to the System.Collections.Concurrent.ConcurrentDictionary[TKey,TValue]
        /// if the key does not already exist, or updates a key/value pair in the
        /// System.Collections.Concurrent.ConcurrentDictionary[TKey,TValue] if the key already exists.
        /// </summary>
        /// <param name="key1">Key 1st part</param>
        /// <param name="key2">Key 2nd part</param>
        /// <param name="addValue">The value to be added for an absent key</param>
        /// <param name="updateValueFactory">The function used to generate a new value for an
        /// existing key based on the key's existing value</param>
        /// <returns>The new value for the key. This will be either be the result of addValueFactory
        /// (if the key was absent) or the result of updateValueFactory (if the key was present).</returns>
        public TValue AddOrUpdate(TKey1 key1, TKey2 key2, TValue addValue, Func<TKey1, TKey2, TValue, TValue> updateValueFactory) {
            var updateFactory = new Func<(TKey1, TKey2), TValue, TValue>((t, v) => updateValueFactory(t.Item1, t.Item2, v));
            return _dict.AddOrUpdate((key1, key2), addValue, updateFactory);
        }

        /// <summary>
        /// Removes all keys and values from the System.Collections.Concurrent.ConcurrentDictionary[TKey,TValue].
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
        /// <returns> true if the System.Collections.Concurrent.ConcurrentDictionary[TKey,TValue]
        /// contains an element with the specified key; otherwise, false.</returns>
        public bool ContainsKey(TKey1 key1, TKey2 key2) {
            return _dict.ContainsKey((key1, key2));
        }

        /// <summary>
        /// Returns an enumerator that iterates through the System.Collections.Concurrent.ConcurrentDictionary[TKey,TValue].
        /// </summary>
        /// <returns>An enumerator for the System.Collections.Concurrent.ConcurrentDictionary[TKey,TValue].</returns>
        public IEnumerator<KeyValuePair<(TKey1, TKey2), TValue>> GetEnumerator() {
            return _dict.GetEnumerator();
        }

        /// <summary>
        /// Adds a key/value pair to the System.Collections.Concurrent.ConcurrentDictionary[TKey,TValue] if the key does not already exist.
        /// </summary>
        /// <param name="key1">Key 1st part</param>
        /// <param name="key2">Key 2nd part</param>
        /// <param name="valueFactory">The function used to generate a value for the key</param>
        /// <returns>The value for the key. This will be either the existing value for the key
        /// if the key is already in the dictionary, or the new value for the key as returned
        /// by valueFactory if the key was not in the dictionary.</returns>
        public TValue GetOrAdd(TKey1 key1, TKey2 key2, Func<TKey1, TKey2, TValue> valueFactory) {
            var vFactory = new Func<(TKey1, TKey2), TValue>(t => valueFactory(t.Item1, t.Item2));
            return _dict.GetOrAdd((key1, key2), vFactory);
        }

        /// <summary>
        /// Adds a key/value pair to the System.Collections.Concurrent.ConcurrentDictionary[TKey,TValue]
        /// if the key does not already exist.
        /// </summary>
        /// <param name="key1">Key 1st part</param>
        /// <param name="key2">Key 2nd part</param>
        /// <param name="value">the value to be added, if the key does not already exist</param>
        /// <returns></returns>
        public TValue GetOrAdd(TKey1 key1, TKey2 key2, TValue value) {
            return _dict.GetOrAdd((key1, key2), value);
        }

        /// <summary>
        /// Copies the key and value pairs stored in the System.Collections.Concurrent.ConcurrentDictionary[TKey,TValue] to a new array.
        /// </summary>
        /// <returns>A new array containing a snapshot of key and value pairs copied from the
        /// System.Collections.Concurrent.ConcurrentDictionary[TKey,TValue].</returns>
        public KeyValuePair<(TKey1, TKey2), TValue>[] ToArray() {
            return _dict.ToArray();
        }

        /// <summary>
        /// Attempts to add the specified key and value to the System.Collections.Concurrent.ConcurrentDictionary[TKey,TValue].
        /// </summary>
        /// <param name="key1">Key 1st part</param>
        /// <param name="key2">Key 2nd part</param>
        /// <param name="value">the value to be added, if the key does not already exist</param>
        /// <returns>true if the key/value pair was added to the
        /// System.Collections.Concurrent.ConcurrentDictionary[TKey,TValue] successfully; otherwise, false.</returns>
        public bool TryAdd(TKey1 key1, TKey2 key2, TValue value) {
            return _dict.TryAdd((key1, key2), value);
        }

        /// <summary>
        /// Attempts to get the value associated with the specified key from the System.Collections.Concurrent.ConcurrentDictionary[TKey,TValue].
        /// </summary>
        /// <param name="key1">Key 1st part</param>
        /// <param name="key2">Key 2nd part</param>
        /// <param name="value">When this method returns, value contains the object from the
        /// System.Collections.Concurrent.ConcurrentDictionary[TKey,TValue] with the specified key
        /// or the default value of , if the operation failed.</param>
        /// <returns>true if the key was found in the System.Collections.Concurrent.ConcurrentDictionary[TKey,TValue] otherwise, false.</returns>
        public bool TryGetValue(TKey1 key1, TKey2 key2, out TValue value) {
            return _dict.TryGetValue((key1, key2), out value);
        }

        /// <summary>
        /// Attempts to remove and return the the value with the specified key from the System.Collections.Concurrent.ConcurrentDictionary[TKey,TValue].
        /// </summary>
        /// <param name="key1">Key 1st part</param>
        /// <param name="key2">Key 2nd part</param>
        /// <param name="value">When this method returns, value contains the object removed from the
        /// System.Collections.Concurrent.ConcurrentDictionary[TKey,TValue]
        /// or the default value of if the operation failed.</param>
        /// <returns>true if an object was removed successfully; otherwise, false.</returns>
        public bool TryRemove(TKey1 key1, TKey2 key2, out TValue value) {
            return _dict.TryRemove((key1, key2), out value);
        }

        /// <summary>
        /// Compares the existing value for the specified key with a specified value, and if they’re equal, updates the key with a third value.
        /// </summary>
        /// <param name="key1">Key 1st part</param>
        /// <param name="key2">Key 2nd part</param>
        /// <param name="newValue">The value that replaces the value of the element with key if the comparison results in equality.</param>
        /// <param name="comparisonValue">The value that is compared to the value of the element with key.</param>
        /// <returns>true if the value with key was equal to comparisonValue and replaced with newValue; otherwise, false.</returns>
        public bool TryUpdate(TKey1 key1, TKey2 key2, TValue newValue, TValue comparisonValue) {
            return _dict.TryUpdate((key1, key2), newValue, comparisonValue);
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
