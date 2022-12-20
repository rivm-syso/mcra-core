using MCRA.General.Action.Settings.Dto.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.General.Test.UnitTests.Action.Settings {
    [TestClass]
    public class McraVersionInfoTests {

        /// <summary>
        /// Tests version comparison method.
        /// </summary>
        [TestMethod]
        public void McraVersionInfo_TestCheckMinimalVersionNumber() {
            var info = new McraVersionInfo() {
                Major = 9,
                Minor = 1,
                Build = 10,
                Revision = 3
            };
            Assert.IsTrue(info.CheckMinimalVersionNumber(9, 1));
            Assert.IsTrue(info.CheckMinimalVersionNumber(9, 1, 9));
            Assert.IsTrue(info.CheckMinimalVersionNumber(9, 1, 10));
            Assert.IsFalse(info.CheckMinimalVersionNumber(9, 2));
            Assert.IsFalse(info.CheckMinimalVersionNumber(9, 2, 0));
            Assert.IsFalse(info.CheckMinimalVersionNumber(9, 1, 11));
            Assert.IsTrue(info.CheckMinimalVersionNumber(9, 0, 9));
            Assert.IsTrue(info.CheckMinimalVersionNumber(8, 3, 12));
            Assert.IsTrue(info.CheckMinimalVersionNumber(9, 1, 10, 0));
            Assert.IsTrue(info.CheckMinimalVersionNumber(9, 1, 10, 3));
            Assert.IsFalse(info.CheckMinimalVersionNumber(9, 1, 10, 4));
        }
    }
}
