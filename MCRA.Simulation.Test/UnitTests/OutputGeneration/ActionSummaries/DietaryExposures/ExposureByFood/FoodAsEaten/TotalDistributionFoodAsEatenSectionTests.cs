using MCRA.Utils.Statistics;
using MCRA.General;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.DietaryExposures {
    /// <summary>
    /// OutputGeneration, ActionSummaries, DietaryExposures, ExposureByFood, FoodAsEaten
    /// </summary>
    [TestClass()]
    public class TotalDistributionFoodAsEatenSectionTests : SectionTestBase {

        /// <summary>
        /// No imputation, acute, test TotalDistributionFoodAsEatenSection view
        /// </summary>
        [TestMethod]
        public void TotalDistributionFoodAsEatenSectionSummary_SummarizeAcute1() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = FakeFoodsGenerator.MockFoods("Apple", "Pear", "Bananas");
            var compounds = FakeSubstancesGenerator.Create(3);
            var individuals = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(20, 2, true, random);
            var rpfs = compounds.ToDictionary(r => r, r => 1d);
            var memberships = compounds.ToDictionary(r => r, r => 1d);
            var exposures = FakeDietaryIndividualDayIntakeGenerator.Create(individuals, foods, compounds, 0.5, true, random);

            var section = new TotalDistributionFoodAsEatenSection();
            section.Summarize(exposures, rpfs, memberships, ExposureType.Acute, 2.5, 97.5, 2.5, 97.5, false);
            Assert.IsTrue(section.Records.Sum(c => c.ContributionPercentage) > 99.99 && section.Records.Sum(c => c.ContributionPercentage) < 100.01);
            AssertIsValidView(section);
        }


        /// <summary>
        /// With imputation, acute, test TotalDistributionFoodAsEatenSection view
        /// </summary>
        [TestMethod]
        public void TotalDistributionFoodAsEatenSectionSummary_SummarizeAcute2() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = FakeFoodsGenerator.MockFoods("Apple", "Pear", "Bananas");
            var compounds = FakeSubstancesGenerator.Create(3);
            var individuals = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(20, 2, true, random);
            var rpfs = compounds.ToDictionary(r => r, r => 1d);
            var memberships = compounds.ToDictionary(r => r, r => 1d);
            var exposures = FakeDietaryIndividualDayIntakeGenerator.GenerateImputed(individuals, foods, compounds, 0.5, true, random);

            var section = new TotalDistributionFoodAsEatenSection();
            section.Summarize(exposures, rpfs, memberships, ExposureType.Acute, 2.5, 97.5, 2.5, 97.5, false);
            Assert.IsTrue(section.Records.Sum(c => c.ContributionPercentage) > 99.99 && section.Records.Sum(c => c.ContributionPercentage) < 100.01);
            AssertIsValidView(section);
        }

        /// <summary>
        /// With imputation, acute, test TotalDistributionFoodAsEatenSection view
        /// </summary>
        [TestMethod]
        public void TotalDistributionFoodAsEatenSectionSummary_SummarizeUncertaintyAcute1() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = FakeFoodsGenerator.MockFoods("Apple", "Pear", "Bananas");
            var compounds = FakeSubstancesGenerator.Create(3);
            var individuals = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(20, 2, true, random);
            var rpfs = compounds.ToDictionary(r => r, r => 1d);
            var memberships = compounds.ToDictionary(r => r, r => 1d);
            var exposures = FakeDietaryIndividualDayIntakeGenerator.GenerateImputed(individuals, foods, compounds, 0.5, true, random);

            var section = new TotalDistributionFoodAsEatenSection();
            section.Summarize(exposures, rpfs, memberships, ExposureType.Acute, 2.5, 97.5, 2.5, 97.5, false);
            section.SummarizeUncertainty(exposures, rpfs, memberships, ExposureType.Acute, false);
            Assert.IsTrue(section.Records.Sum(c => c.ContributionPercentage) > 99.99 && section.Records.Sum(c => c.ContributionPercentage) < 100.01);
            AssertIsValidView(section);
        }


        /// <summary>
        /// No imputation, chronic, test TotalDistributionFoodAsEatenSection view
        /// </summary>
        [TestMethod]
        public void TotalDistributionFoodAsEatenSectionSummary_SummarizeChronic1() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = FakeFoodsGenerator.MockFoods("Apple", "Pear", "Bananas");
            var compounds = FakeSubstancesGenerator.Create(3);
            var individuals = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(20, 2, true, random);
            var rpfs = compounds.ToDictionary(r => r, r => 1d);
            var memberships = compounds.ToDictionary(r => r, r => 1d);
            var exposures = FakeDietaryIndividualDayIntakeGenerator.Create(individuals, foods, compounds, 0.5, true, random);

            var section = new TotalDistributionFoodAsEatenSection();
            section.Summarize(exposures, rpfs, memberships, ExposureType.Chronic, 2.5, 97.5, 2.5, 97.5, false);
            Assert.IsTrue(section.Records.Sum(c => c.ContributionPercentage) > 99.99 && section.Records.Sum(c => c.ContributionPercentage) < 100.01);
            AssertIsValidView(section);
        }


        /// <summary>
        /// With imputation, chronic, test TotalDistributionFoodAsEatenSection view
        /// </summary>
        [TestMethod]
        public void TotalDistributionFoodAsEatenSectionSummary_SummarizeChronic2() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = FakeFoodsGenerator.MockFoods("Apple", "Pear", "Bananas");
            var compounds = FakeSubstancesGenerator.Create(3);
            var individuals = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(20, 2, true, random);
            var rpfs = compounds.ToDictionary(r => r, r => 1d);
            var memberships = compounds.ToDictionary(r => r, r => 1d);
            var exposures = FakeDietaryIndividualDayIntakeGenerator.GenerateImputed(individuals, foods, compounds, 0.5, true, random);

            var section = new TotalDistributionFoodAsEatenSection();
            section.Summarize(exposures, rpfs, memberships, ExposureType.Chronic, 2.5, 97.5, 2.5, 97.5, false);
            Assert.IsTrue(section.Records.Sum(c => c.ContributionPercentage) > 99.99 && section.Records.Sum(c => c.ContributionPercentage) < 100.01);
            AssertIsValidView(section);
        }


        /// <summary>
        /// With imputation, chronic, test TotalDistributionFoodAsEatenSection view
        /// </summary>
        [TestMethod]
        public void TotalDistributionFoodAsEatenSectionSummary_SummarizeUncertaintyChronic1() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = FakeFoodsGenerator.MockFoods("Apple", "Pear", "Bananas");
            var compounds = FakeSubstancesGenerator.Create(3);
            var individuals = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(20, 2, true, random);
            var rpfs = compounds.ToDictionary(r => r, r => 1d);
            var memberships = compounds.ToDictionary(r => r, r => 1d);
            var exposures = FakeDietaryIndividualDayIntakeGenerator.GenerateImputed(individuals, foods, compounds, 0.5, true, random);

            var section = new TotalDistributionFoodAsEatenSection();
            section.Summarize(exposures, rpfs, memberships, ExposureType.Chronic, 2.5, 97.5, 2.5, 97.5, false);
            section.SummarizeUncertainty(exposures, rpfs, memberships, ExposureType.Chronic, false);
            Assert.IsTrue(section.Records.Sum(c => c.ContributionPercentage) > 99.99 && section.Records.Sum(c => c.ContributionPercentage) < 100.01);
            AssertIsValidView(section);
        }
    }
}
