using MCRA.General;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Data.Management.Test.UnitTests.DataManagement {
    [TestClass]
    public class DataLinkingUnitVariabilityFactorsTests : LinkTestsBase {
        [TestInitialize]
        public override void TestInitialize() {
            base.TestInitialize();
        }

        [TestMethod]
        public void DataLinkingUnitVariabilityFactorsSimpleTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.UnitVariabilityFactors, @"UnitVariabilityFactorsTests\UnitVariabilityFactorsSimple")
            );

            _compiledLinkManager.LoadScope(SourceTableGroup.UnitVariabilityFactors);

            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.UnitVariabilityFactors);
            AssertDataLinkingSummaryRecord(report, ScopingType.UnitVariabilityFactors, ScopingType.ProcessingTypes, 5, "t1,t2,t3,t4,t5", "", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.UnitVariabilityFactors, ScopingType.Foods, 8, "f1,f2,f3,f4,f5,f6,f7,f8", "", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.UnitVariabilityFactors, ScopingType.Compounds, 6, "A,B,C,D,E,F", "", "");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Foods);
            AssertDataReadingSummaryRecord(report, ScopingType.ProcessingTypes, 0, "", "", "t1,t2,t3,t4,t5");
            AssertDataReadingSummaryRecord(report, ScopingType.Foods, 0, "", "", "f1,f2,f3,f4,f5,f6,f7,f8");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Compounds);
            AssertDataReadingSummaryRecord(report, ScopingType.Compounds, 0, "", "", "A,B,C,D,E,F");
        }

        [TestMethod]
        public void DataLinkingUnitVariabilityFactorsSimpleProcessingTypesFilterTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.UnitVariabilityFactors, @"UnitVariabilityFactorsTests\UnitVariabilityFactorsSimple")
            );

            _compiledLinkManager.LoadScope(SourceTableGroup.UnitVariabilityFactors);

            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.UnitVariabilityFactors);
            AssertDataLinkingSummaryRecord(report, ScopingType.UnitVariabilityFactors, ScopingType.ProcessingTypes, 5, "t1,t2,t3,t4,t5", "", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.UnitVariabilityFactors, ScopingType.Foods, 8, "f1,f2,f3,f4,f5,f6,f7,f8", "", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.UnitVariabilityFactors, ScopingType.Compounds, 6, "A,B,C,D,E,F", "", "");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Foods);
            AssertDataReadingSummaryRecord(report, ScopingType.ProcessingTypes, 0, "", "", "t1,t2,t3,t4,t5");
            AssertDataReadingSummaryRecord(report, ScopingType.Foods, 0, "", "", "f1,f2,f3,f4,f5,f6,f7,f8");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Compounds);
            AssertDataReadingSummaryRecord(report, ScopingType.Compounds, 0, "", "", "A,B,C,D,E,F");
        }


        [TestMethod]
        public void DataLinkingUnitVariabilityFactorsSimpleFoodsFilterTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.UnitVariabilityFactors, @"UnitVariabilityFactorsTests\UnitVariabilityFactorsSimple")
            );

            _rawDataProvider.SetFilterCodes(ScopingType.Foods, new[] { "f1" });

            _compiledLinkManager.LoadScope(SourceTableGroup.UnitVariabilityFactors);

            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.UnitVariabilityFactors);
            AssertDataLinkingSummaryRecord(report, ScopingType.UnitVariabilityFactors, ScopingType.ProcessingTypes, 5, "", "t1,t2,t3,t4,t5", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.UnitVariabilityFactors, ScopingType.Foods, 8, "f1", "f2,f3,f4,f5,f6,f7,f8", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.UnitVariabilityFactors, ScopingType.Compounds, 6, "", "A,B,C,D,E,F", "");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Foods);
            AssertDataReadingSummaryRecord(report, ScopingType.ProcessingTypes, 0, "", "", "");
            AssertDataReadingSummaryRecord(report, ScopingType.Foods, 0, "", "", "f1");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Compounds);
            AssertDataReadingSummaryRecord(report, ScopingType.Compounds, 0, "", "", "");
        }

        [TestMethod]
        public void DataLinkingUnitVariabilityFactorsSimpleCompoundsFilterTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.UnitVariabilityFactors, @"UnitVariabilityFactorsTests\UnitVariabilityFactorsSimple")
            );
            _rawDataProvider.SetFilterCodes(ScopingType.Compounds, new[] { "B", "C" });

            _compiledLinkManager.LoadScope(SourceTableGroup.UnitVariabilityFactors);

            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.UnitVariabilityFactors);
            AssertDataLinkingSummaryRecord(report, ScopingType.UnitVariabilityFactors, ScopingType.ProcessingTypes, 5, "t1,t2,t3,t4,t5", "", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.UnitVariabilityFactors, ScopingType.Foods, 8, "f1,f2,f3,f4,f5,f6,f7", "f8", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.UnitVariabilityFactors, ScopingType.Compounds, 6, "B,C", "A,D,E,F", "");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Foods);
            AssertDataReadingSummaryRecord(report, ScopingType.ProcessingTypes, 0, "", "", "t1,t2,t3,t4,t5");
            AssertDataReadingSummaryRecord(report, ScopingType.Foods, 0, "", "", "f1,f2,f3,f4,f5,f6,f7");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Compounds);
            AssertDataReadingSummaryRecord(report, ScopingType.Compounds, 0, "", "", "B,C");
        }

        [TestMethod]
        public void DataLinkingUnitVariabilityFactorsFilterFoodsAndCompoundsSimpleTest() {
            _rawDataProvider.SetEmptyDataSource(SourceTableGroup.Foods);
            _rawDataProvider.SetEmptyDataSource(SourceTableGroup.Compounds);
            _rawDataProvider.SetEmptyDataSource(SourceTableGroup.Processing);
            _rawDataProvider.SetDataTables(
                (ScopingType.UnitVariabilityFactors, @"UnitVariabilityFactorsTests\UnitVariabilityFactorsSimple")
            );
            _rawDataProvider.SetFilterCodes(ScopingType.Foods, new[] { "f1" });
            _rawDataProvider.SetFilterCodes(ScopingType.Compounds, new[] { "B", "C" });

            _compiledLinkManager.LoadScope(SourceTableGroup.UnitVariabilityFactors);

            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.UnitVariabilityFactors);
            AssertDataLinkingSummaryRecord(report, ScopingType.UnitVariabilityFactors, ScopingType.ProcessingTypes, 5, "", "t1,t2,t3,t4,t5", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.UnitVariabilityFactors, ScopingType.Foods, 8, "f1", "f2,f3,f4,f5,f6,f7,f8", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.UnitVariabilityFactors, ScopingType.Compounds, 6, "B,C", "A,D,E,F", "");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Foods);
            AssertDataReadingSummaryRecord(report, ScopingType.ProcessingTypes, 0, "", "", "");
            AssertDataReadingSummaryRecord(report, ScopingType.Foods, 0, "", "", "f1");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Compounds);
            AssertDataReadingSummaryRecord(report, ScopingType.Compounds, 0, "", "", "B,C");
        }
    }
}
