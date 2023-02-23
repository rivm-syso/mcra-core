using MCRA.General;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using MCRA.Simulation.OutputGeneration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.DietaryExposures {
    /// <summary>
    /// OutputGeneration, ActionSummaries,DietaryExposures, ExposureByFood, FoodAsMeasured
    /// </summary>
    [TestClass()]
    public class UpperDistributionFoodAsMeasuredSectionTests : SectionTestBase {
        /// <summary>
        /// No imputation, acute, test UpperDistributionFoodAsMeasuredSection view
        /// </summary>
        [TestMethod]
        public void UpperDistributionFoodAsMeasuredSectionSummary_SummarizeAcute1() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = MockFoodsGenerator.MockFoods("Apple", "Pear", "Bananas");
            var compounds = MockSubstancesGenerator.Create(3);
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(20, 2, true, random);
            var rpfs = compounds.ToDictionary(r => r, r => 1d);
            var memberships = compounds.ToDictionary(r => r, r => 1d);
            var exposures = MockDietaryIndividualDayIntakeGenerator.Create(individualDays, foods, compounds, 0.5, true, random);

            var section = new UpperDistributionFoodAsMeasuredSection();
            section.Summarize(foods, exposures, rpfs, memberships, foods, ExposureType.Acute, 2.5, 97.5, 2.5, 97.5, 95, false);
            Assert.IsTrue(section.Records.Sum(c => c.ContributionPercentage) > 99.99 && section.Records.Sum(c => c.ContributionPercentage) < 100.01);
            AssertIsValidView(section);
        }
        /// <summary>
        /// With imputation, acute, test UpperDistributionFoodAsMeasuredSection view
        /// </summary>
        [TestMethod]
        public void UpperDistributionFoodAsMeasuredSectionSummary_SummarizeAcute2() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = MockFoodsGenerator.MockFoods("Apple", "Pear", "Bananas");
            var compounds = MockSubstancesGenerator.Create(3);
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(20, 2, true, random);
            var rpfs = compounds.ToDictionary(r => r, r => 1d);
            var memberships = compounds.ToDictionary(r => r, r => 1d);
            var exposures = MockDietaryIndividualDayIntakeGenerator.GenerateImputed(individualDays, foods, compounds, 0.5, true, random);

            var section = new UpperDistributionFoodAsMeasuredSection();
            section.Summarize(foods, exposures, rpfs, memberships, foods, ExposureType.Acute, 2.5, 97.5, 2.5, 97.5, 95, false);
            Assert.IsTrue(section.Records.Sum(c => c.ContributionPercentage) > 99.99 && section.Records.Sum(c => c.ContributionPercentage) < 100.01);
            AssertIsValidView(section);
        }
        /// <summary>
        /// With imputation,acute, test UpperDistributionFoodAsMeasuredSection view
        /// </summary>
        [TestMethod]
        public void UpperDistributionFoodAsMeasuredSectionSummary_SummarizeUncertaintyAcute1() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = MockFoodsGenerator.MockFoods("Apple", "Pear", "Bananas");
            var compounds = MockSubstancesGenerator.Create(3);
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(20, 2, true, random);
            var rpfs = compounds.ToDictionary(r => r, r => 1d);
            var memberships = compounds.ToDictionary(r => r, r => 1d);
            var exposures = MockDietaryIndividualDayIntakeGenerator.GenerateImputed(individualDays, foods, compounds, 0.5, true, random);

            var section = new UpperDistributionFoodAsMeasuredSection();
            section.Summarize(foods, exposures, rpfs, memberships, foods, ExposureType.Acute, 2.5, 97.5, 2.5, 97.5, 95, false);
            section.SummarizeUncertainty(foods, exposures, rpfs, memberships, ExposureType.Acute, false);
            Assert.IsTrue(section.Records.Sum(c => c.ContributionPercentage) > 99.99 && section.Records.Sum(c => c.ContributionPercentage) < 100.01);
            AssertIsValidView(section);
        }
        /// <summary>
        /// No imputation, chronic, test UpperDistributionFoodAsMeasuredSection view
        /// </summary>
        [TestMethod]
        public void UpperDistributionFoodAsMeasuredSectionSummary_SummarizeChronic1() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = MockFoodsGenerator.MockFoods("Apple", "Pear", "Bananas");
            var compounds = MockSubstancesGenerator.Create(3);
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(20, 2, true, random);
            var rpfs = compounds.ToDictionary(r => r, r => 1d);
            var memberships = compounds.ToDictionary(r => r, r => 1d);
            var exposures = MockDietaryIndividualDayIntakeGenerator.Create(individualDays, foods, compounds, 0.5, true, random);

            var section = new UpperDistributionFoodAsMeasuredSection();
            section.Summarize(foods, exposures, rpfs, memberships, foods, ExposureType.Chronic, 2.5, 97.5, 2.5, 97.5, 95, false);
            Assert.IsTrue(section.Records.Sum(c => c.ContributionPercentage) > 99.99 && section.Records.Sum(c => c.ContributionPercentage) < 100.01);
            AssertIsValidView(section);
        }
        /// <summary>
        /// With imputation, chronic, test UpperDistributionFoodAsMeasuredSection view
        /// </summary>
        [TestMethod]
        public void UpperDistributionFoodAsMeasuredSectionSummary_SummarizeChronic2() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = MockFoodsGenerator.MockFoods("Apple", "Pear", "Bananas");
            var compounds = MockSubstancesGenerator.Create(3);
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(20, 2, true, random);
            var rpfs = compounds.ToDictionary(r => r, r => 1d);
            var memberships = compounds.ToDictionary(r => r, r => 1d);
            var exposures = MockDietaryIndividualDayIntakeGenerator.GenerateImputed(individualDays, foods, compounds, 0.5, true, random);
            var section = new UpperDistributionFoodAsMeasuredSection();
            section.Summarize(foods, exposures, rpfs, memberships, foods, ExposureType.Chronic, 2.5, 97.5, 2.5, 97.5, 95, false);
            Assert.IsTrue(section.Records.Sum(c => c.ContributionPercentage) > 99.99 && section.Records.Sum(c => c.ContributionPercentage) < 100.01);
            AssertIsValidView(section);
        }
        /// <summary>
        /// With imputation, chronic, test UpperDistributionFoodAsMeasuredSection view
        /// </summary>
        [TestMethod]
        public void UpperDistributionFoodAsMeasuredSectionSummary_SummarizeUncertaintyChronic1() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = MockFoodsGenerator.MockFoods("Apple", "Pear", "Bananas");
            var compounds = MockSubstancesGenerator.Create(3);
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(20, 2, true, random);
            var rpfs = compounds.ToDictionary(r => r, r => 1d);
            var memberships = compounds.ToDictionary(r => r, r => 1d);
            var exposures = MockDietaryIndividualDayIntakeGenerator.GenerateImputed(individualDays, foods, compounds, 0.5, true, random);

            var section = new UpperDistributionFoodAsMeasuredSection();
            section.Summarize(foods, exposures, rpfs, memberships, foods, ExposureType.Chronic, 2.5, 97.5, 2.5, 97.5, 95, false);
            section.SummarizeUncertainty(foods, exposures, rpfs, memberships, ExposureType.Chronic, false);
            Assert.IsTrue(section.Records.Sum(c => c.ContributionPercentage) > 99.99 && section.Records.Sum(c => c.ContributionPercentage) < 100.01);
            AssertIsValidView(section);
        }
    }
}
