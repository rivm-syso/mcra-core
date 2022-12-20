using System;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Utils.ExtensionMethods {

    public sealed class KeyEqualityComparer<T> : IEqualityComparer<T> {

        private readonly Func<T, T, bool> _comparer;

        private readonly Func<T, object> _keyExtractor;

        // Allows us to simply specify the key to compare with: y => y.CustomerID
        public KeyEqualityComparer(Func<T, object> keyExtractor) : this(keyExtractor, null) { }

        // Allows us to tell if two objects are equal: (x, y) => y.CustomerID == x.CustomerID
        public KeyEqualityComparer(Func<T, T, bool> comparer) : this(null, comparer) { }

        public KeyEqualityComparer(Func<T, object> keyExtractor, Func<T, T, bool> comparer) {
            _keyExtractor = keyExtractor;
            _comparer = comparer;
        }

        public bool Equals(T x, T y) {
            if (_comparer != null) {
                return _comparer(x, y);
            } else {
                var valX = _keyExtractor(x);
                // The special case where we pass a list of keys
                if (valX is IEnumerable<object>) {
                    return ((IEnumerable<object>)valX).SequenceEqual((IEnumerable<object>)_keyExtractor(y));
                }
                return valX.Equals(_keyExtractor(y));
            }
        }

        public int GetHashCode(T obj) {
            if (_keyExtractor == null) {
                return obj.ToString().ToLower().GetHashCode();
            } else {
                var val = _keyExtractor(obj);
                // The special case where we pass a list of keys
                if (val is IEnumerable<object>) {
                    return (int)((IEnumerable<object>)val).Aggregate((x, y) => x.GetHashCode() ^ y.GetHashCode());
                }
                return val.GetHashCode();
            }
        }
    }
}
