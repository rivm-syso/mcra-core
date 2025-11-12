using System.Data;

namespace MCRA.Utils.DataSourceReading.DataSourceReaders {

    /// <summary>
    /// Encapsulates an IDataReader and adds a virtual column with a constant value
    /// in the front of the data table.
    /// </summary>
    public sealed class VirtualStringColumnDataReader : VirtualColumnDataReaderBase<string> {

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public VirtualStringColumnDataReader(IDataReader reader, string name, string value)
            : base(reader, name, value) {
        }

        /// <summary>
        /// Gets the string value of the specified field.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public override string GetString(int i) {
            return i == 0
                ? _virtualColumnValue
                : base.GetString(i);
        }

        /// <summary>
        /// Return whether the specified field is set to null.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public override bool IsDBNull(int i) {
            return i == 0
                ? string.IsNullOrEmpty(_virtualColumnValue)
                : base.IsDBNull(i);
        }
    }
}
