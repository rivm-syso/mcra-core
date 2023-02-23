using MCRA.Data.Management.CompiledDataManagers;
using MCRA.Data.Management.RawDataProviders;
using MCRA.General;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Data.Management.Test.UnitTests.DataManagement {
    [TestClass]
    public class LinkTestsBase {
        protected CsvRawDataProvider _rawDataProvider;
        protected CompiledLinkManager _compiledLinkManager;

        [TestInitialize]
        public virtual void TestInitialize() {
            _rawDataProvider = new CsvRawDataProvider(@"Resources\Csv\");
            _compiledLinkManager = new CompiledLinkManager(_rawDataProvider);
        }

        protected void AssertDataReadingSummaryRecord(
            IDictionary<ScopingType, DataReadingReport> reports,
            ScopingType sourceTable,
            int totalRefCount,
            string codesSourceAndScope,
            string codesSourceNotScope,
            string codesScopeNotSource,
            char sep = ','
        ) {
            var record = reports[sourceTable].ReadingSummary;
            Assert.AreEqual(sourceTable, record.ScopingType);
            Assert.AreEqual(totalRefCount, record.CodesInSource.Count);
            checkKeysList(codesSourceAndScope, record.CodesInSourceAndScope, sep);
            checkKeysList(codesScopeNotSource, record.CodesInScopeNotInSource, sep);
            checkKeysList(codesSourceNotScope, record.CodesInSourceNotInScope, sep);
        }

        protected void AssertDataSourceReadingSummaryRecord(
            IDictionary<ScopingType, DataReadingReport> reports,
            ScopingType sourceTable,
            int idDataSource,
            string codesSourceAndScope,
            string codesSourceNotScope,
            string codesScopeNotSource,
            char sep = ','
        ) {
            var readingReport = reports[sourceTable].ReadingSummary;
            var record = reports[sourceTable].ReadingSummary.DataSourceReadingSummaryRecords[idDataSource];
            checkKeysList(codesSourceAndScope, record.CodesInSourceAndScope(readingReport.CodesInScope), sep);
            checkKeysList(codesScopeNotSource, record.CodesInScopeNotInSource(readingReport.CodesInScope), sep);
            checkKeysList(codesSourceNotScope, record.CodesInSourceNotInScope(readingReport.CodesInScope), sep);
        }

        protected void AssertDataLinkingSummaryRecord(
            IDictionary<ScopingType, DataReadingReport> reports,
            ScopingType sourceTable,
            ScopingType targetTable,
            int totalRefCount,
            string codesSourceAndScope,
            string codesSourceNotScope,
            string codesScopeNotSource,
            char sep = ','
        ) {
            var record = reports[sourceTable].LinkingSummaries[targetTable];
            Assert.AreEqual(sourceTable, record.ScopingType);
            Assert.AreEqual(targetTable, record.ReferencedScopingType);
            Assert.AreEqual(totalRefCount, record.CodesInSource.Count);
            checkKeysList(codesSourceAndScope, record.CodesInSourceAndScope, sep);
            checkKeysList(codesScopeNotSource, record.CodesInScopeNotInSource, sep);
            checkKeysList(codesSourceNotScope, record.CodesInSourceNotInScope, sep);
        }

        protected void AssertDataSourceLinkingSummaryRecord(
            IDictionary<ScopingType, DataReadingReport> reports,
            ScopingType sourceTable,
            ScopingType targetTable,
            int idDataSource,
            string codesSourceAndScope,
            string codesSourceNotScope,
            string codesScopeNotSource,
            char sep = ','
        ) {
            var linkingReport = reports[sourceTable].LinkingSummaries[targetTable];
            var record = linkingReport.DataSourceReadingSummaryRecords[idDataSource];
            checkKeysList(codesSourceAndScope, record.CodesInSourceAndScope(linkingReport.CodesInScope), sep);
            checkKeysList(codesScopeNotSource, record.CodesInScopeNotInSource(linkingReport.CodesInScope), sep);
            checkKeysList(codesSourceNotScope, record.CodesInSourceNotInScope(linkingReport.CodesInScope), sep);
        }

        private void checkKeysList(string checks, HashSet<string> codes, char sep = ',') {
            if (string.IsNullOrEmpty(checks)) {
                Assert.AreEqual(0, codes.Count);
            } else {
                var checkCodes = checks.Split(sep);
                Assert.AreEqual(checkCodes.Length, codes.Count);
                Assert.IsTrue(checkCodes.All(s => codes.Contains(s)));
            }
        }
    }
}
