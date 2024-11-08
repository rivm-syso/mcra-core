using MCRA.General;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using MCRA.Simulation.OutputGeneration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.DietaryExposures {
    /// <summary>
    /// OutputGeneration, ActionSummaries, DietaryExposures, ExposureByFood, FoodAsEaten
    /// </summary>
    [TestClass()]
    public class UpperDistributionFoodAsEatenSectionTests : SectionTestBase {
        /// <summary>
        /// No imputation, acute, test UpperDistributionFoodAsEatenSection view
        /// </summary>
        [TestMethod]
        public void UpperDistributionFoodAsEatenSectionSummary_SummarizeAcute1() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = FakeFoodsGenerator.MockFoods("Apple", "Pear", "Bananas");
            var compounds = FakeSubstancesGenerator.Create(3);
            var individuals = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(20, 2, true, random);
            var rpfs = compounds.ToDictionary(r => r, r => 1d);
            var memberships = compounds.ToDictionary(r => r, r => 1d);
            var exposures = FakeDietaryIndividualDayIntakeGenerator.Create(individuals, foods, compounds, 0.5, true, random);
            var section = new UpperDistributionFoodAsEatenSection();
            section.Summarize(exposures, rpfs, memberships, foods, ExposureType.Acute, 2.5, 97.5, 2.5, 97.5, 97.5, false);
            Assert.IsTrue(section.Records.Sum(c => c.ContributionPercentage) > 99.99 && section.Records.Sum(c => c.ContributionPercentage) < 100.01);
            AssertIsValidView(section);
        }

        /// <summary>
        /// With imputation, acute, test UpperDistributionFoodAsEatenSection view
        /// </summary>
        [TestMethod]
        public void UpperDistributionFoodAsEatenSectionSummary_SummarizeAcute2() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = FakeFoodsGenerator.MockFoods("Apple", "Pear", "Bananas");
            var compounds = FakeSubstancesGenerator.Create(3);
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(20, 2, true, random);
            var rpfs = compounds.ToDictionary(r => r, r => 1d);
            var memberships = compounds.ToDictionary(r => r, r => 1d);
            var exposures = FakeDietaryIndividualDayIntakeGenerator.GenerateImputed(individualDays, foods, compounds, 0.5, true, random);

            var section = new UpperDistributionFoodAsEatenSection();
            section.Summarize(exposures, rpfs, memberships, foods, ExposureType.Acute, 2.5, 97.5, 2.5, 97.5, 97.5, false);
            Assert.IsTrue(section.Records.Sum(c => c.ContributionPercentage) > 99.99 && section.Records.Sum(c => c.ContributionPercentage) < 100.01);
            AssertIsValidView(section);
        }

        /// <summary>
        /// With imputation, acute, test UpperDistributionFoodAsEatenSection view
        /// </summary>
        [TestMethod]
        public void UpperDistributionFoodAsEatenSectionSummary_SummarizeUncertaintyAcute1() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = FakeFoodsGenerator.MockFoods("Apple", "Pear", "Bananas");
            var compounds = FakeSubstancesGenerator.Create(3);
            var individuals = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(20, 2, true, random);
            var rpfs = compounds.ToDictionary(r => r, r => 1d);
            var memberships = compounds.ToDictionary(r => r, r => 1d);
            var exposures = FakeDietaryIndividualDayIntakeGenerator.GenerateImputed(individuals, foods, compounds, 0.5, true, random);

            var section = new UpperDistributionFoodAsEatenSection();
            section.Summarize(exposures, rpfs, memberships, foods, ExposureType.Acute, 2.5, 97.5, 2.5, 97.5, 97.5, false);
            section.SummarizeUncertainty(exposures, rpfs, memberships, ExposureType.Acute, 97.5, false);
            Assert.IsTrue(section.Records.Sum(c => c.ContributionPercentage) > 99.99 && section.Records.Sum(c => c.ContributionPercentage) < 100.01);
            AssertIsValidView(section);
        }

        /// <summary>
        /// No imputation, chronic, test UpperDistributionFoodAsEatenSection view
        /// </summary>
        [TestMethod]
        public void UpperDistributionFoodAsEatenSectionSummary_SummarizeChronic1() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = FakeFoodsGenerator.MockFoods("Apple", "Pear", "Bananas");
            var compounds = FakeSubstancesGenerator.Create(3);
            var individuals = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(20, 2, true, random);
            var rpfs = compounds.ToDictionary(r => r, r => 1d);
            var memberships = compounds.ToDictionary(r => r, r => 1d);
            var exposures = FakeDietaryIndividualDayIntakeGenerator.Create(individuals, foods, compounds, 0.5, true, random);

            var section = new UpperDistributionFoodAsEatenSection();
            section.Summarize(exposures, rpfs, memberships, foods, ExposureType.Chronic, 2.5, 97.5, 2.5, 97.5, 97.5, false);
            Assert.IsTrue(section.Records.Sum(c => c.ContributionPercentage) > 99.99 && section.Records.Sum(c => c.ContributionPercentage) < 100.01);
            AssertIsValidView(section);
        }

        /// <summary>
        /// With imputation, chronic, test UpperDistributionFoodAsEatenSection view
        /// </summary>
        [TestMethod]
        public void UpperDistributionFoodAsEatenSectionSummary_SummarizeChronic2() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = FakeFoodsGenerator.MockFoods("Apple", "Pear", "Bananas");
            var compounds = FakeSubstancesGenerator.Create(3);
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(20, 2, true, random);
            var rpfs = compounds.ToDictionary(r => r, r => 1d);
            var memberships = compounds.ToDictionary(r => r, r => 1d);
            var exposures = FakeDietaryIndividualDayIntakeGenerator.GenerateImputed(individualDays, foods, compounds, 0.5, true, random);

            var section = new UpperDistributionFoodAsEatenSection();
            section.Summarize(exposures, rpfs, memberships, foods, ExposureType.Chronic, 2.5, 97.5, 2.5, 97.5, 97.5, false);
            Assert.IsTrue(section.Records.Sum(c => c.ContributionPercentage) > 99.99 && section.Records.Sum(c => c.ContributionPercentage) < 100.01);
            AssertIsValidView(section);
        }

        /// <summary>
        /// With imputation, chronic, test UpperDistributionFoodAsEatenSection view
        /// </summary>
        [TestMethod]
        public void UpperDistributionFoodAsEatenSectionSummary_SummarizeUncertaintyChronic1() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = FakeFoodsGenerator.MockFoods("Apple", "Pear", "Bananas");
            var compounds = FakeSubstancesGenerator.Create(3);
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(20, 2, true, random);
            var rpfs = compounds.ToDictionary(r => r, r => 1d);
            var memberships = compounds.ToDictionary(r => r, r => 1d);
            var exposures = FakeDietaryIndividualDayIntakeGenerator.GenerateImputed(individualDays, foods, compounds, 0.5, true, random);

            var section = new UpperDistributionFoodAsEatenSection();
            section.Summarize(exposures, rpfs, memberships, foods, ExposureType.Chronic, 2.5, 97.5, 2.5, 97.5, 97.5, false);
            section.SummarizeUncertainty(exposures, rpfs, memberships, ExposureType.Chronic, 97.5, false);
            Assert.IsTrue(section.Records.Sum(c => c.ContributionPercentage) > 99.99 && section.Records.Sum(c => c.ContributionPercentage) < 100.01);
            AssertIsValidView(section);
        }
    }
}
