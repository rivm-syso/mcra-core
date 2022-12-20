using MCRA.General;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Data.Management.Test.UnitTests.DataManagement {
    [TestClass]
    public class DataLinkingMarketSharesTests : LinkTestsBase {

        [TestMethod]
        public void DataLinkingMarketShares_TestSimple() {
            _rawDataProvider.SetDataTables(
                (ScopingType.MarketShares, @"MarketSharesTests\MarketSharesSimple")
            );

            _compiledLinkManager.LoadScope(SourceTableGroup.MarketShares);
            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.MarketShares);
            AssertDataLinkingSummaryRecord(report, ScopingType.MarketShares, ScopingType.Foods, 3, "A,B,C", "", "");
        }
    }
}
