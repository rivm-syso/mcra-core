using MCRA.Data.Management.CompiledDataManagers.DataReadingSummary;
using MCRA.General;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Data.Management.CompiledDataManagers {
    public sealed class DataReadingReport {

        public ScopingType ScopingType { get; set; }

        public DataReadingSummaryRecord ReadingSummary { get; set; }

        public Dictionary<ScopingType, DataLinkingSummaryRecord> LinkingSummaries { get; set; }

        public bool IsError {
            get {
                return ((ReadingSummary?.GetValidationStatus() ?? AlertType.None) == AlertType.Error)
                    || (LinkingSummaries?.Values?.Any(r => r.IsError()) ?? false);
            }
        }

        public bool HasData {
            get {
                var hasReadingSummary = ReadingSummary?.CodesInScope?.Any() ?? false;
                var hasLinkingSummaries = LinkingSummaries.Any(r => r.Value.CodesInSource?.Any() ?? false);
                return hasReadingSummary || hasLinkingSummaries;
            }
        }
    }
}
