using System.Data;
using System.Reflection;
using MCRA.General;
using MCRA.General.TableDefinitions;
using MCRA.Utils.DataFileReading;

namespace MCRA.Data.Raw.Test.UnitTests.Copying.BulkCopiers {

    public abstract class BulkCopierTestsBase {

        /// <summary>
        /// Delegate function for GetDataReaderByDefinition of mock class.
        /// </summary>
        /// <param name="tableDef">Table definition</param>
        /// <param name="sourceTableName"></param>
        public delegate void GetDataReaderByDefinitionDelegate(
            TableDefinition tableDef,
            out string sourceTableName
        );

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

        protected static List<T> getRawDataRecords<T>(
            DataTable table
        ) where T : IRawDataTableRecord, new() {
            var records = new List<T>();

            if (table == null || table.Rows.Count == 0) {
                return records;
            }

            // Create property lookup
            var props = typeof(T)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanWrite)
                .ToDictionary(p => p.Name.ToLower(), p => p);

            foreach (DataRow row in table.Rows) {
                var obj = new T();
                foreach (DataColumn col in table.Columns) {
                    var colName = col.ColumnName.ToLower();
                    if (props.TryGetValue(colName, out PropertyInfo prop)) {
                        var value = row[col];
                        if (value == DBNull.Value) {
                            continue;
                        }

                        // Handle nullable types
                        var targetType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                        var safeValue = Convert.ChangeType(value, targetType);
                        prop.SetValue(obj, safeValue);
                    }
                }

                records.Add(obj);
            }

            return records;
        }

        protected static DataTable getRawDataSourceTable(
            RawDataSourceTableID tableId,
            Dictionary<string, DataTable> tables
        ) {
            var tableDefinition = McraTableDefinitions.Instance.TableDefinitions[tableId];
            return tables[tableDefinition.TargetDataTable];
        }
    }
}
