using System.Data;

namespace MCRA.Utils.DataSourceReading.DataSourceReaders {

    /// <summary>
    /// Encapsulates an IDataReader and adds a virtual column with a constant value
    /// in the front of the data table.
    /// </summary>
    public sealed class VirtualIntColumnDataReader : VirtualColumnDataReaderBase<int> {

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public VirtualIntColumnDataReader(IDataReader reader, string name, int value)
            : base(reader, name, value) {
        }

        /// <summary>
        /// Gets the string value of the specified field.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public override int GetInt32(int i) {
            if (i == 0) {
                return _virtualColumnValue;
            }
            return _internalReader.GetInt32(i - 1);
        }

        /// <summary>
        /// Return whether the specified field is set to null.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public override bool IsDBNull(int i) {
            if (i == 0) {
                return false;
            }
            return _internalReader.IsDBNull(i - 1);
        }
    }
}
