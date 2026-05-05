using System.Globalization;

namespace MCRA.Utils.ExtensionMethods {
    public static class FormatExtensions {

        /// <summary>
        /// 0              -> 0.00
        /// 1.234E-07      -> 1.23E-07
        /// 0.0000456      -> 4.56E-05
        /// 0.00537       -> 0.00537
        /// 0.0051         -> 0.00510
        /// 1.2345         -> 1.23
        /// 9.9999         -> 10.0
        /// 12.3           -> 12
        /// 1234567        -> 1.23E+06
        /// NaN            -> NaN
        /// ∞              -> ∞
        /// </summary>
        public static string FormatAdaptive(
            this double? value,
            int significantDigits = 3,
            double smallThreshold = 0.0001,
            double largeThreshold = 10,
            int scientificLowerExp = -4,
            int scientificUpperExp = 6,
            IFormatProvider culture = null
        ) {
            if (value is null) {
                return string.Empty;
            }
            return value.Value.FormatAdaptive(significantDigits, smallThreshold, largeThreshold, scientificLowerExp, scientificUpperExp, culture);
        }

        public static string FormatAdaptive(
            this double value,
            int significantDigits = 3,
            double smallThreshold = 0.0001,
            double largeThreshold = 10,
            int scientificLowerExp = -4,
            int scientificUpperExp = 6,
            IFormatProvider culture = null
        ) {
            culture ??= CultureInfo.InvariantCulture;

            // Handle special cases explicitly
            if (double.IsNaN(value)) {
                return "NaN";
            }
            if (double.IsPositiveInfinity(value)) {
                return "∞";
            }
            if (double.IsNegativeInfinity(value)) {
                return "-∞";
            }
            if (value == 0) {
                return 0.0.ToString($"F{significantDigits - 1}", culture);
            }

            double absValue = Math.Abs(value);
            int exponent = (int)Math.Floor(Math.Log10(absValue));

            // Scientific notation for extreme magnitudes
            if (exponent <= scientificLowerExp || exponent >= scientificUpperExp) {
                return value.ToString($"E{significantDigits - 1}", culture);
            }

            if (absValue <= smallThreshold) {
                return value.ToString($"G{significantDigits}", culture);
            }

            if (absValue <= largeThreshold) {
                return ToSignificantDigits(value, significantDigits, culture);
            }

            return value.ToString("N0", culture);
        }

        private static string ToSignificantDigits(
            double value,
            int significantDigits,
            IFormatProvider culture
        ) {
            double absValue = Math.Abs(value);
            int magnitude = (int)Math.Floor(Math.Log10(absValue));

            int decimals = significantDigits - magnitude - 1;
            decimals = Math.Max(0, decimals);

            return value.ToString($"F{decimals}", culture);
        }
    }
}
