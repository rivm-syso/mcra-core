using MCRA.Data.Raw;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.General.ModuleDefinitions;
using MCRA.General.ScopingTypeDefinitions;

namespace MCRA.Data.Management.RawDataProviders {
    public class ActionRawDataProvider : IRawDataProvider {

        private readonly ProjectDto _project;
        private readonly IDictionary<SourceTableGroup, List<int>> _linkedDataSources;
        private readonly IRawDataManagerFactory _rawDataManagerFactory;

        public ActionRawDataProvider(
            ProjectDto project,
            IDictionary<SourceTableGroup, List<int>> linkedDataSources,
            IRawDataManagerFactory rawDataManagerFactory
        ) {
            _project = project;
            _linkedDataSources = linkedDataSources;
            _rawDataManagerFactory = rawDataManagerFactory;
        }

        /// <summary>
        /// Gets the raw data source ids for the specified table group.
        /// </summary>
        /// <param name="tableGroup"></param>
        /// <returns></returns>
        public ICollection<int> GetRawDatasourceIds(SourceTableGroup tableGroup) {
            if (_linkedDataSources != null) {
                if (_linkedDataSources.TryGetValue(tableGroup, out var dataSources) && dataSources.Any()) {
                    var actionType = McraModuleDefinitions.Instance.ModuleDefinitionsByTableGroup[tableGroup].ActionType;
                    if (_project.CalculationActionTypes == null
                        || !_project.CalculationActionTypes.Contains(actionType)) {
                        return dataSources.Distinct().ToList();
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Creates a raw data manager for reading from the raw data tables.
        /// </summary>
        /// <returns></returns>
        public IRawDataManager CreateRawDataManager() {
            if (_rawDataManagerFactory == null) {
                return null;
            }
            return _rawDataManagerFactory.CreateRawDataManager();
        }

        /// <summary>
        /// Gets the codes filter of the specified scoping type.
        /// </summary>
        /// <param name="scopingType"></param>
        /// <returns></returns>
        public HashSet<string> GetFilterCodes(ScopingType scopingType) {
            if (McraScopingTypeDefinitions.Instance.ScopingDefinitions.TryGetValue(scopingType, out var definition)
                && McraModuleDefinitions.Instance.ModuleDefinitionsByTableGroup.TryGetValue(definition.TableGroup, out var module)
                && (_project.CalculationActionTypes?.Contains(module.ActionType) ?? false)
            ) {
                return null;
            }
            return _project.GetFilterCodes(scopingType);
        }

        /// <summary>
        /// Specifies whether there is an active keys filter.
        /// </summary>
        /// <param name="scopingType"></param>
        /// <returns></returns>
        public bool HasKeysFilter(ScopingType scopingType) {
            return GetFilterCodes(scopingType)?.Count > 0;
        }
    }
}
