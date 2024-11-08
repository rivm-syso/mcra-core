using System.Text;

namespace MCRA.Utils.Collections {

    /// <summary>
    /// Class to hold an array of bits in a sequence of ulong values
    /// this class enables large bit patterns and contains methods to
    /// quickly determine whether bit patterns are 'subsets' in the
    /// sense that a pattern is a subset if all '1' bits are also
    /// '1' in the compared pattern.
    /// </summary>
    public class BitPattern64 {
        private readonly int _length;
        private readonly ulong[] _bins;
        private const int _bitsize = 64;

        /// <summary>
        /// Constructor taking the number of bits that will be addressed
        /// </summary>
        /// <param name="length">Number of bits to create this collection for</param>
        public BitPattern64(int length) {
            //create array of ulongs large enough to hold bits
            _length = length < 0 ? 0 : length;
            _bins = new ulong[(length + _bitsize - 1) / _bitsize];
        }

        /// <summary>
        /// construct a bit pattern from a string with ones and zeroes
        /// (all non-zero characters are set).
        /// </summary>
        /// <param name="binString"></param>
        public BitPattern64(string binString) : this(binString?.Length ?? 0) {
            if (_length > 0) {
                for (int i = 0; i < _length; i++) {
                    if (binString[i] != '0') {
                        Set(i);
                    }
                }
            }
        }

        /// <summary>
        /// Construct a bit pattern from a boolean array
        /// </summary>
        /// <param name="values"></param>
        public BitPattern64(bool[] values) : this(values?.Length ?? 0) {
            if (_length > 0) {
                for (int i = 0; i < _length; i++) {
                    if (values[i]) {
                        Set(i);
                    }
                }
            }
        }

        /// <summary>
        /// Set a bit pattern using the incoming value
        /// </summary>
        /// <param name="values"></param>
        public BitPattern64(ulong[] values) : this(values?.Length ?? 0) {
            //set the value as-is
            if (values != null) {
                for (int i = 0; i < values.Length; i++) {
                    _bins[i] = values[i];
                }
            }
        }

        /// <summary>
        /// Set the bit at position index
        /// </summary>
        /// <param name="index"></param>
        public void Set(int index) {
            if (index < 0) {
                return;
            }

            var valIdx = index / _bitsize;
            if (valIdx < _bins.Length) {
                var bit = 1UL << (index % _bitsize);
                _bins[valIdx] |= bit;
            }
        }

        /// <summary>
        /// Return the boolean value of the bit
        /// at position 'index'
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public bool Get(int index) {
            if (index < 0) {
                return false;
            }

            var valIdx = index / _bitsize;
            if (valIdx < _bins.Length) {
                var bit = 1UL << (index % _bitsize);
                return (_bins[valIdx] & bit) == bit;
            }
            return false;
        }

        /// <summary>
        /// Return an integer enumerable of all the indices
        /// into the full bit array with bits set to 1
        /// </summary>
        public IEnumerable<int> IndicesOfSetBits {
            get {
                for (int i = 0; i < _bins.Length; i++) {
                    for (int j = 0; j < _bitsize; j++) {
                        if ((_bins[i] & (1UL << j)) == (1UL << j)) {
                            yield return (i * _bitsize + j);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Return the total amount of set bits in this instance
        /// </summary>
        public int NumberOfSetBits {
            get {
                return _bins.Sum(i => getNumberOfSetBits(i));
            }
        }

        /// <summary>
        /// returns whether the current bitpattern is a subset
        /// of the input bit pattern, that is at least all set bits
        /// in this pattern should match the set bits in the other
        /// pattern
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool IsSubSetOf(BitPattern64 other) {
            //if (other == null) return false;
            //if (SetBitCount > other.SetBitCount) return false;
            //max count of ints to parse
            if (_length == 0) {
                return false;
            }

            if (_length == other._length) {
                for (int i = 0; i < _bins.Length; i++) {
                    if ((_bins[i] & other._bins[i]) != _bins[i]) {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Return the string representation of this instance
        /// as a string of 0s and 1s
        /// </summary>
        /// <returns></returns>
        public override string ToString() {
            var sb = new StringBuilder(_length);
            for (int i = 0; i < _length; i++) {
                sb.Append(Get(i) ? "1" : "0");
            }
            return sb.ToString();
        }

        /// <summary>
        /// Calculate the number of set bits in an integer
        /// using the
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        private static int getNumberOfSetBits(ulong i) {
            i = i - ((i >> 1) & 0x5555555555555555UL);
            i = (i & 0x3333333333333333UL) + ((i >> 2) & 0x3333333333333333UL);
            i = unchecked(((i + (i >> 4)) & 0xF0F0F0F0F0F0F0FUL) * 0x101010101010101UL) >> 56;
            return (int)i;
        }

        /// <summary>
        /// Check whether 2 instances are equal, compare the bins
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj) {
            if (obj is BitPattern64 other) {
                if (_bins.Length == other._bins.Length) {
                    for (int i = 0; i < _bins.Length; i++) {
                        if (_bins[i] != other._bins[i]) {
                            return false;
                        }
                    }
                    return true;
                }
                return false;
            }
            return false;
        }

        /// <summary>
        /// GetHashCode override
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode() {
            if (_bins.Length == 0) {
                return -1;
            }

            unchecked {
                int hash = 17;
                hash = 31 * hash + _bins[0].GetHashCode();
                hash = 31 * hash + _bins[_bins.Length / 2].GetHashCode();
                hash = 31 * hash + _bins[_bins.Length - 1].GetHashCode();
                hash = 31 * hash + _bins.Length;
                return hash;
            }
        }
    }
}
