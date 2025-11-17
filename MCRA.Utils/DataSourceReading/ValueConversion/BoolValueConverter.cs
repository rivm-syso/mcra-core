namespace MCRA.Utils.DataSourceReading.ValueConversion {

    /// <summary>
    /// Implements <see cref="IValueConverter"/> for converting string values to
    /// <see cref="bool"/> values.
    /// </summary>
    public class BoolValueConverter : IValueConverter {

        /// <summary>
        /// Converts a <see cref="bool"/> from a <see cref="string"/> value, which
        /// may be numeric (1 or 0 or "1" or "0"), or "True" or "False", to the
        /// boolean type.
        /// </summary>
        public object Convert(string stringVal) {
            if (bool.TryParse(stringVal, out var boolVal)) {
                return boolVal;
            }
            if (int.TryParse(stringVal, out var intVal)) {
                return intVal != 0;
            }
            //string comparison if all of the above fails
            var boolStr = stringVal.ToLower();
            //we allow y/n, yes/no, t/f, true/false (case insensitive) here for now.
            if (boolStr == "n" || boolStr == "no" || boolStr == "f") {
                return false;
            }
            return boolStr == "y" || boolStr == "yes" || boolStr == "t"
                ? true
                : throw new FormatException($"Failed to parse '{stringVal}' as a boolean value.");
        }
    }
}
