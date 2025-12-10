using MCRA.Data.Management.Tests.UnitTests.DataManagement;
using MCRA.General;

namespace MCRA.Data.Management.Test.UnitTests.DataManagement {
    [TestClass]
    public class CompiledMarketSharesTests : CompiledTestsBase {

        [TestMethod]
        [DataRow(ManagerType.CompiledDataManager)]
        [DataRow(ManagerType.SubsetManager)]
        public void CompiledFoodsGetAllFoodsFromMarketSharesScopeTest(ManagerType managerType) {
            RawDataProvider.SetDataTables(
                (ScopingType.MarketShares, @"MarketSharesTests/MarketSharesSimple")
            );

            var marketShares = CompiledDataManager.GetAllMarketShares();
            Assert.HasCount(3, marketShares);
            Assert.AreEqual(20, marketShares.First(c => c.Food.Code == "A").Percentage);
            Assert.AreEqual(80, marketShares.First(c => c.Food.Code == "B").Percentage);
            Assert.AreEqual(0.1, marketShares.First(c => c.Food.Code == "C").Percentage);
            Assert.AreEqual(0, marketShares.First(c => c.Food.Code == "A").BrandLoyalty);
            Assert.AreEqual(1, marketShares.First(c => c.Food.Code == "B").BrandLoyalty);
            Assert.AreEqual(0.4, marketShares.First(c => c.Food.Code == "C").BrandLoyalty);
        }
    }
}
