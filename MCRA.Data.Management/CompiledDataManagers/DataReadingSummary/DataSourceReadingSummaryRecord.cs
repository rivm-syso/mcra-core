namespace MCRA.Data.Management.CompiledDataManagers {
    public sealed class DataSourceReadingSummaryRecord {

        public HashSet<string> CodesInSource { get; set; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public bool Unavailable { get; set; }

        /// <summary>
        /// Returns the codes in scope, not in source.
        /// </summary>
        public HashSet<string> CodesInSourceAndScope(ICollection<string> codesInScope) {
            return codesInScope.Intersect(CodesInSource, StringComparer.OrdinalIgnoreCase).ToHashSet(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Returns the codes in scope, not in source.
        /// </summary>
        public HashSet<string> CodesInScopeNotInSource(ICollection<string> codesInScope) {
            return codesInScope.Except(CodesInSource, StringComparer.OrdinalIgnoreCase).ToHashSet(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Returns the codes in scope, not in source.
        /// </summary>
        public HashSet<string> CodesInSourceNotInScope(ICollection<string> codesInScope) {
            return CodesInSource.Except(codesInScope, StringComparer.OrdinalIgnoreCase).ToHashSet(StringComparer.OrdinalIgnoreCase);
        }
    }
}
