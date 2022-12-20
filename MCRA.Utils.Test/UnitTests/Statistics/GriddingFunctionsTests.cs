using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Utils.Test.UnitTests {
    [TestClass]
    public class GriddingFunctionsTests {

        [TestMethod]
        public void GriddingFunctions_TestLogspace1() {
            var expected = new double[] { 1, 10, 100, 1000, 10000 };
            var actual = GriddingFunctions.LogSpace(1, 10000, 5).ToArray();
            Assert.AreEqual(expected[0], actual[0], 1e-4);
            Assert.AreEqual(expected[4], actual[4], 1e-4);
        }

        [TestMethod]
        public void GriddingFunctions_TestLogspace2() {
            var actual = GriddingFunctions.LogSpacePercentage(0, 100, 100).ToArray();
            Assert.AreEqual(0.01, actual[0], 1e-4);
            Assert.AreEqual(100, actual[99], 1e-4);
        }

        [TestMethod]
        public void GriddingFunctions_TestLogspace3() {
            var actual = GriddingFunctions.LogSpacePercentage(-100, 100, 100).ToArray();
            Assert.AreEqual(0.01, actual[0], 1e-4);
            Assert.AreEqual(100, actual[99], 1e-4);
        }

        [TestMethod]
        public void GriddingFunctions_TestArange() {
            var expected = new double[] { 1, 2, 3, 4, 5 };
            var actual = GriddingFunctions.Arange(1, 5, 5).ToArray();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void GriddingFunctions_TestGetPlotPercentages() {
            var percentages = GriddingFunctions.GetPlotPercentages();
            Assert.IsTrue(percentages.Distinct().Count() == percentages.Length);
            Assert.IsTrue(percentages.All(r => r > 0 && r < 100));
        }
    }
}
