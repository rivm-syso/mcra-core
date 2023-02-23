using System.IO.Compression;

namespace MCRA.Utils.DataFileReading {

    /// <summary>
    /// Data source writer that writes the data sources to a zip file
    /// with csvs.
    /// </summary>
    public class ZippedCsvFileDataSourceWriter : CsvFileDataSourceWriter {

        private readonly string _targetFileName;
        private readonly bool _keepTempCsvFolder;

        /// <summary>
        /// Constructor, must provide a target filename and a 
        /// temp folder for writing the csv files.
        /// </summary>
        /// <param name="targetFileName"></param>
        /// <param name="tempFolder"></param>
        /// <param name="keepTempCsvFolder"></param>
        public ZippedCsvFileDataSourceWriter(
            string targetFileName,
            string tempFolder,
            bool keepTempCsvFolder = false
        ) : base(tempFolder) {
            _keepTempCsvFolder = keepTempCsvFolder;
            _targetFileName = targetFileName;
            if (File.Exists(_targetFileName)) {
                File.Delete(_targetFileName);
            }
        }

        /// <summary>
        /// Closes the writer and creates the zip-file from the specified folder.
        /// </summary>
        public override void Close() {
            ZipFile.CreateFromDirectory(_csvDirectory.FullName, _targetFileName, CompressionLevel.Optimal, false);
            if (!_keepTempCsvFolder && _csvDirectory != null && _csvDirectory.Exists) {
                _csvDirectory.Delete(true);
            }
        }
    }
}
