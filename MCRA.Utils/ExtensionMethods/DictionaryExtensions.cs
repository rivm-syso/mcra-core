namespace MCRA.Utils.ExtensionMethods {

    public static class DictionaryExtensions {

        /// <summary>
        /// Adds a value to a dictionary of collections.
        /// </summary>
        public static IDictionary<T, TCollection> AddOrAppend<T, U, TCollection>(
            this IDictionary<T, TCollection> dictionary,
            T key,
            U value
        ) where TCollection: ICollection<U>, new() {
            if (!dictionary.TryGetValue(key, out var collection)) {
                collection = new TCollection();
                dictionary[key] = collection;
            }

            collection.Add(value);
            return dictionary;
        }
    }
}
