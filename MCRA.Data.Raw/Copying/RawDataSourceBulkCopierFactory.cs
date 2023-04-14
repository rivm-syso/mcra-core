using MCRA.Data.Raw.Copying.EuHbmDataCopiers;
using MCRA.General;
using MCRA.Utils.DataFileReading;

namespace MCRA.Data.Raw.Copying {
    public static class RawDataSourceBulkCopierFactory {

        public static ICollection<RawDataSourceBulkCopierBase> Create(
            IDataSourceReader dataSourceReader,
            IDataSourceWriter targetWriter,
            HashSet<SourceTableGroup> parsedTableGroups,
            HashSet<RawDataSourceTableID> parsedDataTables,
            IEnumerable<SourceTableGroup> tableGroups = null
        ) {
            if (tableGroups == null) {
                tableGroups = Enum.GetValues(typeof(SourceTableGroup))
                    .Cast<SourceTableGroup>()
                    .Where(r => r != SourceTableGroup.Unknown)
                    .ToList();
            }
            var tableNames = dataSourceReader.GetTableNames()?.ToHashSet();
            if (tableNames != null && tableNames.Any() && isEuHbmDbImportFormat(tableNames)) {
                return new List<RawDataSourceBulkCopierBase>() {
                    new EuHbmImportDataCopier(targetWriter, parsedTableGroups, parsedDataTables)
                };
            } else {
                var copiers = tableGroups
                    .Select(tg => createDefaultTableGroupCopier(tg, targetWriter, parsedTableGroups, parsedDataTables))
                    .Where(r => r != null)
                    .ToList();
                return copiers;
            }
        }

        private static RawDataSourceBulkCopierBase createDefaultTableGroupCopier(
            SourceTableGroup tableGroup,
            IDataSourceWriter targetWriter,
            HashSet<SourceTableGroup> parsedTableGroups,
            HashSet<RawDataSourceTableID> parsedDataTables
        ) {
            RawDataSourceBulkCopierBase result;
            var copierType = Type.GetType($"MCRA.Data.Raw.Copying.BulkCopiers.{tableGroup}BulkCopier", false, true);
            if (copierType == null) {
                return null;
            }
            result = (RawDataSourceBulkCopierBase)Activator.CreateInstance(copierType, targetWriter, parsedTableGroups, parsedDataTables);
            return result;
        }

        private static bool isEuHbmDbImportFormat(HashSet<string> tableNames) {
            return tableNames.Contains("STUDYINFO", StringComparer.OrdinalIgnoreCase);
        }
    }
}
