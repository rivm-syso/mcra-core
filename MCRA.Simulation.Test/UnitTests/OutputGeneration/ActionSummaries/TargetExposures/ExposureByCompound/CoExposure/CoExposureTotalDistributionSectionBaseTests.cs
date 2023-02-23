using MCRA.Utils.Collections;
using MCRA.Utils.Statistics;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.TargetExposures {
    /// <summary>
    /// OutputGeneration, ActionSummaries, TargetExposures, ExposuresByCompound, CoExposure
    /// </summary>
    [TestClass]
    public class CoExposureTotalDistributionSectionBaseTests {
        /// <summary>
        /// Test exposure patterns
        /// </summary>
        [TestMethod]
        public void CoExposureDistributionSectionBase_TestGetExposurePatternFrequencies1() {
            var substances = MockSubstancesGenerator.Create(3);
            var numExposures = 10000;
            var patterns = new List<BitPattern32>() {
                new BitPattern32("000"),
                new BitPattern32("001"),
                new BitPattern32("011"),
            };

            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var substanceIndividualExposures = Enumerable.Range(0, numExposures)
                .Select(r => patterns[random.Next(0, patterns.Count)])
                .ToList();

            var dcsb = new CoExposureDistributionSectionBase();
            var rawGroupedExposurePatterns = dcsb.CalculateGroupedExposuresPatterns(substanceIndividualExposures, substances);
            var fullExposureRecords = dcsb.GetGroupedExposurePatterns(rawGroupedExposurePatterns);

            // For the full exposure records, all frequencies should sum up to 100 percent and the total number of exposures.
            Assert.AreEqual(100D, fullExposureRecords.Sum(r => r.Percentage), 1e-5);
            Assert.AreEqual(numExposures, fullExposureRecords.Sum(r => r.Frequency));

            var recordZero = fullExposureRecords.First(r => r.NumberOfSubstances == 0);
            var recordSingle = fullExposureRecords.First(r => r.NumberOfSubstances == 1);

            // All records with at least one substance include the single substance percentage
            Assert.AreEqual(100D - recordZero.Percentage, 66.27, 1e-1);
        }

        /// <summary>
        /// Summarize co-exposures based on binary patterns
        /// </summary>
        [TestMethod]
        public void CoExposureDistributionSectionBase_TestGetExposurePatternFrequencies2() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var nCompounds = 10;
            var substances = MockSubstancesGenerator.Create(nCompounds);
            var numExposures = 10000;
            var numPatterns = 20;

            var zeroPattern = new BitPattern32(nCompounds);
            var patterns = Enumerable
                .Range(0, numPatterns - 1)
                .Select(r => new BitPattern32(Enumerable.Range(0, nCompounds).Select((c, ix) => ix == 0 || random.NextDouble() > .5).ToArray()))
                .ToList();
            patterns.Insert(0, zeroPattern);

            var substanceIndividualExposures = Enumerable.Range(0, numExposures)
                .Select(r => patterns[random.Next(0, patterns.Count())])
                .ToList();

            var dcsb = new CoExposureDistributionSectionBase();
            var rawGroupedExposurePatterns = dcsb.CalculateGroupedExposuresPatterns(substanceIndividualExposures, substances);
            var fullExposureRecords = dcsb.GetGroupedExposurePatterns(rawGroupedExposurePatterns);

            // For the full exposure records, all frequencies should sum up to 100 percent and the total number of exposures.
            Assert.AreEqual(100D, fullExposureRecords.Sum(r => r.Percentage), 1e-5);
            Assert.AreEqual(numExposures, fullExposureRecords.Sum(r => r.Frequency));
            Assert.AreEqual(substanceIndividualExposures.Distinct().Count(), fullExposureRecords.Count);
        }
    }
}
