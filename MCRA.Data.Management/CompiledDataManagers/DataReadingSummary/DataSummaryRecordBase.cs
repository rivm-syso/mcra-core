using MCRA.General;

namespace MCRA.Data.Management.CompiledDataManagers.DataReadingSummary {
    public abstract class DataSummaryRecordBase {

        public abstract ScopingType ScopingType { get; }

        public abstract HashSet<string> CodesInScope { get; }

        public abstract HashSet<string> CodesInSelection { get; }

        public abstract HashSet<string> CodesInSource { get; }

        /// <summary>
        /// Reading summaries per data source.
        /// </summary>
        public Dictionary<int, DataSourceReadingSummaryRecord> DataSourceReadingSummaryRecords { get; set; }
            = [];

        /// <summary>
        /// Data reading validation results.
        /// </summary>
        public ICollection<IDataValidationResult> ValidationResults { get; set; }

        /// <summary>
        /// Returns the maximum alert type.
        /// </summary>
        /// <returns></returns>
        public AlertType GetValidationStatus() {
            if (DataSourceReadingSummaryRecords?.Any(r => r.Value.Unavailable) ?? true) {
                return AlertType.Error;
            } else if (ValidationResults?.Count > 0) {
                return ValidationResults.Max(r => r.AlertType);
            } else {
                return AlertType.None;
            }
        }

        /// <summary>
        /// Returns the codes in scope, not in source.
        /// </summary>
        public HashSet<string> CodesInSourceAndScope {
            get {
                return CodesInScope.Intersect(CodesInSource, StringComparer.OrdinalIgnoreCase).ToHashSet(StringComparer.OrdinalIgnoreCase);
            }
        }

        /// <summary>
        /// Returns the codes in scope, not in source.
        /// </summary>
        public HashSet<string> CodesInScopeNotInSource {
            get {
                return CodesInScope.Except(CodesInSource, StringComparer.OrdinalIgnoreCase).ToHashSet(StringComparer.OrdinalIgnoreCase);
            }
        }

        /// <summary>
        /// Returns the codes in scope, not in source.
        /// </summary>
        public HashSet<string> CodesInSourceNotInScope {
            get {
                return CodesInSource.Except(CodesInScope, StringComparer.OrdinalIgnoreCase).ToHashSet(StringComparer.OrdinalIgnoreCase);
            }
        }

        /// <summary>
        /// Returns all codes.
        /// </summary>
        public HashSet<string> AllCodes {
            get {
                return CodesInScope.Union(CodesInSource, StringComparer.OrdinalIgnoreCase).ToHashSet(StringComparer.OrdinalIgnoreCase);
            }
        }
    }
}
