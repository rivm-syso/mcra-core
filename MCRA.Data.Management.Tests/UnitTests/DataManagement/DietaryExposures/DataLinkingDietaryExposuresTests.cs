using MCRA.General;

namespace MCRA.Data.Management.Test.UnitTests.DataManagement {
    [TestClass]
    public class DataLinkingDietaryExposuresTests : LinkTestsBase {
        [TestMethod]
        public void DataLinkingDietaryExposuresOnlyTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.DietaryExposureModels, @"DietaryExposuresTests/DietaryExposureModels"),
                (ScopingType.DietaryExposurePercentiles, @"DietaryExposuresTests/DietaryExposurePercentiles"),
                (ScopingType.DietaryExposurePercentilesUncertain, @"DietaryExposuresTests/DietaryExposurePercentilesUncertain")
            );

            _compiledLinkManager.LoadScope(SourceTableGroup.DietaryExposures);

            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.DietaryExposures);
            AssertDataReadingSummaryRecord(report, ScopingType.DietaryExposureModels, 2, "dem1,dem2", "", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.DietaryExposureModels, ScopingType.Compounds, 1, "sub1", "", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.DietaryExposurePercentiles, ScopingType.DietaryExposureModels, 2, "dem1,dem2", "", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.DietaryExposurePercentilesUncertain, ScopingType.DietaryExposureModels, 2, "dem1,dem2", "", "");
        }
    }
}
