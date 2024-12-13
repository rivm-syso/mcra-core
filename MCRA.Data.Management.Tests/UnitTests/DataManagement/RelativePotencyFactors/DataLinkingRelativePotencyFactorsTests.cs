using MCRA.General;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Data.Management.Test.UnitTests.DataManagement {
    [TestClass]
    public class DataLinkingRelativePotencyFactorsTests : LinkTestsBase {
        [TestMethod]
        public void DataLinkingRelativePotencyFactorsSimpleTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.RelativePotencyFactors, @"RelativePotencyFactorsTests\RelativePotencyFactorsSimple")
            );

            _compiledLinkManager.LoadScope(SourceTableGroup.RelativePotencyFactors);

            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.RelativePotencyFactors);
            AssertDataLinkingSummaryRecord(report, ScopingType.RelativePotencyFactors, ScopingType.Compounds, 6, "A,B,C,D,E,F", "", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.RelativePotencyFactors, ScopingType.Effects, 6, "Eff1,eff2,eff3,eff4,eff5,eff6", "", "");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Effects);
            AssertDataReadingSummaryRecord(report, ScopingType.Effects, 0, "", "", "Eff1,eff2,eff3,eff4,eff5,eff6");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Compounds);
            AssertDataReadingSummaryRecord(report, ScopingType.Compounds, 0, "", "", "A,B,C,D,E,F");
        }


        [TestMethod]
        public void DataLinkingRelativePotencyFactorsSimpleEffectsFilterTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.RelativePotencyFactors, @"RelativePotencyFactorsTests\RelativePotencyFactorsSimple")
            );
            _rawDataProvider.SetFilterCodes(ScopingType.Effects, ["Eff1"]);

            _compiledLinkManager.LoadScope(SourceTableGroup.RelativePotencyFactors);

            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.RelativePotencyFactors);
            AssertDataLinkingSummaryRecord(report, ScopingType.RelativePotencyFactors, ScopingType.Compounds, 6, "B", "A,C,D,E,F", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.RelativePotencyFactors, ScopingType.Effects, 6, "Eff1", "Eff2,eff3,eff4,eff5,eff6", "");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Effects);
            AssertDataReadingSummaryRecord(report, ScopingType.Effects, 0, "", "", "Eff1");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Compounds);
            AssertDataReadingSummaryRecord(report, ScopingType.Compounds, 0, "", "", "B");
        }

        [TestMethod]
        public void DataLinkingRelativePotencyFactorsSimpleCompoundsFilterTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.RelativePotencyFactors, @"RelativePotencyFactorsTests\RelativePotencyFactorsSimple")
            );
            _rawDataProvider.SetFilterCodes(ScopingType.Compounds, ["B", "C"]);

            _compiledLinkManager.LoadScope(SourceTableGroup.RelativePotencyFactors);

            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.RelativePotencyFactors);
            AssertDataLinkingSummaryRecord(report, ScopingType.RelativePotencyFactors, ScopingType.Compounds, 6, "b,c", "a,d,e,f", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.RelativePotencyFactors, ScopingType.Effects, 6, "Eff1,eff2,eff3,eff4,eff5", "eff6", "");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Effects);
            AssertDataReadingSummaryRecord(report, ScopingType.Effects, 0, "", "", "Eff1,eff2,eff3,eff4,eff5");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Compounds);
            AssertDataReadingSummaryRecord(report, ScopingType.Compounds, 0, "", "", "b,c");
        }

        [TestMethod]
        public void DataLinkingRelativePotencyFactorsFilterEffectsAndCompoundsSimpleTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.RelativePotencyFactors, @"RelativePotencyFactorsTests\RelativePotencyFactorsSimple")
            );
            _rawDataProvider.SetFilterCodes(ScopingType.Effects, ["Eff1"]);
            _rawDataProvider.SetFilterCodes(ScopingType.Compounds, ["B", "C"]);

            _compiledLinkManager.LoadScope(SourceTableGroup.RelativePotencyFactors);

            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.RelativePotencyFactors);
            AssertDataLinkingSummaryRecord(report, ScopingType.RelativePotencyFactors, ScopingType.Compounds, 6, "b,c", "a,d,e,f", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.RelativePotencyFactors, ScopingType.Effects, 6, "Eff1", "Eff2,eff3,eff4,eff5,eff6", "");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Effects);
            AssertDataReadingSummaryRecord(report, ScopingType.Effects, 0, "", "", "Eff1");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Compounds);
            AssertDataReadingSummaryRecord(report, ScopingType.Compounds, 0, "", "", "b,c");
        }
    }
}
