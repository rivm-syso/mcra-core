using MCRA.Utils.ExtensionMethods;
using MCRA.Data.Compiled.Objects;
using MCRA.Data.Raw;
using MCRA.General;
using MCRA.General.ScopingTypeDefinitions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Data.Management.CompiledDataManagers {
    public partial class CompiledLinkManager : CompiledLinkManagerBase, ICompiledLinkManager {

        /// <summary>
        /// Instantiate with a rawdatamanager and rawdatasource ids per table group
        /// </summary>
        /// <param name="rawDataProvider"></param>
        public CompiledLinkManager(IRawDataProvider rawDataProvider)
            : base(rawDataProvider) {
        }

        /// <summary>
        /// Gets the codes in the scope of the specified scoping type.
        /// </summary>
        /// <param name="scopingType"></param>
        /// <returns></returns>
        public HashSet<string> GetCodesInScope(ScopingType scopingType) {
            var definition = McraScopingTypeDefinitions.Instance.ScopingDefinitions[scopingType];
            if (definition.IsStrongEntity) {
                loadScopingType(scopingType);
                return _reportBuilder.DataReadingReports[scopingType].ReadingSummary.CodesInScope;
            }
            throw new Exception("Cannot get scope entities for weak entity types.");
        }

        /// <summary>
        /// Gets the codes in the source. When the referencing scoping type is selected,
        /// the codes are represent target scope code to which is linked from the specified
        /// referencing scoping type. When the data source is specified, the codes restrict
        /// to the codes of that particular data source (provided that this data source is
        /// specified for the referencing scoping type, or the target scoping type).
        /// </summary>
        /// <param name="scopingType"></param>
        /// <param name="referencingScopingType"></param>
        /// <param name="idDataSource"></param>
        /// <returns></returns>
        public HashSet<string> GetCodesInSource(
            ScopingType scopingType,
            ScopingType referencingScopingType = ScopingType.Unknown,
            int idDataSource = -1
        ) {
            var definition = McraScopingTypeDefinitions.Instance.ScopingDefinitions[scopingType];
            if (definition.IsStrongEntity) {
                if (referencingScopingType != ScopingType.Unknown) {
                    loadScopingType(referencingScopingType);
                    if (_reportBuilder.DataReadingReports.TryGetValue(referencingScopingType, out var readingReport)
                        && readingReport.LinkingSummaries.TryGetValue(scopingType, out var dataLinkingSummary)
                    ) {
                        if (idDataSource >= 0) {
                            dataLinkingSummary.DataSourceReadingSummaryRecords.TryGetValue(idDataSource, out var result);
                            return result.CodesInSource;
                        } else {
                            return dataLinkingSummary.CodesInSource;
                        }
                    }
                    return null;
                } else {
                    loadScopingType(scopingType);
                    if (_reportBuilder.DataReadingReports.TryGetValue(scopingType, out var readingReport)) {
                        if (idDataSource >= 0) {
                            readingReport.ReadingSummary.DataSourceReadingSummaryRecords.TryGetValue(idDataSource, out var result);
                            return result.CodesInSource;
                        } else {
                            return readingReport.ReadingSummary.CodesInSource;
                        }
                    }
                    return null;
                }
            }
            throw new Exception("Cannot get source entities for weak entity types.");
        }

        /// <summary>
        /// Returns a dictionary of all strong entity objects for the specified entity type.
        /// </summary>
        /// <param name="targetScope"></param>
        /// <param name="sourceScope">If specified, then only the codes referenced by the
        /// specified source scoping type will be retreived.</param>
        /// <param name="idDataSource">If specified (i.e., >-1), then only the codes
        /// referenced by the specified data source will be retreived.</param>
        /// <returns>A collection of codes.</returns>
        public HashSet<string> GetAllCodes(
            ScopingType targetScope,
            ScopingType sourceScope = ScopingType.Unknown,
            int idDataSource = -1
        ) {
            var codesInScope = GetCodesInScope(targetScope);
            var codesInSource = GetCodesInSource(targetScope, sourceScope, idDataSource);
            var result = new HashSet<string>(codesInScope.Union(codesInSource), StringComparer.OrdinalIgnoreCase);
            return result;
        }

        /// <summary>
        /// Gets all scope entities.
        /// </summary>
        /// <param name="scopingType"></param>
        /// <returns></returns>
        public IDictionary<string, StrongEntity> GetAllScopeEntities(ScopingType scopingType) {
            loadScopingType(scopingType);
            var codesInScope = GetCodesInScope(scopingType);
            var readingSummary = _reportBuilder.DataReadingReports[scopingType].ReadingSummary;
            var readingSourceEntities = readingSummary.SourceEntities;
            var result = codesInScope
                .ToDictionary(
                    r => r,
                    r => {
                        if (!readingSourceEntities.TryGetValue(r, out var entity)) {
                            entity = new StrongEntity(r);
                        } else {
                            entity.IsInSource = true;
                        }
                        entity.IsSelected = true;
                        return entity;
                    },
                    StringComparer.OrdinalIgnoreCase
                );
            return result;
        }

        /// <summary>
        /// Gets all scope entities.
        /// </summary>
        /// <param name="targetScope"></param>
        /// <param name="sourceScope"></param>
        /// <param name="idDataSource"></param>
        /// <returns></returns>
        public IDictionary<string, StrongEntity> GetAllSourceEntities(
            ScopingType targetScope,
            ScopingType sourceScope = ScopingType.Unknown,
            int idDataSource = -1
        ) {
            var targetDefinition = McraScopingTypeDefinitions.Instance.ScopingDefinitions[targetScope];
            if (!targetDefinition.IsStrongEntity) {
                throw new Exception("Cannot get source entities for weak entity types.");
            }
            var codesInSource = GetCodesInSource(targetScope, sourceScope, idDataSource);
            if (codesInSource != null
                && _reportBuilder.DataReadingReports.TryGetValue(targetScope, out var readingReport)
            ) {
                var readingSummary = readingReport.ReadingSummary;
                var readingSourceEntities = readingSummary.SourceEntities;
                var result = codesInSource
                    .ToDictionary(
                        r => r,
                        r => readingSourceEntities.TryGetValue(r, out var entity)
                            ? entity
                            : new StrongEntity(r) {
                                IsSelected = readingSummary.CodesInScope.Contains(r),
                                IsInSource = true
                            },
                        StringComparer.OrdinalIgnoreCase
                    );
                return result;
            }
            return null;
        }

        /// <summary>
        /// Read all relevant entities of the given strong entity type, including both the entities
        /// in the source and referenced keys.
        /// </summary>
        /// <param name="scopingType"></param>
        /// <returns></returns>
        public IDictionary<string, StrongEntity> GetAllEntities(ScopingType scopingType) {
            var targetDefinition = McraScopingTypeDefinitions.Instance.ScopingDefinitions[scopingType];
            if (!targetDefinition.IsStrongEntity) {
                throw new Exception("Cannot get source entities for weak entity types.");
            }
            loadScopingType(scopingType);
            var readingSummary = _reportBuilder.DataReadingReports[scopingType].ReadingSummary;
            var readingSourceEntities = readingSummary.SourceEntities;
            var result = GetAllCodes(scopingType)
                .ToDictionary(
                    r => r,
                    r => {
                        if (!readingSourceEntities.TryGetValue(r, out var entity)) {
                            entity = new StrongEntity(r);
                        } else {
                            entity.IsInSource = true;
                        }
                        entity.IsSelected = readingSummary.CodesInScope.Contains(r);
                        return entity;
                    },
                    StringComparer.OrdinalIgnoreCase
                );
            return result;
        }

        /// <summary>
        /// Returns whether the specified code is selected for the specified scoping type.
        /// </summary>
        /// <param name="scopingType"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        public bool IsCodeSelected(ScopingType scopingType, string code) {
            return _reportBuilder.DataReadingReports[scopingType].ReadingSummary.CodesInScope.Contains(code);
        }

        public bool CheckLinkSelected(ScopingType targetScope, bool matchAny, params string[] codes) {
            if (matchAny) {
                return codes.Any(code => IsCodeSelected(targetScope, code));
            } else {
                return codes.All(code => IsCodeSelected(targetScope, code));
            }
        }

        public bool CheckLinkSelected(ScopingType targetScope, params string[] codes) {
            return CheckLinkSelected(targetScope, true, codes);
        }

        /// <summary>
        /// Retrieve data reading report from report builder.
        /// </summary>
        public Dictionary<ScopingType, DataReadingReport> GetDataReadingReports(SourceTableGroup tableGroup) {
            return _reportBuilder.GetReportsOfTableGroup(tableGroup);
        }
    }
}
