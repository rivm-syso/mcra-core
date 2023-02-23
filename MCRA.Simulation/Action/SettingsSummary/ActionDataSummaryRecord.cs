using MCRA.Utils.ExtensionMethods;
using MCRA.General;

namespace MCRA.Simulation.Action {

    [Serializable]
    public sealed class ActionDataSummaryRecord : IActionSettingSummaryRecord {
        public SourceTableGroup SourceTableGroup { get; set; }
        public int IdDataSourceVersion { get; set; }
        public string DataSourceName { get; set; }
        public string DataSourcePath { get; set; }
        public string Checksum { get; set; }
        public int Version { get; set; }
        public string VersionName { get; set; }
        public DateTime? VersionDate { get; set; }

        public string Option {
            get {
                return $"Data source {SourceTableGroup.GetDisplayName(true)}";
            }
        }

        public string Value {
            get {
                return Path.Combine(DataSourcePath ?? "", DataSourceName ?? "");
            }
        }

        public bool IsValid { get; set; }

        public override int GetHashCode() {
            unchecked {
                int hash = 17;
                hash = hash * 31 + SourceTableGroup.GetHashCode();
                hash = hash * 31 + Checksum?.GetHashCode() ?? VersionDate.GetHashCode();
                return hash;
            }
        }
    }
}
