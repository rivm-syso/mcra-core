using System.Text;

namespace MCRA.Utils.TestReporting {
    /// <summary>
    /// Utility functions for unit testing.
    /// </summary>
    public static class TestUtils {

        private static readonly Random m_getrandom = new();
        private static readonly object m_syncLock = new();



        private class StateData {
            public int InstanceNumber { get; set; }
            public AutoResetEvent ResetEvent { get; set; }
        };



        /// <summary>
        /// Generates a random number.
        /// </summary>
        /// <param name="min">Specify the lower bound of the range in which the random number will be generated.</param>
        /// <param name="max">Specify the upper bound of the range in which the random number will be generated.</param>
        /// <param name="excludedNumbers">A optional list of integers which should not be returned from this function.</param>
        /// <returns>A random integer.</returns>
        public static int GetRandomNumber(int min = System.Int32.MinValue, int max = System.Int32.MaxValue, List<int> excludedNumbers = null) {
            lock (m_syncLock) {
                int number;
                do {
                    number = m_getrandom.Next(min, max);
                } while (excludedNumbers != null && excludedNumbers.Contains(number));
                return number;
            }
        }

        /// <summary>
        /// Generates a random double.
        /// </summary>
        /// <returns>A random double.</returns>
        public static double GetRandomDouble(double min = System.Double.MinValue, double max = System.Double.MaxValue) {
            double scalingFactor = max - min;

            double rd;
            lock (m_syncLock) {
                rd = m_getrandom.NextDouble() * scalingFactor + min;
            }

            return rd;
        }

        /// <summary>
        /// Generates a random ANSI string of a specified length.
        /// </summary>
        /// <param name="length">The length of the string.</param>
        /// <returns>A random ANSI string e.g. "¤"`+`¥®qQN"!x:6UEohXoG_Y~G|"G}.c4rGnL*oBC{".</returns>
        public static string GetRandomAnsiString(int length = 256) {
            StringBuilder builder = new StringBuilder();
            char ch;
            for (int i = 0; i < length; i++) {
                // Generate a character between ASCII code 32 and 175.
                ch = Convert.ToChar(Convert.ToInt32(System.Math.Floor(143 * m_getrandom.NextDouble() + 32)));
                builder.Append(ch);
            }

            return builder.ToString();
        }

        /// <summary>
        /// Generates a random string containing only numbers and alphabetical characters.
        /// </summary>
        /// <param name="length">The length of the string.</param>
        /// <returns>A random alphanumerical string e.g. "U3tHrA32MotMVO0hqa4Nl65cV".</returns>
        public static string GetRandomAlphaNumericalString(int length = 256) {
            StringBuilder builder = new StringBuilder();
            char ch;
            for (int i = 0; i < length; i++) {
                int select = m_getrandom.Next(0, 3);
                switch (select) {
                    case 0: ch = Convert.ToChar(Convert.ToInt32(System.Math.Floor(10 * m_getrandom.NextDouble() + 48))); break;
                    case 1: ch = Convert.ToChar(Convert.ToInt32(System.Math.Floor(25 * m_getrandom.NextDouble() + 65))); break;
                    case 2: ch = Convert.ToChar(Convert.ToInt32(System.Math.Floor(25 * m_getrandom.NextDouble() + 97))); break;
                    default: ch = 'x'; break;
                }

                builder.Append(ch);
            }

            return builder.ToString();
        }

        /// <summary>
        /// Generates a random Unicode string of a specified length.
        /// </summary>
        /// <param name="length">The length of the string.</param>
        /// <returns>A random ANSI string e.g. "¤"`+`¥®qQN"!x:6UEohXoG_Y~G|"G}.c4rGnL*oBC{".</returns>
        public static string GetRandomString(int length = 256) {
            byte[] randomBytes = GetandomBytes(length);
            return Convert.ToBase64String(randomBytes);
        }

        /// <summary>
        /// Returns a random Enum value of a specified type.
        /// </summary>
        /// <typeparam name="T">The enum type</typeparam>
        /// <returns>A random Enum value of the specified type</returns>
        public static T GetRandomEnum<T>() {
            if (!typeof(T).IsEnum) {
                return default(T);
            }
            System.Array enumValues = System.Enum.GetValues(typeof(T));
            return (T)enumValues.GetValue(GetRandomNumber(0, enumValues.Length - 1));
        }

        /// <summary>
        /// Generates a random byte array of a specified size.
        /// </summary>
        /// <param name="size">The required size of the byte array. When no size is specified a byte array of 255 bytes is generated.</param>
        /// <returns>A byte array filled with random bytes e.g. 177 209 137 61 204 127 103.</returns>
        public static byte[] GetandomBytes(int size = 255) {
            byte[] bytes = new byte[size];
            m_getrandom.NextBytes(bytes);

            return bytes;
        }
    }
}
