using MCRA.Data.Management.RawDataManagers;
using MCRA.Data.Raw;
using MCRA.General;
using MCRA.General.ScopingTypeDefinitions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Data.Management.RawDataProviders {

    /// <summary>
    /// CsvRawDataProvider
    /// </summary>
    public class CsvRawDataProvider : IRawDataProvider {

        private readonly Dictionary<ScopingType, HashSet<string>> _filterCodes;

        private readonly Dictionary<SourceTableGroup, List<int>> _rawDataSourceIds;

        private readonly CsvTableRawDataManager _rawDataManager;

        public CsvRawDataProvider(
            string csvBaseFilePath
        ) {
            _filterCodes = new Dictionary<ScopingType, HashSet<string>>();
            _rawDataSourceIds = new Dictionary<SourceTableGroup, List<int>>();
            _rawDataManager = new CsvTableRawDataManager(csvBaseFilePath);
        }

        public IRawDataManager CreateRawDataManager() {
            return _rawDataManager;
        }

        public ICollection<int> GetRawDatasourceIds(SourceTableGroup tableGroup) {
            if (_rawDataSourceIds.TryGetValue(tableGroup, out var value)) {
                return value;
            }
            return null;
        }

        public void SetDataGroupsFromFolder(int idDataSource, string folder, params SourceTableGroup[] tableGroups) {
            _rawDataManager.SetDataTablesFromFolder(idDataSource, folder, tableGroups);
            SetEmptyDataSource(idDataSource, tableGroups);
        }

        public void SetDataTables(params (ScopingType TableId, string Filename)[] tables) {
            SetDataTables(1, tables);
        }

        public void SetDataTables(int idDataSource, params (ScopingType TableId, string Filename)[] tables) {
            foreach (var table in tables) {
                var scopingDefinition = McraScopingTypeDefinitions.Instance.ScopingDefinitions[table.TableId];
                SetEmptyDataSource(idDataSource, scopingDefinition.TableGroup);
                _rawDataManager.SetDataTable((RawDataSourceTableID)scopingDefinition.RawTableId, table.Filename, idDataSource);
            }
        }

        public void SetEmptyDataSource(params SourceTableGroup[] tableGroups) {
            SetEmptyDataSource(1, tableGroups);
        }

        public void SetEmptyDataSource(int id, params SourceTableGroup[] tableGroups) {
            foreach (var tableGroup in tableGroups) {
                if (!_rawDataSourceIds.TryGetValue(tableGroup, out var dataSources)) {
                    dataSources = new List<int>();
                    _rawDataSourceIds.Add(tableGroup, dataSources);
                }
                if (!dataSources.Contains(id)) {
                    dataSources.Add(id);
                }
            }
        }

        public HashSet<string> GetFilterCodes(ScopingType scopingType) {
            _filterCodes.TryGetValue(scopingType, out HashSet<string> filterCodes);
            return filterCodes;
        }

        public void SetFilterCodes(ScopingType scopingType, IEnumerable<string> codes) {
            _filterCodes[scopingType] = new HashSet<string>(codes, StringComparer.OrdinalIgnoreCase);
        }

        public bool HasKeysFilter(ScopingType scopingType) {
            return GetFilterCodes(scopingType)?.Any() ?? false;
        }
    }
}
