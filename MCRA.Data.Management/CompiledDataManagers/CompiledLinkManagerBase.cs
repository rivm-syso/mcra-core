using MCRA.Data.Raw;
using MCRA.General;
using MCRA.General.ScopingTypeDefinitions;
using System.Data;

namespace MCRA.Data.Management.CompiledDataManagers {

    public class CompiledLinkManagerBase {

        protected readonly IRawDataProvider _rawDataProvider;
        protected readonly CompiledDataReportBuilder _reportBuilder;

        public HashSet<ScopingType> _loadedScopingTypes = [];

        /// <summary>
        /// Instantiate with a rawdatamanager and rawdatasource ids per table group
        /// </summary>
        /// <param name="rawDataProvider"></param>
        public CompiledLinkManagerBase(
            IRawDataProvider rawDataProvider,
            IEnumerable<string> skipScopingTypes = null
        ) {
            _rawDataProvider = rawDataProvider;
            _reportBuilder = new CompiledDataReportBuilder();

            if (skipScopingTypes != null) {
                foreach (var skip in skipScopingTypes) {
                    if (Enum.TryParse<ScopingType>(skip, true, out var skipType)) {
                        //add to loaded types and create an empty reading report
                        _loadedScopingTypes.Add(skipType);
                        _reportBuilder.CreateDataReadingReport(skipType);
                    }
                }
            }
        }

        /// <summary>
        /// Loads the scope for the specified table group.
        /// </summary>
        /// <param name="tableGroup"></param>
        public void LoadScope(SourceTableGroup tableGroup) {
            var scopeTypes = McraScopingTypeDefinitions.Instance.TableGroupScopingTypesLookup[tableGroup];
            using (var rdm = _rawDataProvider.CreateRawDataManager()) {
                var rawDataSourceIds = _rawDataProvider.GetRawDatasourceIds(tableGroup);
                if (rawDataSourceIds?.Count > 0) {
                    foreach (var scopingType in scopeTypes) {
                        loadScopingType(scopingType.Id);
                    }
                }
            }
        }

        /// <summary>
        /// Loads the data of and associated with the specified scoping type.
        /// </summary>
        /// <param name="scopingType"></param>
        protected void loadScopingType(ScopingType scopingType) {
            if (!_loadedScopingTypes.Contains(scopingType)) {
                var definition = McraScopingTypeDefinitions.Instance.ScopingDefinitions[scopingType];

                // Load parent scoping types
                McraScopingTypeDefinitions.Instance.ParentScopingTypesLookup
                    .TryGetValue(scopingType, out var parentScopeReferences);
                if (parentScopeReferences?.Count > 0) {
                    foreach (var parentReference in parentScopeReferences) {
                        loadScopingType(parentReference.TargetScopingType);
                    }
                }

                // Load scoping types
                if (!_loadedScopingTypes.Contains(scopingType)) {
                    using (var rdm = _rawDataProvider.CreateRawDataManager()) {
                        var rawDataSourceIds = _rawDataProvider.GetRawDatasourceIds(definition.TableGroup);
                        readSourceEntities(
                            rdm,
                            rawDataSourceIds,
                            scopingType,
                            definition.RawTableId ?? RawDataSourceTableID.Unknown,
                            parentScopeReferences?.ToArray() ?? []
                        );

                        if (scopingType == ScopingType.KineticModelDefinitions) {
                            var readingReport = _reportBuilder.GetDataReadingReport(scopingType);
                            var codes = McraEmbeddedPbkModelDefinitions.Definitions
                                .Select(r => r.Value.Id)
                                .ToHashSet(StringComparer.OrdinalIgnoreCase);
                            codes.UnionWith(McraEmbeddedPbkModelDefinitions.Definitions
                                .Where(r => r.Value.Aliases != null)
                                .SelectMany(r => r.Value.Aliases));
                            var selectedCodes = readingReport.ReadingSummary?.CodesInSelection ?? [];
                            foreach (var code in codes) {
                                readingReport.ReadingSummary.AddCodeInSource(code, null, -1);
                                if (selectedCodes.Count == 0 || selectedCodes.Contains(code)) {
                                    readingReport.ReadingSummary.AddCodeInScope(code);
                                }
                            }
                        }
                    }
                    _loadedScopingTypes.Add(scopingType);
                }

                // Load child scoping types
                if (!_rawDataProvider.HasKeysFilter(scopingType)
                    && definition.IsAutoScope
                    && McraScopingTypeDefinitions.Instance.ChildScopingTypesLookup.TryGetValue(scopingType, out var childScopingReferences)) {
                    foreach (var scopeReference in childScopingReferences) {
                        loadScopingType(scopeReference.SourceScopingType);
                    }
                }
            }
        }

