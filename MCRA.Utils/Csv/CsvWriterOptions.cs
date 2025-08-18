namespace MCRA.Utils.Csv {
    public sealed class CsvWriterOptions {

        /// <summary>
        /// Set the amount of significant digits the CSV writer will use to write
        /// double values to the CSV file output, '0' means same as input, not rounded
        /// </summary>
        public int SignificantDigits { get; set; } = 0;

        /// <summary>
        /// If true, write enums using the display names when available.
        /// </summary>
        public bool UseEnumDisplayNames { get; set; } = true;

    }
}
