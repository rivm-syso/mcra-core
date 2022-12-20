using MCRA.Data.Management.CompiledDataManagers.DataReadingSummary;
using MCRA.General;
using MCRA.General.ScopingTypeDefinitions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Data.Management.CompiledDataManagers {

    public sealed class DataLinkingSummaryRecord : DataSummaryRecordBase {

        private readonly HashSet<string> _codesInSource;

        public ScopingTypeDefinition ScopingTypeDefinition { get; private set; }

        /// <summary>
        /// The referenced scoping type.
        /// </summary>
        public ScopeReference ScopeReference { get; set; }

        /// <summary>
        /// The parent reading report. I.e., the reading report for the
        /// scoping type to which this linking summary is linking.
        /// </summary>
        public DataReadingReport ParentReadingReport { get; set; }

        public DataLinkingSummaryRecord(
            ScopeReference scopeReference,
            DataReadingReport parentReadingReport
        ) {
            ScopeReference = scopeReference;
            ParentReadingReport = parentReadingReport;
            _codesInSource = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Adds the specified code to the codes in scope.
        /// </summary>
        /// <param name="code"></param>
        /// <param name="idDataSource"></param>
        public void AddCodeInSource(
            string code,
            int idDataSource
        ) {
            _codesInSource.Add(code);
            if (!DataSourceReadingSummaryRecords.TryGetValue(idDataSource, out var dataSourceReadingSummaryRecord)) {
                dataSourceReadingSummaryRecord = new DataSourceReadingSummaryRecord();
                DataSourceReadingSummaryRecords.Add(idDataSource, dataSourceReadingSummaryRecord);
            }
            dataSourceReadingSummaryRecord.CodesInSource.Add(code);
        }

        /// <summary>
        /// The scoping type to which this reading summary belongs.
        /// </summary>
        public override ScopingType ScopingType {
            get {
                return ScopeReference.SourceScopingType;
            }
        }

        /// <summary>
        /// The scoping type to which this reading summary belongs.
        /// </summary>
        public ScopingType ReferencedScopingType {
            get {
                return ScopeReference.TargetScopingType;
            }
        }

        /// <summary>
        /// The codes referenced in the source.
        /// </summary>
        public override HashSet<string> CodesInSource {
            get {
                return _codesInSource;
            }
        }

        /// <summary>
        /// Codes in scope are obtained from reading report.
        /// </summary>
        public override HashSet<string> CodesInScope {
            get {
                return ParentReadingReport.ReadingSummary.CodesInScope;
            }
        }

        /// <summary>
        /// Codes in scope are obtained from reading report.
        /// </summary>
        public override HashSet<string> CodesInSelection {
            get {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Returns whether there linking warnings.
        /// </summary>
        /// <returns></returns>
        public bool IsWarning() {
            return DataSourceReadingSummaryRecords.Any(r => r.Value.Unavailable)
                || (ValidationResults?.Any(r => r.AlertType >= AlertType.Warning) ?? false);
        }

        /// <summary>
        /// Returns whether there liniking errors.
        /// </summary>
        /// <returns></returns>
        public bool IsError() {
            return DataSourceReadingSummaryRecords.Any(r => r.Value.Unavailable)
                || (ValidationResults?.Any(r => r.AlertType >= AlertType.Error) ?? false);
        }
    }
}
