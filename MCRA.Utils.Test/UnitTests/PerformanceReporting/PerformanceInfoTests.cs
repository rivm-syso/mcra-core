using MCRA.Utils.PerformanceReporting;

namespace MCRA.Utils.Test.UnitTests.PerformanceReporting {
    [TestClass]
    public class PerformanceInfoTests {
        [TestMethod]
        public void Test_GetTotalRam() {
            var totalMemory = PerformanceInfo.GetTotalMemory();
            Assert.IsGreaterThan(2UL * 1024, totalMemory);
        }
    }
}
