using MCRA.Data.Raw.Copying.EuHbmDataCopiers;
using MCRA.Data.Raw.Copying.PbkUploadCopiers;
using MCRA.General;
using MCRA.Utils.DataFileReading;
using MCRA.Utils.ExtensionMethods;

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
            if (tableNames?.Count > 0 && isEuHbmDbImportFormat(tableNames)) {
                return [
                    new EuHbmImportDataCopier(targetWriter, parsedTableGroups, parsedDataTables)
                ];
            } else if (dataSourceReader is SbmlDataSourceReader
                || (tableNames?.Count > 0 && isPbkModelUploadFormat(tableNames))
            ) {
                return [
                    new PbkModelUploadCopier(targetWriter, parsedTableGroups, parsedDataTables)
                ];
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
        private static bool isPbkModelUploadFormat(HashSet<string> tableNames) {
            return tableNames.Contains(FieldType.FileReference.GetDisplayName(), StringComparer.OrdinalIgnoreCase);
        }
    }
}
