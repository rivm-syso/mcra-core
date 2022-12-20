using MCRA.General;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Data.Management.Test.UnitTests.DataManagement {
    [TestClass]
    public class DataLinkingTotalDietStudyTests : LinkTestsBase {

        [TestMethod]
        public void DataLinkingTdsFoodSampleCompositionsSimpleTest() {
            _rawDataProvider.SetEmptyDataSource(SourceTableGroup.Foods);
            _rawDataProvider.SetEmptyDataSource(SourceTableGroup.Compounds);
            _rawDataProvider.SetEmptyDataSource(SourceTableGroup.Concentrations);
            _rawDataProvider.SetDataTables(
                (ScopingType.TdsFoodSampleCompositions, @"TotalDietStudyTests\TDSFoodSampleCompositionsSimple")
            );

            _compiledLinkManager.LoadScope(SourceTableGroup.TotalDietStudy);

            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.TotalDietStudy);
            AssertDataLinkingSummaryRecord(report, ScopingType.TdsFoodSampleCompositions, ScopingType.Foods, 10, "f1,f2,f4,f5,f6,t1,t2,t3,t4,t5", "", "");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Foods);
            AssertDataReadingSummaryRecord(report, ScopingType.Foods, 0, "", "", "f1,f2,f4,f5,f6,t1,t2,t3,t4,t5");
        }

        [TestMethod]
        public void DataLinkingTdsFoodSampleCompositionsSimpleFoodFilterTest() {
            _rawDataProvider.SetEmptyDataSource(SourceTableGroup.Foods);
            _rawDataProvider.SetEmptyDataSource(SourceTableGroup.Compounds);
            _rawDataProvider.SetEmptyDataSource(SourceTableGroup.Concentrations);
            _rawDataProvider.SetDataTables(
                (ScopingType.TdsFoodSampleCompositions, @"TotalDietStudyTests\TDSFoodSampleCompositionsSimple")
            );
            _rawDataProvider.SetFilterCodes(ScopingType.Foods, new[] { "f1","f2","t1","t2" });

            _compiledLinkManager.LoadScope(SourceTableGroup.TotalDietStudy);

            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.TotalDietStudy);
            AssertDataLinkingSummaryRecord(report, ScopingType.TdsFoodSampleCompositions, ScopingType.Foods, 10, "f1,f2,t1,t2", "f4,f5,f6,t3,t4,t5", "");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Foods);
            AssertDataReadingSummaryRecord(report, ScopingType.Foods, 0, "", "", "f1,f2,t1,t2");
        }
    }
}
