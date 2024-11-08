using MCRA.General;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using MCRA.Simulation.OutputGeneration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.DietaryExposures {
    /// <summary>
    /// OutputGeneration, ActionSummaries, DietaryExposures, ExposureByFood, FoodAsMeasured
    /// </summary>
    [TestClass()]
    public class UpperDistributionTDSFoodAsMeasuredSectionTests : SectionTestBase {
        /// <summary>
        /// With imputation, chronic test UpperDistributionTDSFoodAsMeasuredSection view
        /// </summary>
        [TestMethod]
        public void UpperDistributionTDSFoodSectionSummary_SummarizeChronic() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = FakeFoodsGenerator.MockFoods("Apple", "Pear", "Bananas");
            var substances = FakeSubstancesGenerator.Create(3);
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(20, 2, true, random);
            var rpfs = substances.ToDictionary(r => r, r => 1d);
            var memberships = substances.ToDictionary(r => r, r => 1d);
            var exposures = FakeDietaryIndividualDayIntakeGenerator.GenerateImputed(individualDays, foods, substances, 0.5, true, random);

            var section = new UpperDistributionTDSFoodAsMeasuredSection();
            section.Summarize(exposures, substances, rpfs, memberships, ExposureType.Chronic, 2.5, 97.5, 97.5, false);
            Assert.AreEqual("All TDS samples", section.Records.First().FoodName);
            AssertIsValidView(section);
        }

        /// <summary>
        /// With imputation, chronic test UpperDistributionTDSFoodAsMeasuredSection view
        /// </summary>
        [TestMethod]
        public void UpperDistributionTDSFoodSectionSummary_SummarizeAcute() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = FakeFoodsGenerator.MockFoods("Apple", "Pear", "Bananas");
            var substances = FakeSubstancesGenerator.Create(3);
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(20, 2, true, random);
            var rpfs = substances.ToDictionary(r => r, r => 1d);
            var memberships = substances.ToDictionary(r => r, r => 1d);
            var exposures = FakeDietaryIndividualDayIntakeGenerator.GenerateImputed(individualDays, foods, substances, 0.5, true, random);

            var section = new UpperDistributionTDSFoodAsMeasuredSection();
            section.Summarize(exposures, substances, rpfs, memberships, ExposureType.Acute, 2.5, 97.5, 97.5, false);
            Assert.AreEqual("All TDS samples", section.Records.First().FoodName);
            AssertIsValidView(section);
        }
    }
}
