using MCRA.General;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Data.Management.Test.UnitTests.DataManagement {
    [TestClass]
    public class DataLinkingRisksTests : LinkTestsBase {
        [TestMethod]
        public void DataLinkingRisksOnlyTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.RiskModels, @"RisksTests\RiskModels"),
                (ScopingType.RiskPercentiles, @"RisksTests\RiskPercentiles"),
                (ScopingType.RiskPercentilesUncertain, @"RisksTests\RiskPercentilesUncertain")
            );

            _compiledLinkManager.LoadScope(SourceTableGroup.Risks);

            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Risks);
            AssertDataReadingSummaryRecord(report, ScopingType.RiskModels, 2, "dem1,dem2", "", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.RiskModels, ScopingType.Compounds, 1, "sub1", "", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.RiskPercentiles, ScopingType.RiskModels, 2, "dem1,dem2", "", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.RiskPercentilesUncertain, ScopingType.RiskModels, 2, "dem1,dem2", "", "");
        }
    }
}
