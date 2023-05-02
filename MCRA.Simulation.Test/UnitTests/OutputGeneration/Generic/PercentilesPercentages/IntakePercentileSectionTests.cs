using MCRA.Utils.Statistics;
using MCRA.Simulation.OutputGeneration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration {
    /// <summary>
    /// OutputGeneration, Generic, PercentilesPercentages
    /// </summary>
    [TestClass]
    public class IntakePercentileSectionTests : SectionTestBase {

        /// <summary>
        /// Summarize acute, test IntakePercentileSection view
        /// </summary>
        [TestMethod]
        public void IntakePercentileSection_TestAcute() {
            var mu = 110.5;
            var sigma = 5;
            var numberOfIntakes = 100;
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var intakes = NormalDistribution.Samples(random, mu, sigma, numberOfIntakes);
            var weights = Enumerable.Repeat(1.0, numberOfIntakes).ToList();
            var percentages = new double[] { 50, 90, 95, 99, 99.5, 99.9 };
            var section = new IntakePercentileSection();
            section.Summarize(intakes, weights, null, percentages);
            Assert.AreEqual(110.424, section.MeanOfExposure.ReferenceValues.First(), 1E-3);
            AssertIsValidView(section);
        }

        /// <summary>
        /// Summarize  acute, test IntakePercentileSection view
        /// </summary>
        [TestMethod]
        public void IntakePercentileSection_TestAcuteNoWeights() {
            var mu = 110.5;
            var sigma = 5;
            var numberOfIntakes = 100;
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var intakes = NormalDistribution.Samples(random, mu, sigma, numberOfIntakes);
            var percentages = new double[] { 50, 90, 95, 99, 99.5, 99.9 };
            var section = new IntakePercentileSection();
            section.Summarize(intakes, null, null, percentages);
            Assert.AreEqual(intakes.Average(), section.MeanOfExposure.ReferenceValues.First(), double.Epsilon);
            AssertIsValidView(section);
        }

        /// <summary>
        /// Summarize acute, uncertainty, test IntakePercentileSection view
        /// </summary>
        [TestMethod]
        public void IntakePercentileSection_TestAcuteUncertainty() {
            var mu = 110.5;
            var sigma = 5;
            var numberOfIntakes = 100;
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var intakes = NormalDistribution.Samples(random, mu, sigma, numberOfIntakes);
            var weights = Enumerable.Repeat(1.0, numberOfIntakes).ToList();
            var percentages = new double[] { 50, 90, 95, 99, 99.5, 99.9 };
            var section = new IntakePercentileSection();
            section.Summarize(intakes, weights, null, percentages);
            Assert.AreEqual(intakes.Average(), section.MeanOfExposure.ReferenceValues.First(), double.Epsilon);
            for (int i = 0; i < 50; i++) {
                intakes = NormalDistribution.Samples(random, mu, sigma, numberOfIntakes);
                section.SummarizeUncertainty(intakes, weights, 2.5, 97.5);
            }
            Assert.AreEqual(110.552, section.Percentiles.First().MedianUncertainty, 1E-3);
            Assert.AreEqual(50, section.Percentiles.First().UncertainValues.Count);

            var bootstrapRecords = section.GetPercentileBootstrapRecords(false);
            Assert.AreEqual(percentages.Count() * 50, bootstrapRecords.Count);
            Assert.IsNotNull(section.IntakePercentileRecords);
            AssertIsValidView(section);
        }

        /// <summary>
        /// Summarize acute, uncertainty, test IntakePercentileSection view
        /// </summary>
        [TestMethod]
        public void IntakePercentileSection_TestAcuteUncertaintyNoARFD() {
            var mu = 110.5;
            var sigma = 5;
            var numberOfIntakes = 100;
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var intakes = NormalDistribution.Samples(random, mu, sigma, numberOfIntakes);
            var weights = Enumerable.Repeat(1.0, numberOfIntakes).ToList();
            var percentages = new double[] { 50, 90, 95, 99, 99.5, 99.9 };
            var section = new IntakePercentileSection();
            section.Summarize(intakes, weights, null, percentages);
            Assert.AreEqual(intakes.Average(), section.MeanOfExposure.ReferenceValues.First(), double.Epsilon);
            for (int i = 0; i < 50; i++) {
                intakes = NormalDistribution.Samples(random, mu, sigma, numberOfIntakes);
                section.SummarizeUncertainty(intakes, weights, 2.5, 97.5);
            }
            Assert.AreEqual(110.552, section.Percentiles.First().MedianUncertainty, 1E-3);
            Assert.AreEqual(50, section.Percentiles.First().UncertainValues.Count);
            var bootstrapRecords = section.GetPercentileBootstrapRecords(true);
            Assert.AreEqual(percentages.Count() * 50 + percentages.Count(), bootstrapRecords.Count);
            var percentileRecords = section.IntakePercentileRecords;
            AssertIsValidView(section);
        }

        /// <summary>
        /// Summarize acute, ARfD and safety factor, test IntakePercentileSection view
        /// </summary>
        [TestMethod]
        public void IntakePercentileSection_TestAcuteUncertaintyNoARFDandSafetyFactor() {
            var mu = 110.5;
            var sigma = 5;
            var numberOfIntakes = 100;
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var intakes = NormalDistribution.Samples(random, mu, sigma, numberOfIntakes);
            var weights = Enumerable.Repeat(1.0, numberOfIntakes).ToList();
            var percentages = new double[] { 50, 90, 95, 99, 99.5, 99.9 };
            var section = new IntakePercentileSection();
            section.Summarize(intakes, weights, null, percentages);
            Assert.AreEqual(intakes.Average(), section.MeanOfExposure.ReferenceValues.First(), double.Epsilon);
            for (int i = 0; i < 50; i++) {
                intakes = NormalDistribution.Samples(random, mu, sigma, numberOfIntakes);
                section.SummarizeUncertainty(intakes, weights, 2.5, 97.5);
            }
            Assert.AreEqual(110.552, section.Percentiles.First().MedianUncertainty, 1E-3);
            Assert.AreEqual(50, section.Percentiles.First().UncertainValues.Count);
            var bootstrapRecords = section.GetPercentileBootstrapRecords(true);
            Assert.AreEqual(percentages.Count() * 50 + percentages.Count(), bootstrapRecords.Count);
            var percentileRecords = section.IntakePercentileRecords;
            AssertIsValidView(section);
        }
    }
}
