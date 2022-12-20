using MCRA.Utils.Statistics;
using MCRA.Simulation.OutputGeneration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.Generic {
    /// <summary>
    /// OutputGeneration, Generic, PercentilesPercentages
    /// </summary>
    [TestClass]
    public class IntakePercentageSectionTests : SectionTestBase {
        /// <summary>
        /// Summarize percentages acute
        /// </summary>
        [TestMethod]
        public void IntakePercentageSection_TestAcute1() {
            var mu = 110.5d;
            var sigma = 5;
            var numberOfIntakes = 1000;
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var intakes = NormalDistribution.Samples(random, mu, sigma, numberOfIntakes);
            var limits = new double[] { mu - 2 * sigma, mu };
            var section = new IntakePercentageSection();
            section.Summarize(intakes, null, null, limits);
            //Assert.AreEqual(2.5, section.Percentages[0].ReferenceValue, 2.5);
            //Assert.AreEqual(50, section.Percentages[1].ReferenceValue, 2.5);
            AssertIsValidView(section);
        }
        /// <summary>
        /// Summarize percentages acute
        /// </summary>
        [TestMethod]
        public void IntakePercentageSection_TestAcute2() {
            var mu = 110.5;
            var sigma = 5;
            var numberOfIntakes = 100;
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var intakes = NormalDistribution.Samples(random, mu, sigma, numberOfIntakes);
            var weights = Enumerable.Repeat(1.0, numberOfIntakes).ToList();
            var limits = new double[] { mu - 2 * sigma, mu };
            var section = new IntakePercentageSection();
            section.Summarize(intakes, weights, null, limits);
            AssertIsValidView(section);
        }
        /// <summary>
        /// Summarize percentages acute with uncertainty
        /// </summary>
        [TestMethod]
        public void IntakePercentageSection_TestAcuteUncertainty1() {
            var mu = 110.5;
            var sigma = 5D;
            var numberOfIntakes = 100;
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var intakes = NormalDistribution.Samples(random, mu, sigma, numberOfIntakes);
            var weights = Enumerable.Repeat(1.0, numberOfIntakes).ToList();
            var limits = new double[] { mu - 2 * sigma, mu };
            var section = new IntakePercentageSection();
            section.Summarize(intakes, weights, null, limits);
            for (int i = 0; i < 50; i++) {
                intakes = NormalDistribution.Samples(random, mu, sigma, numberOfIntakes);
                section.SummarizeUncertainty(intakes, weights, 2.5, 97.5);
            }
            //Assert.AreEqual(3.597, section.Percentages[5].ReferenceValue, 1E-3);
            //Assert.AreEqual(2.006, section.Percentages[5].MedianUncertainty, 1E-3);
            var percentageRecords = section.IntakePercentageRecords;
            Assert.AreEqual(limits.Count(), percentageRecords.Count);
            Assert.AreEqual(50, section.Percentages.First().UncertainValues.Count);

            AssertIsValidView(section);
        }
        /// <summary>
        /// Summarize percentages acute with uncertainty and ARfD
        /// </summary>
        [TestMethod]
        public void IntakePercentageSection_TestAcuteUncertaintyNoARFD() {
            var mu = 110.5;
            var sigma = 5;
            var numberOfIntakes = 100;
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var intakes = NormalDistribution.Samples(random, mu, sigma, numberOfIntakes);
            var weights = Enumerable.Repeat(1.0, numberOfIntakes).ToList();
            var limits = new double[] { mu - 2 * sigma, mu };
            var section = new IntakePercentageSection();
            section.Summarize(intakes, weights, null, limits);
            for (int i = 0; i < 50; i++) {
                intakes = NormalDistribution.Samples(random, mu, sigma, numberOfIntakes);
                section.SummarizeUncertainty(intakes, weights, 2.5, 97.5);
            }
            //Assert.AreEqual(3.597, section.Percentages[5].ReferenceValue, 1E-3);
            //Assert.AreEqual(2.006, section.Percentages[5].MedianUncertainty, 1E-3);
            var percentageRecords = section.IntakePercentageRecords;
            Assert.AreEqual(limits.Count(), percentageRecords.Count);
            Assert.AreEqual(50, section.Percentages.First().UncertainValues.Count);
            AssertIsValidView(section);
        }
        /// <summary>
        /// Summarize percentages acute with uncertainty, ARfD and safety factor
        /// </summary>
        [TestMethod]
        public void IntakePercentageSection_TestAcuteUncertaintyNoARFDandSafetyFactor() {
            var mu = 110.5;
            var sigma = 5;
            var numberOfIntakes = 100;
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var intakes = NormalDistribution.Samples(random, mu, sigma, numberOfIntakes);
            var weights = Enumerable.Repeat(1.0, numberOfIntakes).ToList();
            var limits = new double[] { mu - 2 * sigma, mu };
            var section = new IntakePercentageSection();
            section.Summarize(intakes, weights, null, limits);
            for (int i = 0; i < 50; i++) {
                intakes = NormalDistribution.Samples(random, mu, sigma, numberOfIntakes);
                section.SummarizeUncertainty(intakes, weights, 2.5, 97.5);
            }
            //Assert.AreEqual(3.597, section.Percentages[3].ReferenceValue, 1E-3);
            //Assert.AreEqual(2.006, section.Percentages[3].MedianUncertainty, 1E-3);
            var percentageRecords = section.IntakePercentageRecords;
            Assert.AreEqual(limits.Count(), percentageRecords.Count);
            Assert.AreEqual(50, section.Percentages.First().UncertainValues.Count);
            AssertIsValidView(section);
        }
    }
}
