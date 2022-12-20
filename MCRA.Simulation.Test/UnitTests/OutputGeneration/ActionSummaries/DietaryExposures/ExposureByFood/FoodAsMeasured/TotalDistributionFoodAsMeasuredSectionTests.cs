using MCRA.Utils.Statistics;
using MCRA.General;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.DietaryExposures {
    /// <summary>
    /// OutputGeneration, ActionSummaries, DietaryExposures, ExposureByFood, FoodAsMeasured
    /// </summary>
    [TestClass()]
    public class TotalDistributionFoodAsMeasuredSectionTests : SectionTestBase {

        /// <summary>
        /// No imputation, acute, test TotalDistributionFoodAsMeasuredSection view
        /// </summary>
        [TestMethod]
        public void TotalDistributionFoodAsMeasuredSectionSummary_SummarizeAcute1() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = MockFoodsGenerator.MockFoods("Apple", "Pear", "Bananas");
            var compounds = MockSubstancesGenerator.Create(3);
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(20, 2, true, random);
            var rpfs = compounds.ToDictionary(r => r, r => 1d);
            var memberships = compounds.ToDictionary(r => r, r => 1d);
            var exposures = MockDietaryIndividualDayIntakeGenerator.Create(individualDays, foods, compounds, 0.5, true, random);

            var section = new TotalDistributionFoodAsMeasuredSection();
            section.Summarize(foods, exposures, rpfs, memberships, foods, ExposureType.Acute, 2.5, 97.5, 2.5, 97.5, false);
            Assert.IsTrue(section.Records.Sum(c => c.ContributionPercentage) > 99.99 && section.Records.Sum(c => c.ContributionPercentage) < 100.01);
            AssertIsValidView(section);
        }

        /// <summary>
        /// With imputation, acute, test TotalDistributionFoodAsMeasuredSection view
        /// </summary>
        [TestMethod]
        public void TotalDistributionFoodAsMeasuredSectionSummary_SummarizeAcute2() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = MockFoodsGenerator.MockFoods("Apple", "Pear", "Bananas");
            var compounds = MockSubstancesGenerator.Create(3);
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(20, 2, true, random);
            var rpfs = compounds.ToDictionary(r => r, r => 1d);
            var memberships = compounds.ToDictionary(r => r, r => 1d);
            var exposures = MockDietaryIndividualDayIntakeGenerator.GenerateImputed(individualDays, foods, compounds, 0.5, true, random);

            var section = new TotalDistributionFoodAsMeasuredSection();
            section.Summarize(foods, exposures, rpfs, memberships, foods, ExposureType.Acute, 2.5, 97.5, 2.5, 97.5, false);
            Assert.IsTrue(section.Records.Sum(c => c.ContributionPercentage) > 99.99 && section.Records.Sum(c => c.ContributionPercentage) < 100.01);
            AssertIsValidView(section);
        }


        /// <summary>
        /// With imputation, acute, test TotalDistributionFoodAsMeasuredSection view
        /// </summary>
        [TestMethod]
        public void TotalDistributionFoodAsMeasuredSectionSummary_SummarizeUncertaintyAcute1() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = MockFoodsGenerator.MockFoods("Apple", "Pear", "Bananas");
            var compounds = MockSubstancesGenerator.Create(3);
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(20, 2, true, random);
            var rpfs = compounds.ToDictionary(r => r, r => 1d);
            var memberships = compounds.ToDictionary(r => r, r => 1d);
            var exposures = MockDietaryIndividualDayIntakeGenerator.GenerateImputed(individualDays, foods, compounds, 0.5, true, random);

            var section = new TotalDistributionFoodAsMeasuredSection();
            section.Summarize(foods, exposures, rpfs, memberships, foods, ExposureType.Acute, 2.5, 97.5, 2.5, 97.5, false);
            section.SummarizeUncertainty(foods, exposures, rpfs, memberships, ExposureType.Acute, false);
            Assert.IsTrue(section.Records.Sum(c => c.ContributionPercentage) > 99.99 && section.Records.Sum(c => c.ContributionPercentage) < 100.01);
            AssertIsValidView(section);
        }

        /// <summary>
        /// No imputation, chronic, test TotalDistributionFoodAsMeasuredSection view
        /// </summary>
        [TestMethod]
        public void TotalDistributionFoodAsMeasuredSectionSummary_SummarizeChronic1() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = MockFoodsGenerator.MockFoods("Apple", "Pear", "Bananas");
            var compounds = MockSubstancesGenerator.Create(3);
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(20, 2, true, random);
            var rpfs = compounds.ToDictionary(r => r, r => 1d);
            var memberships = compounds.ToDictionary(r => r, r => 1d);
            var exposures = MockDietaryIndividualDayIntakeGenerator.Create(individualDays, foods, compounds, 0.5, true, random);

            var section = new TotalDistributionFoodAsMeasuredSection();
            section.Summarize(foods, exposures, rpfs, memberships, foods, ExposureType.Chronic, 2.5, 97.5, 2.5, 97.5, false);
            Assert.IsTrue(section.Records.Sum(c => c.ContributionPercentage) > 99.99 && section.Records.Sum(c => c.ContributionPercentage) < 100.01);
            AssertIsValidView(section);
        }


        /// <summary>
        /// With imputation, chronic, test TotalDistributionFoodAsMeasuredSection view
        /// </summary>
        [TestMethod]
        public void TotalDistributionFoodAsMeasuredSectionSummary_SummarizeChronic2() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = MockFoodsGenerator.MockFoods("Apple", "Pear", "Bananas");
            var compounds = MockSubstancesGenerator.Create(3);
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(20, 2, true, random);
            var rpfs = compounds.ToDictionary(r => r, r => 1d);
            var memberships = compounds.ToDictionary(r => r, r => 1d);
            var exposures = MockDietaryIndividualDayIntakeGenerator.GenerateImputed(individualDays, foods, compounds, 0.5, true, random);

            var section = new TotalDistributionFoodAsMeasuredSection();
            section.Summarize(foods, exposures, rpfs, memberships, foods, ExposureType.Chronic, 2.5, 97.5, 2.5, 97.5, false);
            Assert.IsTrue(section.Records.Sum(c => c.ContributionPercentage) > 99.99 && section.Records.Sum(c => c.ContributionPercentage) < 100.01);
            AssertIsValidView(section);
        }


        /// <summary>
        /// With imputation, chronic, test TotalDistributionFoodAsMeasuredSection view
        /// </summary>
        [TestMethod]
        public void TotalDistributionFoodAsMeasuredSectionSummary_SummarizeUncertaintyChronic1() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = MockFoodsGenerator.MockFoods("Apple", "Pear", "Bananas");
            var compounds = MockSubstancesGenerator.Create(3);
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(20, 2, true, random);
            var rpfs = compounds.ToDictionary(r => r, r => 1d);
            var memberships = compounds.ToDictionary(r => r, r => 1d);
            var exposures = MockDietaryIndividualDayIntakeGenerator.GenerateImputed(individualDays, foods, compounds, 0.5, true, random);
            var section = new TotalDistributionFoodAsMeasuredSection();

            section.Summarize(foods, exposures, rpfs, memberships, foods, ExposureType.Acute, 2.5, 97.5, 2.5, 97.5, false);
            section.SummarizeUncertainty(foods, exposures, rpfs, memberships, ExposureType.Acute, false);
            Assert.IsTrue(section.Records.Sum(c => c.ContributionPercentage) > 99.99 && section.Records.Sum(c => c.ContributionPercentage) < 100.01);
            AssertIsValidView(section);
        }
    }
}
