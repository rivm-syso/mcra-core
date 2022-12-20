using MCRA.General;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Data.Management.Test.UnitTests.DataManagement {
    [TestClass]
    public class DataLinkingProcessingTests : LinkTestsBase {

        [TestMethod]
        public void DataLinkingProcessingSimpleTypesFilterTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.ProcessingTypes, @"ProcessingTests\ProcessingTypesSimple"),
                (ScopingType.ProcessingFactors, @"ProcessingTests\ProcessingFactorsSimple")
            );

            _compiledLinkManager.LoadScope(SourceTableGroup.Processing);

            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Processing);
            AssertDataLinkingSummaryRecord(report, ScopingType.ProcessingFactors, ScopingType.ProcessingTypes, 5, "t1,t2,t3,t4,t5", "", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.ProcessingFactors, ScopingType.Foods, 5, "f2,f3,f4,f5,f6", "", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.ProcessingFactors, ScopingType.Compounds, 5, "a,b,c,d,e", "", "");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Foods);
            AssertDataReadingSummaryRecord(report, ScopingType.ProcessingTypes, 3, "t1,t2,t3", "", "t4,t5");
            AssertDataReadingSummaryRecord(report, ScopingType.Foods, 0, "", "", "f2,f3,f4,f5,f6");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Compounds);
            AssertDataReadingSummaryRecord(report, ScopingType.Compounds, 0, "", "", "a,b,c,d,e");
        }

        [TestMethod]
        public void DataLinkingProcessingSimpleCompoundsFilterTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.ProcessingFactors, @"ProcessingTests\ProcessingFactorsSimple")
            );
            _rawDataProvider.SetFilterCodes(ScopingType.Compounds, new[] { "B", "C" });

            _compiledLinkManager.LoadScope(SourceTableGroup.Processing);

            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Processing);
            AssertDataLinkingSummaryRecord(report, ScopingType.ProcessingFactors, ScopingType.ProcessingTypes, 5, "t1,t2,t3,t4", "t5", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.ProcessingFactors, ScopingType.Foods, 5, "f2,f3,f4,f5", "f6", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.ProcessingFactors, ScopingType.Compounds, 5, "b,c", "a,d,e", "");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Foods);
            AssertDataReadingSummaryRecord(report, ScopingType.ProcessingTypes, 0, "", "", "t1,t2,t3,t4");
            AssertDataReadingSummaryRecord(report, ScopingType.Foods, 0, "", "", "f2,f3,f4,f5");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Compounds);
            AssertDataReadingSummaryRecord(report, ScopingType.Compounds, 0, "", "", "b,c");
        }

        [TestMethod]
        public void DataLinkingProcessingFactorsFilterFoodsAndCompoundsSimpleTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.ProcessingFactors, @"ProcessingTests\ProcessingFactorsSimple")
            );
            _rawDataProvider.SetFilterCodes(ScopingType.Foods, new[] { "f2" });
            _rawDataProvider.SetFilterCodes(ScopingType.Compounds, new[] { "A", "C" });

            _compiledLinkManager.LoadScope(SourceTableGroup.Processing);

            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Processing);
            AssertDataLinkingSummaryRecord(report, ScopingType.ProcessingFactors, ScopingType.ProcessingTypes, 5, "t1,t3", "t2,t4,t5", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.ProcessingFactors, ScopingType.Foods, 5, "f2", "f3,f4,f5,f6", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.ProcessingFactors, ScopingType.Compounds, 5, "a,c", "b,d,e", "");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Foods);
            AssertDataReadingSummaryRecord(report, ScopingType.ProcessingTypes, 0, "", "", "t1,t3");
            AssertDataReadingSummaryRecord(report, ScopingType.Foods, 0, "", "", "f2");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Compounds);
            AssertDataReadingSummaryRecord(report, ScopingType.Compounds, 0, "", "", "a,c");
        }

        [TestMethod]
        public void DataLinkingProcessingSimpleTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.ProcessingFactors, @"ProcessingTests\ProcessingFactorsSimpleOld")
            );

            _compiledLinkManager.LoadScope(SourceTableGroup.Processing);

            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Processing);
            AssertDataLinkingSummaryRecord(report, ScopingType.ProcessingFactors, ScopingType.ProcessingTypes, 5, "t1,t2,t3,t4,t5", "", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.ProcessingFactors, ScopingType.Foods, 6, "f1,f2,f3,f4,f5,f6", "", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.ProcessingFactors, ScopingType.Compounds, 5, "a,b,c,d,e", "", "");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Foods);
            AssertDataReadingSummaryRecord(report, ScopingType.Foods, 0, "", "", "f1,f2,f3,f4,f5,f6");
            AssertDataReadingSummaryRecord(report, ScopingType.ProcessingTypes, 0, "", "", "t1,t2,t3,t4,t5");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Compounds);
            AssertDataReadingSummaryRecord(report, ScopingType.Compounds, 0, "", "", "a,b,c,d,e");
        }

        [TestMethod]
        public void DataLinkingProcessingSimpleFoodsFilterTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.ProcessingFactors, @"ProcessingTests\ProcessingFactorsSimpleOld")
            );
            _rawDataProvider.SetFilterCodes(ScopingType.Foods, new[] { "f1" });

            _compiledLinkManager.LoadScope(SourceTableGroup.Processing);

            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Processing);
            AssertDataLinkingSummaryRecord(report, ScopingType.ProcessingFactors, ScopingType.ProcessingTypes, 5, "", "t1,t2,t3,t4,t5", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.ProcessingFactors, ScopingType.Foods, 6, "f1", "f2,f3,f4,f5,f6", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.ProcessingFactors, ScopingType.Compounds, 5, "", "a,b,c,d,e", "");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Foods);
            AssertDataReadingSummaryRecord(report, ScopingType.ProcessingTypes, 0, "", "", "");
            AssertDataReadingSummaryRecord(report, ScopingType.Foods, 0, "", "", "f1");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Compounds);
            AssertDataReadingSummaryRecord(report, ScopingType.Compounds, 0, "", "", "");
        }
    }
}
