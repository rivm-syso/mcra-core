using MCRA.General;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Data.Management.Test.UnitTests.DataManagement {

    [TestClass]
    public class DataLinkingNonDietaryExposureSourceTests : LinkTestsBase {

        /// <summary>
        /// Test reading of simple non-dietary exposure sources data source.
        /// </summary>
        [TestMethod]
        public void DataLinkingNonDietaryExposureSources_TestSimple() {
            _rawDataProvider.SetDataTables(
                (ScopingType.NonDietaryExposureSources, @"NonDietaryExposureSourcesTests/NonDietaryExposureSourcesSimple")
            );

            _compiledLinkManager.LoadScope(SourceTableGroup.NonDietaryExposureSources);
            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.NonDietaryExposureSources);
            AssertDataReadingSummaryRecord(report, ScopingType.NonDietaryExposureSources, 3, "A,B,C", "", "");
        }

        /// <summary>
        /// Test reading of non-dietary exposure sources data source with scope filter.
        /// </summary>
        [TestMethod]
        public void DataLinkingFood_TestFiltered() {
            _rawDataProvider.SetDataTables(
                (ScopingType.NonDietaryExposureSources, @"NonDietaryExposureSourcesTests/NonDietaryExposureSourcesSimple")
            );
            _rawDataProvider.SetFilterCodes(ScopingType.NonDietaryExposureSources, ["A", "C"]);
            _compiledLinkManager.LoadScope(SourceTableGroup.NonDietaryExposureSources);
            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.NonDietaryExposureSources);
            AssertDataReadingSummaryRecord(report, ScopingType.NonDietaryExposureSources, 3, "A,C", "B", "");
        }

        /// <summary>
        /// Test reading of two food data sources.
        /// </summary>
        [TestMethod]
        public void DataLinkingFood_TestMultiple() {
            _rawDataProvider.SetDataTables(1, (ScopingType.NonDietaryExposureSources, @"NonDietaryExposureSourcesTests/NonDietaryExposureSourcesSimple"));
            _rawDataProvider.SetDataTables(2, (ScopingType.NonDietaryExposureSources, @"NonDietaryExposureSourcesTests/NonDietaryExposureSourcesAdditional"));
            _compiledLinkManager.LoadScope(SourceTableGroup.NonDietaryExposureSources);
            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.NonDietaryExposureSources);
            AssertDataReadingSummaryRecord(report, ScopingType.NonDietaryExposureSources, 6, "A,B,C,D,E,F", "", "");
            AssertDataSourceReadingSummaryRecord(report, ScopingType.NonDietaryExposureSources, 1, "A,B,C", "", "D,E,F");
            AssertDataSourceReadingSummaryRecord(report, ScopingType.NonDietaryExposureSources, 2, "D,E,F", "", "A,B,C");
        }

        /// <summary>
        /// Test reading of two food data sources with scope filter.
        /// </summary>
        [TestMethod]
        public void DataLinkingFood_TestMultipleFiltered() {
            _rawDataProvider.SetDataTables(1, (ScopingType.NonDietaryExposureSources, @"NonDietaryExposureSourcesTests/NonDietaryExposureSourcesSimple"));
            _rawDataProvider.SetDataTables(2, (ScopingType.NonDietaryExposureSources, @"NonDietaryExposureSourcesTests/NonDietaryExposureSourcesAdditional"));
            _rawDataProvider.SetFilterCodes(ScopingType.NonDietaryExposureSources, ["A", "C", "E", "xxx"]);
            _compiledLinkManager.LoadScope(SourceTableGroup.NonDietaryExposureSources);
            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.NonDietaryExposureSources);
            AssertDataReadingSummaryRecord(report, ScopingType.NonDietaryExposureSources, 6, "A,C,E", "B,D,F", "xxx");
            AssertDataSourceReadingSummaryRecord(report, ScopingType.NonDietaryExposureSources, 1, "A,C", "B", "E,xxx");
            AssertDataSourceReadingSummaryRecord(report, ScopingType.NonDietaryExposureSources, 2, "E", "D,F", "A,C,xxx");
        }
    }
}
