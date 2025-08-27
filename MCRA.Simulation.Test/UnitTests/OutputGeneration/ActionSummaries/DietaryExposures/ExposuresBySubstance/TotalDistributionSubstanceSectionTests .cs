using MCRA.General;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.DietaryExposures {
    /// <summary>
    /// OutputGeneration, ActionSummaries, TargetExposures, ExposureByCompound, ByCompound
    /// </summary>
    [TestClass]
    public class TotalDistributionSubstanceSectionTests : SectionTestBase {
        /// <summary>
        /// Summarize (uncertainty) acute dietary, test TotalDistributionCompoundSection view
        /// </summary>
        [TestMethod]
        public void TotalDistributionCompoundSection_TestDietaryAcute() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = FakeFoodsGenerator.MockFoods("Apple", "Pear", "Bananas");
            var substances = FakeSubstancesGenerator.Create(3);
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(20, 2, true, random);
            var rpfs = substances.ToDictionary(r => r, r => 1d);
            var memberships = substances.ToDictionary(r => r, r => 1d);
            var exposures = FakeDietaryIndividualDayIntakeGenerator.Create(individualDays, foods, substances, 0.5, true, random);

            var section = new TotalDistributionCompoundSection();
            section.Summarize(exposures, substances, rpfs, memberships, ExposureType.Acute, 25, 75, 2.5, 97.5, false);
            Assert.AreEqual(100D, section.Records.Sum(c => c.ContributionPercentage), .001);
            Assert.AreEqual(substances.Count, section.Records.Count);

            section.SummarizeUncertainty(exposures, substances, rpfs, memberships, ExposureType.Acute, false);
            Assert.AreEqual(3, section.Records.SelectMany(c => c.Contributions).Count());
            AssertIsValidView(section);
        }
        /// <summary>
        /// Summarize (uncertainty) chronic dietary, test TotalDistributionCompoundSection view
        /// </summary>
        [TestMethod]
        public void TotalDistributionCompoundSection_TestDietaryChronic() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = FakeFoodsGenerator.MockFoods("Apple", "Pear", "Bananas");
            var substances = FakeSubstancesGenerator.Create(3);
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(20, 2, true, random);
            var rpfs = substances.ToDictionary(r => r, r => 1d);
            var memberships = substances.ToDictionary(r => r, r => 1d);
            var exposures = FakeDietaryIndividualDayIntakeGenerator.Create(individualDays, foods, substances, 0.5, true, random);

            var section = new TotalDistributionCompoundSection();
            section.Summarize(exposures, substances, rpfs, memberships, ExposureType.Chronic, 25, 75, 2.5, 97.5, false);

            Assert.AreEqual(100D, section.Records.Sum(c => c.ContributionPercentage), .001);
            Assert.AreEqual(substances.Count, section.Records.Count);

            section.SummarizeUncertainty(exposures, substances, rpfs, memberships, ExposureType.Chronic, false);
            Assert.AreEqual(3, section.Records.SelectMany(c => c.Contributions).Count());
        }
    }
}
