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
        [DataRow(2.7, 3.701, 1000, 1000)]
        [DataRow(3.701, 2.7, 1000, 1000)]
        [DataRow(2.7, 2.7, 1000, 1000)]
        public void GriddingFunctions_TestArange_OutputSize(double min, double max, int n, int expected) {
            var actual = GriddingFunctions.Arange(min, max, n).ToList();
            Assert.HasCount(expected, actual);
            Assert.AreEqual(min, actual.First());
            Assert.AreEqual(max, actual.Last());
        }

        [TestMethod]
        public void GriddingFunctions_TestGetPlotPercentages() {
            var percentages = GriddingFunctions.GetPlotPercentages();
            Assert.HasCount(percentages.Distinct().Count(), percentages);
            Assert.IsTrue(percentages.All(r => r > 0 && r < 100));
        }
    }
}