using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Utils.Test.UnitTests {
    [TestClass]
    public class DesignUtilsTests {
        [TestMethod]
        public void DesignUtils_AddConstantColumn1() {
            double[,] x = null;
            var m = DesignUtils.AddConstantColumn(x, 3);
            Assert.AreEqual(3, m.Length);
            Assert.AreEqual(1, m[0, 0]);
            Assert.AreEqual(1, m[1, 0]);
            Assert.AreEqual(1, m[2, 0]);
        }

        [TestMethod]
        public void DesignUtils_AddConstantColumn2() {
            double[,] x = new double[,] {
                { 1 },
                { 2 },
                { 3 },
            };
            var m = DesignUtils.AddConstantColumn(x, 3);
            Assert.AreEqual(2, m.GetLength(1));
            Assert.AreEqual(1, m[0, 0]);
            Assert.AreEqual(1, m[1, 0]);
            Assert.AreEqual(1, m[2, 0]);
        }

        [TestMethod]
        public void DesignUtils_AddConstantColumn3() {
            double[,] x = new double[,] {
                { 1 },
                { 2 },
                { 3 },
            };
            var m = DesignUtils.AddConstantColumn(x, 2);
            Assert.AreEqual(2, m.GetLength(1));
            Assert.AreEqual(1, m[0, 0]);
            Assert.AreEqual(1, m[1, 0]);
            Assert.AreEqual(1, m[2, 0]);
        }

        [TestMethod]
        public void DesignUtils_AddConstantColumn4() {
            double[,] x = new double[,] {
                { 1 },
                { 2 },
                { 3 },
            };
            var m = DesignUtils.AddConstantColumn(x, 4);
            Assert.AreEqual(2, m.GetLength(1));
            Assert.AreEqual(1, m[0, 0]);
            Assert.AreEqual(1, m[1, 0]);
            Assert.AreEqual(1, m[2, 0]);
        }
    }
}
