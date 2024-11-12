using System.Data;
using MCRA.Utils.SBML;

namespace MCRA.Utils.DataFileReading {
    /// <summary>
    /// DataSourceReaderFactory for supported file types
    /// </summary>
    public sealed class DataSourceReaderFactory {

        /// <summary>
        /// Returns the appropriate file reader instance for the given filename.
        /// </summary>
        /// <param name="filename">Reference to the raw data source file.</param>
        /// <param name="rawDataTables">Optional: raw data tables to use as raw data source.</param>
        /// <returns></returns>
        public static IDataSourceReader GetDataReader(string filename, params DataTable[] rawDataTables) {
            if (rawDataTables.Length > 0) {
                return new RawDataTableReader(rawDataTables);
            }
            if (!File.Exists(filename)) {
                throw new IOException("File does not exist");
            }
            switch (Path.GetExtension(filename).ToLower()) {
                case ".mdb":
                case ".accdb":
                    if (OperatingSystem.IsWindows()) {
                        return new AccessDataFileReader(filename);
                    } else {
                        throw new Exception("Access data file reader only available on windows.");
                    }
                case ".xls":
                case ".xlsx":
                    return new ExcelFileReader(filename);
                case ".zip":
                    return new ZipCsvFileReader(filename);
                case ".sbml":
                    return new SbmlDataSourceReader(filename);
                default:
                    throw new InvalidDataException("Invalid file format");
            }
        }
    }
}
