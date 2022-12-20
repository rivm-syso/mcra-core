using MCRA.Utils.Statistics;
using MCRA.General;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.DietaryExposures {

    /// <summary>
    /// OutputGeneration, ActionSummaries, DietaryExposures, ExposureByFoodCompound
    /// </summary>
    [TestClass()]
    public class TotalDistributionFoodCompoundSectionTests : SectionTestBase {

        /// <summary>
        /// Test summarize TotalDistributionFoodCompoundSection, acute with detailed food intakes.
        /// </summary>
        [TestMethod]
        public void TotalDistributionFoodCompoundSectionSummary_SummarizeAcuteDetailedFoodIntakes() {
            int seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = MockFoodsGenerator.Create(3);
            var substances = MockSubstancesGenerator.Create(3);
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(20, 2, true, random);
            var rpfs = substances.ToDictionary(r => r, r => 1d);
            var memberships = substances.ToDictionary(r => r, r => 1d);
            var exposures = MockDietaryIndividualDayIntakeGenerator.Create(individualDays, foods, substances, 0.5, true, random);

            var section = new TotalDistributionFoodCompoundSection();
            section.Summarize(exposures, rpfs, memberships, foods, substances, ExposureType.Acute, 2.5, 97.5, 2.5, 97.5, false);

            var sumContributions = section.Records.Sum(c => c.ContributionPercentage);
            Assert.AreEqual(100, sumContributions, 1e-1);
            AssertIsValidView(section);
        }

        /// <summary>
        /// Test summarize TotalDistributionFoodCompoundSection, acute with no RPFs.
        /// </summary>
        [TestMethod]
        public void TotalDistributionFoodCompoundSectionSummary_SummarizeAcuteNoRpfs() {
            int seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = MockFoodsGenerator.Create(3);
            var substances = MockSubstancesGenerator.Create(3);
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(20, 2, true, random);
            var exposures = MockDietaryIndividualDayIntakeGenerator.Create(individualDays, foods, substances, 0.5, true, random);
            var rpfs = substances.ToDictionary(r => r, r => 1d);
            var membershipProbabilities = substances.ToDictionary(r => r, r => 1d);
            var section = new TotalDistributionFoodCompoundSection();
            section.Summarize(exposures, rpfs, membershipProbabilities, foods, substances, ExposureType.Acute, 2.5, 97.5, 2.5, 97.5, false);

            Assert.AreEqual(foods.Count * substances.Count, section.Records.Count);
            Assert.IsTrue(section.Records.All(r => !double.IsNaN(r.Contribution)));
            AssertIsValidView(section);
        }

        /// <summary>
        /// Test summarize TotalDistributionFoodCompoundSection, acute with aggregate intakes.
        /// </summary>
        [TestMethod]
        public void TotalDistributionFoodCompoundSectionSummary_SummarizeAcuteAggregateIntakes() {
            int seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = MockFoodsGenerator.Create(3);
            var substances = MockSubstancesGenerator.Create(3);
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(20, 2, true, random);
            var rpfs = substances.ToDictionary(r => r, r => 1d);
            var memberships = substances.ToDictionary(r => r, r => 1d);
            var exposures = MockDietaryIndividualDayIntakeGenerator.GenerateImputed(individualDays, foods, substances, 0.5, true, random);

            var section = new TotalDistributionFoodCompoundSection();
            section.Summarize(exposures, rpfs, memberships, foods, substances, ExposureType.Acute, 2.5, 97.5, 2.5, 97.5, false);

            var sumContributions = section.Records.Sum(c => c.ContributionPercentage);
            Assert.AreEqual(100, sumContributions, 1e-1);
            AssertIsValidView(section);
        }

        /// <summary>
        /// Test summarize uncertainty run TotalDistributionFoodCompoundSection, acute with aggregate intakes.
        /// </summary>
        [TestMethod]
        public void TotalDistributionFoodCompoundSectionSummary_SummarizeUncertaintyAcute() {
            int seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = MockFoodsGenerator.Create(3);
            var substances = MockSubstancesGenerator.Create(3);
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(20, 2, true, random);
            var rpfs = substances.ToDictionary(r => r, r => 1d);
            var memberships = substances.ToDictionary(r => r, r => 1d);
            var exposures = MockDietaryIndividualDayIntakeGenerator.GenerateImputed(individualDays, foods, substances, 0.5, true, random);

            var section = new TotalDistributionFoodCompoundSection();
            section.Summarize(exposures, rpfs, memberships, foods, substances, ExposureType.Acute, 2.5, 97.5, 2.5, 97.5, false);
            section.SummarizeUncertainty(exposures, rpfs, memberships, substances, ExposureType.Acute, false);

            var sumContributions = section.Records.Sum(c => c.ContributionPercentage);
            Assert.AreEqual(100, sumContributions, 1e-1);
            foreach (var record in section.Records) {
                Assert.AreEqual(1, record.Contributions.Count);
            }

            AssertIsValidView(section);
        }

        /// <summary>
        /// Test summarize TotalDistributionFoodCompoundSection, chronic with detailed food intakes.
        /// </summary>
        [TestMethod]
        public void TotalDistributionFoodCompoundSectionSummary_SummarizeChronicDetailedFoodIntakes() {
            int seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = MockFoodsGenerator.Create(3);
            var substances = MockSubstancesGenerator.Create(3);
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(20, 2, true, random);
            var rpfs = substances.ToDictionary(r => r, r => 1d);
            var memberships = substances.ToDictionary(r => r, r => 1d);
            var exposures = MockDietaryIndividualDayIntakeGenerator.Create(individualDays, foods, substances, 0.5, true, random);

            var section = new TotalDistributionFoodCompoundSection();
            section.Summarize(exposures, rpfs, memberships, foods, substances, ExposureType.Chronic, 2.5, 97.5, 2.5, 97.5, false);

            var sumContributions = section.Records.Sum(c => c.ContributionPercentage);
            Assert.AreEqual(100, sumContributions, 1e-1);
            AssertIsValidView(section);
        }

        /// <summary>
        /// Test summarize TotalDistributionFoodCompoundSection, chronic without RPFs.
        /// </summary>
        [TestMethod]
        public void TotalDistributionFoodCompoundSectionSummary_SummarizeChronicNoRpfs() {
            int seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = MockFoodsGenerator.Create(3);
            var substances = MockSubstancesGenerator.Create(3);
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(20, 2, true, random);
            var exposures = MockDietaryIndividualDayIntakeGenerator.Create(individualDays, foods, substances, 0.5, true, random);

            var section = new TotalDistributionFoodCompoundSection();
            section.Summarize(exposures, null, null, foods, substances, ExposureType.Chronic, 2.5, 97.5, 2.5, 97.5, false);

            Assert.AreEqual(foods.Count * substances.Count, section.Records.Count);
            Assert.IsTrue(section.Records.All(r => double.IsNaN(r.Contribution)));
            AssertIsValidView(section);
        }

        /// <summary>
        /// Test summarize TotalDistributionFoodCompoundSection, chronic with aggregate intakes.
        /// </summary>
        [TestMethod]
        public void TotalDistributionFoodCompoundSectionSummary_SummarizeChronicAggregateIntakes() {
            int seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = MockFoodsGenerator.Create(3);
            var substances = MockSubstancesGenerator.Create(3);
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(20, 2, true, random);
            var rpfs = substances.ToDictionary(r => r, r => 1d);
            var memberships = substances.ToDictionary(r => r, r => 1d);
            var exposures = MockDietaryIndividualDayIntakeGenerator.GenerateImputed(individualDays, foods, substances, 0.5, true, random);

            var section = new TotalDistributionFoodCompoundSection();
            section.Summarize(exposures, rpfs, memberships, foods, substances, ExposureType.Chronic, 2.5, 97.5, 2.5, 97.5, false);

            var sumContributions = section.Records.Sum(c => c.ContributionPercentage);
            Assert.AreEqual(100, sumContributions, 1e-1);
            AssertIsValidView(section);
        }

        /// <summary>
        /// Test summarize uncertainty run TotalDistributionFoodCompoundSection, chronic with aggregate intakes.
        /// </summary>
        [TestMethod]
        public void TotalDistributionFoodCompoundSectionSummary_SummarizeUncertaintyChronic() {
            int seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = MockFoodsGenerator.Create(3);
            var substances = MockSubstancesGenerator.Create(3);
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(20, 2, true, random);
            var rpfs = substances.ToDictionary(r => r, r => 1d);
            var memberships = substances.ToDictionary(r => r, r => 1d);
            var exposures = MockDietaryIndividualDayIntakeGenerator.GenerateImputed(individualDays, foods, substances, 0.5, true, random);

            var section = new TotalDistributionFoodCompoundSection();
            section.Summarize(exposures, rpfs, memberships, foods, substances, ExposureType.Chronic, 2.5, 97.5, 2.5, 97.5, false);
            section.SummarizeUncertainty(exposures, rpfs, memberships, substances, ExposureType.Chronic, false);

            var sumContributions = section.Records.Sum(c => c.ContributionPercentage);
            Assert.AreEqual(100, sumContributions, 1e-1);
            foreach (var record in section.Records) {
                Assert.AreEqual(1, record.Contributions.Count);
            }

            AssertIsValidView(section);
        }
    }
}
