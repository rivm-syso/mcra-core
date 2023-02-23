using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace MCRA.Utils.ExtensionMethods {
    public static class StringExtensions {

        public static IEnumerable<string> GetRangeStrings(this string source) {
            var r = new Regex(@"\d+(\.\d+)?-\d+(\.\d+)?");
            foreach (Match match in r.Matches(source)) {
                yield return match.Value;
            }
        }

        public static string GetSmallerEqualString(this string source) {
            var r = new Regex(@"(?<![\d\.])-\d+(\.\d+)?");
            return r.Match(source).Value;
        }

        public static string GetGreaterEqualString(this string source) {
            var r = new Regex(@"\d+(\.\d+)?-(?!\d)");
            return r.Match(source).Value;
        }

        public static string CreateFingerprint(string input) {
            // TODO: we could speed up things if we didn't have to make a md5 hasher every time.
            var encoded = new UTF8Encoding().GetBytes(input);
            var hash = MD5.Create().ComputeHash(encoded);
            return BitConverter.ToString(hash).Replace("-", string.Empty);
        }

        /// <summary>
        /// Munges a hash string with another string to create a new Guid (using a MD5
        /// hash of the concatenated strings).
        /// </summary>
        /// <param name="hashString"></param>
        /// <param name="mungeString"></param>
        /// <returns></returns>
        public static string MungeToGuid(this string hashString, string mungeString) {
            using (MD5 md5 = MD5.Create()) {
                byte[] hash = md5.ComputeHash(Encoding.Default.GetBytes(hashString + mungeString));
                var result = new Guid(hash);
                return result.ToString();
            }
        }

        /// <summary>
        /// Creates substring limited to n (length) characters
        /// </summary>
        /// <param name="data"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string LimitTo(this string data, int length) {
            return (data == null || data.Length < length)
              ? data
              : $"{data.Substring(0, length)}...";
        }

        public static string ReplaceCaseInsensitive(this string str, string old, string @new, StringComparison comparison) {
            @new = @new ?? "";
            if (string.IsNullOrEmpty(str) || string.IsNullOrEmpty(old) || old.Equals(@new, comparison)) {
                return str;
            }

            int foundAt = 0;
            while ((foundAt = str.IndexOf(old, foundAt, comparison)) != -1) {
                str = str.Remove(foundAt, old.Length).Insert(foundAt, @new);
                foundAt += @new.Length;
            }
            return str;
        }

        /// <summary>
        /// SplitToInvariantDoubleArray, split a string containing double values with either , or . as decimal
        /// separator to an array of double
        /// </summary>
        /// <param name="str"></param>
        /// <param name="separator"></param>
        /// <returns></returns>
        public static double[] SplitToInvariantDoubleArray(this string str, char separator = ' ') {
            var values = new double[0];
            str = str.Trim();
            if (!string.IsNullOrWhiteSpace(str)) {
                values = str.Replace(',', '.')
                    .Split(separator)
                    .Where(v => !string.IsNullOrEmpty(v))
                    .Select(v => double.Parse(v, NumberFormatInfo.InvariantInfo))
                    .ToArray();
            }
            return values;
        }

        /// <summary>
        /// SplitToIntArray, split a string containing integer values
        /// separator to an array of integer
        /// </summary>
        /// <param name="str"></param>
        /// <param name="separator"></param>
        /// <returns></returns>
        public static int[] SplitToIntArray(this string str, char separator = ' ') {
            var values = new int[0];
            str = str.Trim();
            if (!string.IsNullOrWhiteSpace(str)) {
                values = str.Split(separator)
                    .Where(v => !string.IsNullOrEmpty(v))
                    .Select(v => int.Parse(v))
                    .ToArray();
            }
            return values;
        }

        /// <summary>
        /// Checks whether the string contains the specified substring, allowing for passing specific
        /// comparison options.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="toCheck"></param>
        /// <param name="comp"></param>
        /// <returns></returns>
        public static bool Contains(this string source, string toCheck, StringComparison comp) {
            return source?.IndexOf(toCheck, comp) >= 0;
        }

        /// <summary>
        /// Gets a deterministic Hash for a string, so the same string always
        /// results in the same hash.
        /// .NET 6's string.GetHashCode() doesn't do this any more, and many seed
        /// progressions rely on the fixed hash-per-string, so therefore this extension method
        /// copied from: https://andrewlock.net/why-is-string-gethashcode-different-each-time-i-run-my-program-in-net-core/
        /// </summary>
        /// <param name="str">input string</param>
        /// <returns>checksum: deterministic hash of the string value</returns>
        public static int GetChecksum(this string str) {
            unchecked {
                int hash1 = (5381 << 16) + 5381;
                int hash2 = hash1;

                for (int i = 0; i < str.Length; i += 2) {
                    hash1 = ((hash1 << 5) + hash1) ^ str[i];
                    if (i == str.Length - 1)
                        break;
                    hash2 = ((hash2 << 5) + hash2) ^ str[i + 1];
                }

                return hash1 + (hash2 * 1566083941);
            }
        }
    }
}
