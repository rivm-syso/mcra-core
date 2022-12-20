using System.IO;

namespace MCRA.Utils.DataFileReading {

    /// <summary>
    /// Base class of a data reader. Intended to read from various types
    /// of data sources in a uniform way.
    /// </summary>
    public abstract class DataFileReader : DataSourceReaderBase {

        /// <summary>
        /// Filename/path of the data file.
        /// </summary>
        protected string _filename;

        /// <summary>
        /// Protected constructor.
        /// </summary>
        /// <param name="filename"></param>
        protected DataFileReader(string filename) {
            _filename = filename;
        }

        /// <summary>
        /// The full path of the data file.
        /// </summary>
        public string FullPath {
            get {
                return Path.GetFullPath(_filename);
            }
        }
    }
}
