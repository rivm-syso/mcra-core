using MCRA.Utils.PerformanceReporting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Utils.Test.UnitTests.PerformanceReporting {
    [TestClass]
    public class PerformanceInfoTests {
        [TestMethod]
        public void Test_GetTotalRam() {
            var totalMemory = PerformanceInfo.GetTotalMemory();
            Assert.IsTrue(totalMemory > (2L * 1024));
        }
    }
}
