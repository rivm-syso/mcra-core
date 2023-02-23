using MCRA.Utils.DataFileReading;
using MCRA.General;
using MCRA.General.TableDefinitions;
using System.Data;

namespace MCRA.Data.Raw.Test.UnitTests.Copying.BulkCopiers {

    public abstract class BulkCopierTestsBase {

        /// <summary>
        /// Delegate function for GetDataReaderByDefinition of mock class.
        /// </summary>
        /// <param name="tableDef">Table definition</param>
        /// <param name="sourceTableName"></param>
        public delegate void GetDataReaderByDefinitionDelegate(TableDefinition tableDef, out string sourceTableName);

        protected static TableDefinition getTableDefinition(RawDataSourceTableID tableId) {
            return McraTableDefinitions.Instance.GetTableDefinition(tableId);
        }

        protected static List<T> getDistinctColumnValues<T>(
            DataTable table,
            string fieldName
        ) {
            var result = getColumnValues<T>(table, fieldName)
                .Distinct()
                .ToList();
            return result;
        }

        protected static List<T> getColumnValues<T>(
            DataTable table,
            string fieldName
        ) {
            var result = table.Rows
                .OfType<DataRow>()
                .Select(r => !r.IsNull(fieldName) ? r[fieldName] : default(T))
                .Cast<T>()
                .ToList();
            return result;
        }

        protected static DataTable getRawDataSourceTable(RawDataSourceTableID tableId, Dictionary<string, DataTable> tables) {
            var tableDefinition = McraTableDefinitions.Instance.TableDefinitions[tableId];
            return tables[tableDefinition.TargetDataTable];
        }
    }
}
