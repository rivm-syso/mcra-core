using MCRA.Utils.ExtensionMethods;

namespace MCRA.Utils.Statistics.RandomGenerators {
    public static class RandomUtils {

        public static bool BackwardComparibilityMode = false;

        /// <summary>
        /// Prime number used for hashing. Using a prime number recommended
        /// for 32bit hashing FNV.
        /// </summary>
        private static readonly int _hashPrime = 16777619;
 
        /// <summary>
        /// Creates a random seed from the provided base seed and the hash
        /// code strings.
        /// </summary>
        /// <param name="seed"></param>
        /// <param name="hashCodes"></param>
        /// <returns></returns>
        public static int CreateSeed(int seed, params int[] hashCodes) {
            if (BackwardComparibilityMode) {
                var result = seed;
                foreach (var code in hashCodes) {
                    result ^= code;
                }
                return result;
            } else {
                unchecked {
                    // Hashing based on FNV using the provided seed as offset.
                    var result = seed;
                    for (int i = 0; i < hashCodes.Length; i++) {
                        result *= _hashPrime;
                        result ^= hashCodes[i];
                    }
                    return result;
                }
            }
        }

        /// <summary>
        /// Creates a random seed from the provided base seed and the hash
        /// code strings.
        /// </summary>
        /// <param name="seed"></param>
        /// <param name="hashCodes"></param>
        /// <returns></returns>
        public static int CreateSeed(int seed, params string[] hashCodes) {
            var result = seed;
            foreach (var code in hashCodes) {
                result ^= code.GetChecksum();
            }
            return result;
        }
    }
}
