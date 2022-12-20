using MCRA.General;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Data.Management.Test.UnitTests.DataManagement {
    [TestClass]
    public class DataLinkingTargetExposuresTests : LinkTestsBase {
        [TestMethod]
        public void DataLinkingTargetExposuresOnlyTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.TargetExposureModels, @"TargetExposuresTests\TargetExposureModels"),
                (ScopingType.TargetExposurePercentiles, @"TargetExposuresTests\TargetExposurePercentiles"),
                (ScopingType.TargetExposurePercentilesUncertain, @"TargetExposuresTests\TargetExposurePercentilesUncertain")
            );

            _compiledLinkManager.LoadScope(SourceTableGroup.TargetExposures);

            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.TargetExposures);
            AssertDataReadingSummaryRecord(report, ScopingType.TargetExposureModels, 2, "dem1,dem2", "", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.TargetExposureModels, ScopingType.Compounds, 1, "sub1", "", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.TargetExposurePercentiles, ScopingType.TargetExposureModels, 2, "dem1,dem2", "", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.TargetExposurePercentilesUncertain, ScopingType.TargetExposureModels, 2, "dem1,dem2", "", "");
        }
    }
}
