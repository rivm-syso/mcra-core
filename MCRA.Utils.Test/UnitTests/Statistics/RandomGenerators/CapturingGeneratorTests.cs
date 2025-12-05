using MCRA.Utils.Statistics;
using MCRA.Utils.Statistics.RandomGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Utils.Test.UnitTests.Statistics.RandomGenerators {

    [TestClass]
    public class CapturingGeneratorTests {

        [TestMethod]
        public void CapturingGenerator_Test1() {
            var refGenerator = new McraRandomGenerator(10);
            var cg = new CapturingGenerator(new McraRandomGenerator(10));
            var n = 10;
            var refNumbers = new List<double>();
            var actualNumbers = new List<double>();

            for (var i = 0; i < n; i++) {
                refNumbers.Add(refGenerator.NextDouble());
            }

            for (var i = 0; i < n; i++) {
                actualNumbers.Add(cg.NextDouble());
            }

            foreach (var numbers in refNumbers.Zip(actualNumbers, (r, a) => new { r, a })) {
                Assert.AreEqual(numbers.a, numbers.r);
            }
        }

        [TestMethod]
        public void CapturingGenerator_Test2() {
            var refGenerator = new McraRandomGenerator(10);
            var cg = new CapturingGenerator(new McraRandomGenerator(10));

            var n = 10;

            var refNumbers = new List<double>();
            var actualNumbers = new List<double>();

            for (var i = 0; i < 2 * n; i++) {
                refNumbers.Add(refGenerator.NextDouble());
            }

            cg.StartCapturing();
            for (var i = 0; i < n; i++) {
                actualNumbers.Add(cg.NextDouble());
            }

            cg.Repeat();
            for (var i = 0; i < n; i++) {
                Assert.IsFalse(cg.IsCapturing);
                Assert.IsTrue(cg.IsRepeating);
                actualNumbers.Add(cg.NextDouble());
            }

            cg.NextDouble();

            Assert.IsFalse(cg.IsRepeating);
            Assert.IsTrue(cg.IsCapturing);

            foreach (var numbers in refNumbers.Zip(actualNumbers, (r, a) => new { r, a }).Take(10)) {
                Assert.AreEqual(numbers.a, numbers.r);
            }
            foreach (var numbers in refNumbers.Zip(actualNumbers, (r, a) => new { r, a }).Skip(10)) {
                Assert.AreNotEqual(numbers.a, numbers.r);
            }
            foreach (var numbers in refNumbers.Take(10).Zip(actualNumbers.Skip(10), (r, a) => new { r, a })) {
                Assert.AreEqual(numbers.a, numbers.r);
            }
        }
    }
}
