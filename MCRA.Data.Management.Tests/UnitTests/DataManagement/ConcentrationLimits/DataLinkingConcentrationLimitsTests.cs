using MCRA.General;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Data.Management.Test.UnitTests.DataManagement {
    [TestClass]
    public class DataLinkingConcentrationLimitsTests : LinkTestsBase {
        [TestMethod]
        public void DataLinkingConcentrationLimitsSimpleTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.ConcentrationLimits, @"MaximumResidueLimitsTests\MaximumResidueLimitsSimple")
            );

            _compiledLinkManager.LoadScope(SourceTableGroup.MaximumResidueLimits);

            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.MaximumResidueLimits);
            AssertDataLinkingSummaryRecord(report, ScopingType.ConcentrationLimits, ScopingType.Foods, 6, "f1,f2,f3,f4,f5,f6", "", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.ConcentrationLimits, ScopingType.Compounds, 6, "A,B,C,D,E,F", "", "");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Foods);
            AssertDataReadingSummaryRecord(report, ScopingType.Foods, 0, "", "", "f1,f2,f3,f4,f5,f6");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Compounds);
            AssertDataReadingSummaryRecord(report, ScopingType.Compounds, 0, "", "", "A,B,C,D,E,F");
        }


        [TestMethod]
        public void DataLinkingConcentrationLimitsSimpleFoodsFilterTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.ConcentrationLimits, @"MaximumResidueLimitsTests\MaximumResidueLimitsSimple")
            );
            _rawDataProvider.SetFilterCodes(ScopingType.Foods, new[] { "f1" });

            _compiledLinkManager.LoadScope(SourceTableGroup.MaximumResidueLimits);

            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.MaximumResidueLimits);
            AssertDataLinkingSummaryRecord(report, ScopingType.ConcentrationLimits, ScopingType.Foods, 6, "f1", "f2,f3,f4,f5,f6", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.ConcentrationLimits, ScopingType.Compounds, 6, "B", "A,C,D,E,F", "");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Foods);
            AssertDataReadingSummaryRecord(report, ScopingType.Foods, 0, "", "", "f1");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Compounds);
            AssertDataReadingSummaryRecord(report, ScopingType.Compounds, 0, "", "", "B");
        }

        [TestMethod]
        public void DataLinkingConcentrationLimitsSimpleCompoundsFilterTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.ConcentrationLimits, @"MaximumResidueLimitsTests\MaximumResidueLimitsSimple")
            );
            _rawDataProvider.SetFilterCodes(ScopingType.Compounds, new[] { "B", "C" });

            _compiledLinkManager.LoadScope(SourceTableGroup.MaximumResidueLimits);

            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.MaximumResidueLimits);
            AssertDataLinkingSummaryRecord(report, ScopingType.ConcentrationLimits, ScopingType.Foods, 6, "f1,f2,f3,f4,f5", "f6", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.ConcentrationLimits, ScopingType.Compounds, 6, "b,c", "a,d,e,f", "");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Foods);
            AssertDataReadingSummaryRecord(report, ScopingType.Foods, 0, "", "", "f1,f2,f3,f4,f5");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Compounds);
            AssertDataReadingSummaryRecord(report, ScopingType.Compounds, 0, "", "", "B,C");
        }

        [TestMethod]
        public void DataLinkingConcentrationLimitsFilterFoodsAndCompoundsSimpleTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.ConcentrationLimits, @"MaximumResidueLimitsTests\MaximumResidueLimitsSimple")
            );
            _rawDataProvider.SetFilterCodes(ScopingType.Foods, new[] { "f1" });
            _rawDataProvider.SetFilterCodes(ScopingType.Compounds, new[] { "B", "C" });

            _compiledLinkManager.LoadScope(SourceTableGroup.MaximumResidueLimits);

            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.MaximumResidueLimits);
            AssertDataLinkingSummaryRecord(report, ScopingType.ConcentrationLimits, ScopingType.Foods, 6, "f1", "f2,f3,f4,f5,f6", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.ConcentrationLimits, ScopingType.Compounds, 6, "b,c", "a,d,e,f", "");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Foods);
            AssertDataReadingSummaryRecord(report, ScopingType.Foods, 0, "", "", "f1");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Compounds);
            AssertDataReadingSummaryRecord(report, ScopingType.Compounds, 0, "", "", "B,C");
        }
    }
}
