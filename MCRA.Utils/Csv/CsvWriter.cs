using MCRA.Utils.ExtensionMethods;
using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace MCRA.Utils.Csv {

    /// <summary>
    /// CsvWriter
    /// </summary>
    public sealed class CsvWriter {

        /// <summary>
        /// The writer options.
        /// </summary>
        public CsvWriterOptions Options { get; set; }

        /// <summary>
        /// Creates a new <see cref="CsvWriter"/> instance using
        /// default writer options.
        /// </summary>
        /// <param name="options"></param>
        public CsvWriter() {
            Options = new();
        }

        /// <summary>
        /// Creates a new <see cref="CsvWriter"/> instance using
        /// specified writer options.
        /// </summary>
        /// <param name="options"></param>
        public CsvWriter(CsvWriterOptions options) {
            Options = options;
        }

        /// <summary>
        /// Creates a csv file from the list of table records, generic method
        /// </summary>
        /// <typeparam name="TRecord"></typeparam>
        /// <param name="source"></param>
        /// <param name="fileName"></param>
        /// <param name="writeHeader"></param>
        /// <param name="headerFormatter"></param>
        /// <param name="visibleProperties"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public string WriteToCsvFile<TRecord>(
            IEnumerable<TRecord> source,
            string fileName,
            bool writeHeader = true,
            Func<PropertyInfo, string> headerFormatter = null,
            List<PropertyInfo> visibleProperties = null,
            Encoding encoding = null
        ) {
            if (encoding == null) {
                encoding = Encoding.UTF8;
            }
            using (var stream = new FileStream(fileName, FileMode.Create)) {
                var streamWriter = new StreamWriter(stream, encoding);
                writeCsv(source, typeof(TRecord), streamWriter, writeHeader, headerFormatter, visibleProperties);
            }
            return fileName;
        }

        /// <summary>
        /// Creates a csv file from the list of table records, using a type argument
        /// </summary>
        /// <param name="source"></param>
        /// <param name="recType"></param>
        /// <param name="fileName"></param>
        /// <param name="writeHeader"></param>
        /// <param name="headerFormatter"></param>
        /// <param name="visibleProperties"></param>
        /// <returns></returns>
        public string WriteToCsvFile(
            IEnumerable source,
            Type recType,
            string fileName,
            bool writeHeader = true,
            Func<PropertyInfo, string> headerFormatter = null,
            List<PropertyInfo> visibleProperties = null
        ) {
            using (var stream = new FileStream(fileName, FileMode.Create)) {
                var streamWriter = new StreamWriter(stream, Encoding.Default);
                writeCsv(source, recType, streamWriter, writeHeader, headerFormatter, visibleProperties);
            }
            return fileName;
        }

        /// <summary>
        /// Writes the csv to the stream using type argument
        /// </summary>
        /// <param name="source"></param>
        /// <param name="recordType"></param>
        /// <param name="textWriter"></param>
        /// <param name="writeHeader"></param>
        /// <param name="headerFormatter"></param>
        /// <param name="visibleProperties"></param>
        private void writeCsv(
            IEnumerable source,
            Type recordType,
            TextWriter textWriter,
            bool writeHeader,
            Func<PropertyInfo, string> headerFormatter,
            List<PropertyInfo> visibleProperties
        ) {
            if (writeHeader) {
                writeCsvHeaders(textWriter, recordType, headerFormatter, visibleProperties);
            }
            writeCsvRecords(source, recordType, textWriter, visibleProperties);
            textWriter.Flush();
        }

        /// <summary>
        /// Writes the csv headers.
        /// </summary>
        /// <param name="textWriter"></param>
        /// <param name="recordType"></param>
        /// <param name="headerFormatter"></param>
        /// <param name="visibleProperties"></param>
        private static void writeCsvHeaders(
            TextWriter textWriter,
            Type recordType,
            Func<PropertyInfo, string> headerFormatter,
            IEnumerable<PropertyInfo> visibleProperties
        ) {
            var headerStrings = new List<string>();
            var properties = visibleProperties ?? recordType.GetProperties();
            headerFormatter = headerFormatter ?? ((p) => p.Name);
            foreach (var property in properties) {
                if (property.CanRead) {
                    var displayAttribute = property.GetAttribute<DisplayAttribute>(false);
                    var generateField = displayAttribute?.GetAutoGenerateField();
                    if (generateField ?? true) {
                        var displayName = headerFormatter(property);
                        //remove any line endings, replace with a space
                        displayName = Regex.Replace(displayName, @"\r\n?|\n", " ");
                        headerStrings.Add($"\"{displayName}\"");
                    }
                }
            }
            textWriter.WriteLine(string.Join(",", headerStrings));
        }

        /// <summary>
        /// Writes the csv records.
        /// </summary>
        /// <param name="records"></param>
        /// <param name="recType"></param>
        /// <param name="textWriter"></param>
        /// <param name="visibleProperties"></param>
        private void writeCsvRecords(
            IEnumerable records,
            Type recType,
            TextWriter textWriter,
            List<PropertyInfo> visibleProperties
        ) {
            var properties = visibleProperties ?? recType.GetProperties().ToList();
            var propertiesCount = properties.Count;
            foreach (var tableRow in records) {
                var valueStrings = new List<string>();
                for (int i = 0; i < propertiesCount; ++i) {
                    var property = properties[i];
                    var displayAttribute = property.GetAttribute<DisplayAttribute>(false);
                    var generateField = displayAttribute?.GetAutoGenerateField();
                    if (generateField ?? true) {
                        var formatAttribute = property.GetAttribute<DisplayFormatAttribute>(false);
                        var isNumeric = property.PropertyType == typeof(double)
                            || property.PropertyType == typeof(decimal)
                            || property.PropertyType == typeof(float)
                            || property.PropertyType == typeof(int)
                            || property.PropertyType == typeof(double?)
                            || property.PropertyType == typeof(decimal?)
                            || property.PropertyType == typeof(float?)
                            || property.PropertyType == typeof(int?);
                        var isInteger = property.PropertyType == typeof(int) || property.PropertyType == typeof(int?);
                        var isEnum = property.PropertyType.IsSubclassOf(typeof(Enum));
                        var propertyValue = property.GetValue(tableRow, null);
                        string cellValue;
                        if (propertyValue == null) {
                            cellValue = "";
                        } else if (isNumeric) {
                            if (!isInteger && Options.SignificantDigits > 0) {
                                var rounded = RoundToSignificantDigits(Convert.ToDouble(propertyValue), Options.SignificantDigits);
                                cellValue = string.Format(CultureInfo.InvariantCulture, $"{{0:G{Options.SignificantDigits}}}", rounded);
                                //remove any positive number indicators (e.g. 1.234E+06), they are implied
                                cellValue = cellValue.Replace("+", string.Empty);
                            } else if (formatAttribute != null && formatAttribute.DataFormatString.ToLower().Contains('p')) {
                                cellValue = string.Format(CultureInfo.InvariantCulture, "{0:##0.########}", propertyValue);
                            } else {
                                cellValue = string.Format(CultureInfo.InvariantCulture, "{0}", propertyValue);
                            }
                            //.NET 6 floating point values can result in -0 (minus zero), check for this
                            if (cellValue == "-0") {
                                cellValue = "0";
                            }
                        } else if (isEnum) {
                            if (Options.UseEnumDisplayNames) {
                                cellValue = string.Format(CultureInfo.InvariantCulture, "{0}", ((Enum)propertyValue).GetDisplayName());
                            } else {
                                cellValue = string.Format(CultureInfo.InvariantCulture, "{0}", ((Enum)propertyValue).ToString());
                            }
                        } else if (formatAttribute != null) {
                            cellValue = string.Format(CultureInfo.InvariantCulture, formatAttribute.DataFormatString, propertyValue);
                        } else {
                            cellValue = stringToCSVCell(string.Format(CultureInfo.InvariantCulture, "{0}", propertyValue), true);
                        }
                        // Remove any line endings, replace with a space
                        cellValue = Regex.Replace(cellValue, @"\r\n?|\n", " ");
                        valueStrings.Add(cellValue);
                    }
                }
                textWriter.WriteLine(string.Join(",", valueStrings));
            }
        }

        /// <summary>
        /// Turn a string into a CSV cell output
        /// </summary>
        /// <param name="str">String to output</param>
        /// <param name="useQuotes"></param>
        /// <returns>The CSV cell formatted string</returns>
        private static string stringToCSVCell(string str, bool useQuotes = false) {
            if (useQuotes || str.Contains(",") || str.Contains("\"") || str.Contains("\r") || str.Contains("\n")) {
                var sb = new StringBuilder();
                sb.Append("\"");
                foreach (char nextChar in str) {
                    sb.Append(nextChar);
                    if (nextChar == '"') {
                        sb.Append("\"");
                    }
                }
                sb.Append("\"");
                return sb.ToString();
            }
            return str;
        }

        private static double RoundToSignificantDigits(double d, int digits) {
            if (d == 0) {
                return 0;
            } else if (double.IsInfinity(d) || double.IsNaN(d)) {
                return d;
            }
            double scale = Math.Pow(10, Math.Floor(Math.Log10(Math.Abs(d))) + 1);
            return scale * Math.Round(d / scale, digits);
        }
    }
}
