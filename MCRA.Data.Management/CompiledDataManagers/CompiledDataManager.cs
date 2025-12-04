using System.Data;
using MCRA.Data.Compiled;
using MCRA.Data.Raw;
using MCRA.Utils.DataFileReading;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Data.Management.CompiledDataManagers {

    /// <summary>
    /// CompiledDataManager provides all raw data as 'compiled' data to the upper layers of the system
    /// This is the main class' source code file, the other files that constitute partial class are
    /// located in the 'CompiledDataManagerParts' subfolder, one file per table group
    /// </summary>
    public partial class CompiledDataManager : CompiledLinkManager, ICompiledDataManager {

        private const string _sep = "\a";

        private readonly CompiledData _data;

        /// <summary>
        /// Instantiate with a raw data provider.
        /// </summary>
        /// <param name="rawDataProvider"></param>
        public CompiledDataManager(IRawDataProvider rawDataProvider, IEnumerable<string> skipScopingTypes = null) : base(rawDataProvider, skipScopingTypes) {
            _data = new CompiledData();
        }

        private static void writeToCsv(string tempCsvFolder, TableDefinition tableDef, DataTable table, int[] columnValueCounts = null) {
            //don't write empty tables here
            if (table.Rows.Count == 0) {
                return;
            }

            //remove empty columns from DataTable, that is columns with a zero count
            if (columnValueCounts != null) {
                for (int i = columnValueCounts.Length - 1; i >= 0; i--) {
                    if (columnValueCounts[i] == 0) {
                        table.Columns.RemoveAt(i);
                    }
                }
            }

            var fileName = Path.Combine(tempCsvFolder, $"{tableDef.Id}.csv");
            table.ToCsv(fileName);
        }
    }
}
