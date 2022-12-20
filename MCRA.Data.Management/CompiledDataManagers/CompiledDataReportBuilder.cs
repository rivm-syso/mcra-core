using MCRA.General;
using MCRA.General.ScopingTypeDefinitions;
using System.Collections.Generic;

namespace MCRA.Data.Management.CompiledDataManagers {
    public sealed class CompiledDataReportBuilder {

        public Dictionary<ScopingType, DataReadingReport> DataReadingReports { get; }

        public CompiledDataReportBuilder() {
            DataReadingReports = new Dictionary<ScopingType, DataReadingReport>();
        }

        /// <summary>
        /// Creates the data reading report for the current scoping type.
        /// </summary>
        public DataReadingReport CreateDataReadingReport(ScopingType scopingType) {
            if (!DataReadingReports.TryGetValue(scopingType, out var readingReport)) {
                readingReport = new DataReadingReport() {
                    ScopingType = scopingType,
                    LinkingSummaries = new Dictionary<ScopingType, DataLinkingSummaryRecord>(),
                };
                DataReadingReports.Add(scopingType, readingReport);
            }
            return readingReport;
        }

        /// <summary>
        /// Initialises and returns a linking summary for the link between the specified scoping type
        /// and referenced scoping type.
        /// </summary>
        /// <param name="scopeReference"></param>
        public DataLinkingSummaryRecord CreateDataLinkingReport(ScopeReference scopeReference) {
            var readingReport = GetDataReadingReport(scopeReference.SourceScopingType);
            if (!readingReport.LinkingSummaries.TryGetValue(scopeReference.TargetScopingType, out var linkingSummary)) {
                var targetReadingReport = GetDataReadingReport(scopeReference.TargetScopingType);
                linkingSummary = new DataLinkingSummaryRecord(scopeReference, targetReadingReport);
                readingReport.LinkingSummaries[scopeReference.TargetScopingType] = linkingSummary;
            }
            return linkingSummary;
        }

        /// <summary>
        /// Gets or creates the data reading report for the specified scoping type.
        /// </summary>
        /// <param name="scopingType"></param>
        /// <returns></returns>
        public DataReadingReport GetDataReadingReport(ScopingType scopingType) {
            if (!DataReadingReports.TryGetValue(scopingType, out var record)) {
                record = new DataReadingReport() {
                    ScopingType = scopingType,
                    LinkingSummaries = new Dictionary<ScopingType, DataLinkingSummaryRecord>(),
                };
                DataReadingReports.Add(scopingType, record);
            }
            return record;
        }

        /// <summary>
        /// Returns the reading reports for the specified table group.
        /// </summary>
        /// <param name="tableGroup"></param>
        /// <returns></returns>
        public Dictionary<ScopingType, DataReadingReport> GetReportsOfTableGroup(SourceTableGroup tableGroup) {
            var result = new Dictionary<ScopingType, DataReadingReport>();
            foreach (var record in DataReadingReports.Values) {
                var tg = McraScopingTypeDefinitions.Instance.ScopingDefinitions[record.ScopingType].TableGroup;
                if (tg == tableGroup) {
                    result.Add(record.ScopingType, record);
                }
            }
            return result;
        }
    }
}
