using MCRA.General;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace MCRA.Data.Management.Test.UnitTests.DataManagement {
    [TestClass]
    public class CompiledMarketSharesTests: CompiledTestsBase {

        [TestMethod]
        public void CompiledFoodsGetAllFoodsFromMarketSharesScopeTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.MarketShares, @"MarketSharesTests\MarketSharesSimple")
            );

            var marketShares = _compiledDataManager.GetAllMarketShares();
            Assert.AreEqual(3, marketShares.Count);
            Assert.AreEqual(20, marketShares.First(c => c.Food.Code=="A").Percentage);
            Assert.AreEqual(80, marketShares.First(c => c.Food.Code=="B").Percentage);
            Assert.AreEqual(0.1, marketShares.First(c => c.Food.Code == "C").Percentage);
            Assert.AreEqual(0, marketShares.First(c => c.Food.Code == "A").BrandLoyalty);
            Assert.AreEqual(1, marketShares.First(c => c.Food.Code == "B").BrandLoyalty);
            Assert.AreEqual(0.4, marketShares.First(c => c.Food.Code == "C").BrandLoyalty);
        }
    }
}
