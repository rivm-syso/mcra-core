using MCRA.Utils.Statistics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Utils.Test.UnitTests {

    [TestClass]
    public class CapturingGeneratorTests {

        [TestMethod]
        public void CapturingGenerator_Test1() {
            var refGenerator = new McraRandomGenerator(10, true);
            var cg = new CapturingGenerator(new McraRandomGenerator(10, true));
            var n = 10;
            var refNumbers = new List<double>();
            var actualNumbers = new List<double>();

            for (int i = 0; i < n; i++) {
                refNumbers.Add(refGenerator.NextDouble());
            }

            for (int i = 0; i < n; i++) {
                actualNumbers.Add(cg.NextDouble());
            }

            foreach (var numbers in refNumbers.Zip(actualNumbers, (r, a) => new { r, a })) {
                Assert.IsTrue(numbers.r == numbers.a);
            }
        }

        [TestMethod]
        public void CapturingGenerator_Test2() {
            var refGenerator = new McraRandomGenerator(10, true);
            var cg = new CapturingGenerator(new McraRandomGenerator(10, true));

            var n = 10;

            var refNumbers = new List<double>();
            var actualNumbers = new List<double>();

            for (int i = 0; i < 2 * n; i++) {
                refNumbers.Add(refGenerator.NextDouble());
            }

            cg.StartCapturing();
            for (int i = 0; i < n; i++) {
                actualNumbers.Add(cg.NextDouble());
            }

            cg.Repeat();
            for (int i = 0; i < n; i++) {
                Assert.IsFalse(cg.IsCapturing);
                Assert.IsTrue(cg.IsRepeating);
                actualNumbers.Add(cg.NextDouble());
            }

            cg.NextDouble();

            Assert.IsFalse(cg.IsRepeating);
            Assert.IsTrue(cg.IsCapturing);

            foreach (var numbers in refNumbers.Zip(actualNumbers, (r, a) => new { r, a }).Take(10)) {
                Assert.IsTrue(numbers.r == numbers.a);
            }
            foreach (var numbers in refNumbers.Zip(actualNumbers, (r, a) => new { r, a }).Skip(10)) {
                Assert.IsFalse(numbers.r == numbers.a);
            }
            foreach (var numbers in refNumbers.Take(10).Zip(actualNumbers.Skip(10), (r, a) => new { r, a })) {
                Assert.IsTrue(numbers.r == numbers.a);
            }
        }
    }
}