        private void readSourceEntities(
            IRawDataManager dataManager,
            ICollection<int> dataSourceIds,
            ScopingType scopingType,
            RawDataSourceTableID readingTable,
            params ScopeReference[] parentScopeReferences
        ) {
            // Get the reading report
            var definition = McraScopingTypeDefinitions.Instance.ScopingDefinitions[scopingType];
            var readingReport = _reportBuilder.CreateDataReadingReport(scopingType);
            if (definition.IsStrongEntity) {
                readingReport.ReadingSummary = new DataReadingSummaryRecord(
                    definition,
                    _rawDataProvider.GetFilterCodes(scopingType)
                );
            }
            var readingSummary = readingReport.ReadingSummary;

            if (dataSourceIds?.Count > 0) {

                // Parent entity definitions
                var parentTables = parentScopeReferences.Select(t => (t.TargetTable, t.FieldId)).ToArray();

                // Parent reading reports
                var parentReadingReports = parentScopeReferences.ToDictionary(
                    r => r,
                    r => _reportBuilder.DataReadingReports[r.TargetScopingType].ReadingSummary
                );

                // Linking reports
                var linkingReports = parentScopeReferences.ToDictionary(
                    r => r,
                    r => _reportBuilder.CreateDataLinkingReport(r)
                );

                // Key check flags
                bool linkCheck, hasId;

                // Collection of parent reference keys that should be auto-scoped
                var autoScopeKeys = new List<(ScopeReference ParentReference, string Key)>();

                // Iterate over data sources
                foreach (var idDataSource in dataSourceIds) {
                    using (var reader = dataManager.OpenKeysReader(idDataSource, readingTable, parentTables)) {
                        if (reader != null) {
                            while (reader.Read()) {

                                // Read the id and name, and add code in source to reading report
                                var id = !reader.IsDBNull(0) ? reader.GetString(0) : null;
                                hasId = !string.IsNullOrEmpty(id);
                                if (hasId) {
                                    var name = !reader.IsDBNull(1) ? reader.GetString(1) : null;
                                    readingSummary?.AddCodeInSource(id, name, idDataSource);
                                }

                                // Collection of parent reference keys that should be auto-scoped
                                autoScopeKeys.Clear();

                                // Checks whether the parent scopes do not constrain the entity
                                linkCheck = true;

                                // Iterate over parent scopes
                                for (int i = 0; i < parentScopeReferences.Length; i++) {

                                    // Get parent reference field value
                                    var fieldValue = !reader.IsDBNull(i + 2) ? reader.GetString(i + 2) : null;

                                    // Skip empty parent reference field
                                    if (string.IsNullOrEmpty(fieldValue)) {
                                        continue;
                                    }

                                    // If the parent scope is empty we don't need to check; this means there is no restriction
                                    var parentScopeReference = parentScopeReferences[i];
                                    if (parentScopeReference.IsKeysList) {
                                        var parentIds = fieldValue.Split(parentScopeReference.ListSeparator)
                                            .Select(s => s.Trim()).ToArray();
                                        // Check whether any or all of the parent ids are in scope, based on parameter 'matchAny'
                                        linkCheck &= parentReadingReports[parentScopeReference].CheckLinkedEntities(parentIds, parentScopeReference.MatchAny, out var parentCodesInScope);
                                        if (parentCodesInScope != null && parentReadingReports[parentScopeReference].ScopingTypeDefinition.IsAutoScope) {
                                            autoScopeKeys.AddRange(parentCodesInScope.Select(r => (parentScopeReference, r)));
                                        }
                                        foreach (var parentId in parentIds) {
                                            linkingReports[parentScopeReference].AddCodeInSource(parentId, idDataSource);
                                        }
                                    } else {
                                        // Check whether the parent id is in scope
                                        linkCheck &= parentReadingReports[parentScopeReference].CheckLinkedEntity(fieldValue, out var isAutoScope);
                                        if (isAutoScope) {
                                            autoScopeKeys.Add((parentScopeReference, fieldValue));
                                        }
                                        linkingReports[parentScopeReference].AddCodeInSource(fieldValue, idDataSource);
                                    }
                                }

                                // Check if the code is in the explicit selection
                                var selectedCodes = readingSummary?.CodesInSelection ?? [];
                                var isInSelection = !selectedCodes.Any() || selectedCodes.Contains(id);

                                // Check if the code is in the explicit selection
                                var isSelected = linkCheck && isInSelection;

                                // If the code passes the selection filter AND the record passes the parent table link checks, then add this to the scope
                                if (isSelected) {
                                    if (hasId) {
                                        readingSummary?.AddCodeInScope(id);
                                    }
                                    foreach (var autoScopeKey in autoScopeKeys) {
                                        parentReadingReports[autoScopeKey.ParentReference].AddCodeInScope(autoScopeKey.Key);
                                    }
                                }
                            }
                        }
                    }
                }

                if (definition.IsStrongEntity && !definition.IsAutoScope) {
                    readingSummary.CodesInScope.IntersectWith(readingSummary.CodesInSource);
                }
            }
        }
    }
}
