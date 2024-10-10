using MCRA.Data.Compiled;
using MCRA.Data.Management.CompiledDataManagers.DataReadingSummary;
using MCRA.General;
using MCRA.General.ScopingTypeDefinitions;

namespace MCRA.Data.Management.CompiledDataManagers {
    public class DataReadingSummaryRecord : DataSummaryRecordBase {

        private readonly HashSet<string> _codesInSource;
        private readonly HashSet<string> _codesInScope;
        private readonly HashSet<string> _codesInSelection;

        /// <summary>
        /// The scoping type definition of the scoping type of this reading summary.
        /// </summary>
        public ScopingTypeDefinition ScopingTypeDefinition { get; private set; }

        /// <summary>
        /// The source entities.
        /// </summary>
        public Dictionary<string, ScopeEntity> SourceEntities { get; private set; }

        public DataReadingSummaryRecord(
            ScopingTypeDefinition scopingTypeDefinition,
            HashSet<string> codesInSelection
        ) {
            ScopingTypeDefinition = scopingTypeDefinition;
            _codesInSelection = codesInSelection;
            _codesInSource = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            _codesInScope = (codesInSelection != null) 
                ? new HashSet<string>(codesInSelection, StringComparer.OrdinalIgnoreCase)
                : new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            SourceEntities = new Dictionary<string, ScopeEntity>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// The scoping type to which this reading summary belongs.
        /// </summary>
        public override ScopingType ScopingType {
            get {
                return ScopingTypeDefinition.Id;
            }
        }

        /// <summary>
        /// The codes in the scope.
        /// </summary>
        public override HashSet<string> CodesInScope {
            get {
                return _codesInScope;
            }
        }

        /// <summary>
        /// The codes in the selection.
        /// </summary>
        public override HashSet<string> CodesInSelection {
            get {
                return _codesInSelection;
            }
        }

        /// <summary>
        /// The codes in the source.
        /// </summary>
        public override HashSet<string> CodesInSource {
            get {
                return _codesInSource;
            }
        }

        /// <summary>
        /// States whether the scope filter is active.
        /// </summary>
        public bool IsScopeFilterActive {
            get {
                return CodesInSelection?.Any() ?? false;
            }
        }

        /// <summary>
        /// Marks the specified data source as unavailable.
        /// </summary>
        /// <param name="idDataSource"></param>
        public void SetDataSourceUnavailable(int idDataSource) {
            var dataSourceReadingSummaryRecord = getOrCreateDataSourceReadingSummaryRecord(idDataSource);
            dataSourceReadingSummaryRecord.Unavailable = true;
        }

        /// <summary>
        /// Adds the specified code to the codes in scope.
        /// </summary>
        /// <param name="code"></param>
        /// <param name="name"></param>
        /// <param name="idDataSource"></param>
        public void AddCodeInSource(
            string code,
            string name,
            int idDataSource
        ) {
            if (!SourceEntities.ContainsKey(code)) {
                SourceEntities.Add(code, new ScopeEntity(code, name));
            }
            _codesInSource.Add(code);
            var dataSourceReadingSummaryRecord = getOrCreateDataSourceReadingSummaryRecord(idDataSource);
            dataSourceReadingSummaryRecord.CodesInSource.Add(code);
        }

        /// <summary>
        /// Adds the specified code to the codes in scope.
        /// </summary>
        /// <param name="code"></param>
        public virtual void AddCodeInScope(string code) {
            CodesInScope.Add(code);
        }

        /// <summary>
        /// Check whether or not the specific code is in the scope.
        /// </summary>
        /// <param name="code  "></param>
        /// <param name="isAutoScope"></param>
        /// <returns></returns>
        public bool CheckLinkedEntity(
            string code,
            out bool isAutoScope
        ) {
            if ((CodesInSelection?.Any() ?? false) && !CodesInSelection.Contains(code)) {
                // There is an explicit selection that does not contain the code; FAIL
                isAutoScope = false;
                return false;
            } else if (CodesInScope.Contains(code)) {
                // Code is in scope: OK
                isAutoScope = false;
                return true;
            } else if (ScopingTypeDefinition.IsAutoScope) {
                // Code not in scope, but auto-scope: OK
                isAutoScope = true;
                return true;
            } else {
                // Code not in scope and scoping type is not auto-scope; FAIL
                isAutoScope = false;
                return false;
            }
        }

        /// <summary>
        /// Check whether or not the specific code(s) are in the scope.
        /// </summary>
        /// <param name="codes"></param>
        /// <param name="matchAny"></param>
        /// <param name="autoScopeCodes"></param>
        /// <returns></returns>
        public bool CheckLinkedEntities(
            string[] codes,
            bool matchAny,
            out List<string> autoScopeCodes
        ) {
            // Initial valid value; depends on matchAny
            var valid = !matchAny;

            // Codes that are auto-scope candidates
            autoScopeCodes = null;
            foreach (var code in codes) {
                if ((CodesInSelection?.Any() ?? false) && !CodesInSelection.Contains(code)) {
                    // There is an explicit selection that does not contain the code; FAIL
                    if (!matchAny) {
                        valid = false;
                        break;
                    }
                } else if (CodesInScope.Contains(code)) {
                    // Code is in scope: OK
                    if (matchAny) {
                        valid = true;
                    }
                } else if (ScopingTypeDefinition.IsAutoScope) {
                    // Code not in scope, but auto-scope: OK
                    autoScopeCodes = autoScopeCodes ?? new List<string>();
                    autoScopeCodes.Add(code);
                    if (matchAny) {
                        valid = true;
                    }
                } else {
                    // Code not in scope and scoping type is not auto-scope; FAIL
                    if (!matchAny) {
                        valid = false;
                        break;
                    }
                }
            }
            if (!valid) {
                autoScopeCodes = null;
            }
            return valid;
        }

        private DataSourceReadingSummaryRecord getOrCreateDataSourceReadingSummaryRecord(int idDataSource) {
            if (!DataSourceReadingSummaryRecords.TryGetValue(idDataSource, out var summaryRecord)) {
                summaryRecord = new DataSourceReadingSummaryRecord();
                DataSourceReadingSummaryRecords.Add(idDataSource, summaryRecord);
            }
            return summaryRecord;
        }
    }
}
