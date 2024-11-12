using System.Data;
using MCRA.Utils.DataFileReading;

namespace MCRA.Utils.DataFileReading {
    public class SbmlDataSourceReader : DataFileReader {

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="filename"></param>
        public SbmlDataSourceReader(string filename)
            : base(filename) {
        }

        public override void Close() {
            // Do nothing
        }

        public override IDataReader GetDataReaderByDefinition(TableDefinition tableDefinition, out string sourceTableName) {
            sourceTableName = null;
            return null;
        }

        public override IDataReader GetDataReaderByName(string sheetName, TableDefinition tableDefinition) {
            throw new NotImplementedException();
        }

        public override List<string> GetTableNames() {
            return [];
        }

        public override void Open() {
            // Do nothing
        }

        public override void ValidateSourceTableColumns(ICollection<ColumnDefinition> columnDefinitions, IDataReader sourceTableReader) {
            throw new NotImplementedException();
        }

        public string GetFileReference() {
            return _filename;
        }
    }
}
