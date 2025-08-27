using MCRA.General;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;

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
            var foods = FakeFoodsGenerator.MockFoods("Apple", "Pear", "Bananas");
            var compounds = FakeSubstancesGenerator.Create(3);
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(20, 2, true, random);
            var rpfs = compounds.ToDictionary(r => r, r => 1d);
            var memberships = compounds.ToDictionary(r => r, r => 1d);
            var exposures = FakeDietaryIndividualDayIntakeGenerator.Create(individualDays, foods, compounds, 0.5, true, random);

            var section = new UpperDistributionFoodAsMeasuredSection();
            section.Summarize(foods, exposures, rpfs, memberships, foods, ExposureType.Acute, 2.5, 97.5, 2.5, 97.5, 95, false);
            Assert.IsTrue(section.Records.Sum(c => c.ContributionPercentage) > 99.99 && section.Records.Sum(c => c.ContributionPercentage) < 100.01);
            AssertIsValidView(section);
        }


        /// <summary>
        /// No imputation, acute, test UpperDistributionFoodAsMeasuredSection view
        /// </summary>
        [TestMethod]
        public void UpperDistributionFoodAsMeasuredSectionSummary_SummarizeAcuteHierarchical() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var apple = new Food("Apple");
            var peeledApple = new Food("Apple-Peeled") { Parent = apple };
            var bananas = new Food("Bananas");
            var allFoods = new List<Food>() { apple, peeledApple, bananas };
            var modelledFoods = new List<Food>() { apple, peeledApple, bananas };
            var modelledFoodsWithExposure = new List<Food>() { peeledApple, bananas };
            var compounds = FakeSubstancesGenerator.Create(3);
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(20, 2, true, random);
            var rpfs = compounds.ToDictionary(r => r, r => 1d);
            var memberships = compounds.ToDictionary(r => r, r => 1d);
            var exposures = FakeDietaryIndividualDayIntakeGenerator
                .Create(
                    individualDays,
                    modelledFoodsWithExposure,
                    compounds,
                    fractionZeros: 0.5,
                    isDetailed: true,
                    random
                );

            var section = new UpperDistributionFoodAsMeasuredSection();
            section.Summarize(allFoods, exposures, rpfs, memberships, allFoods, ExposureType.Acute, 2.5, 97.5, 2.5, 97.5, 95, false);

            CollectionAssert.AreEquivalent(
                new[] { "Apple", "Apple-Peeled", "Bananas" },
                section.Records.Select(r => r.__Id).ToArray()
            );

            CollectionAssert.AreEquivalent(
                new[] { "Apple-group" },
                section.HierarchicalNodes.Select(r => r.__Id).ToArray()
            );

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
            var foods = FakeFoodsGenerator.MockFoods("Apple", "Pear", "Bananas");
            var compounds = FakeSubstancesGenerator.Create(3);
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(20, 2, true, random);
            var rpfs = compounds.ToDictionary(r => r, r => 1d);
            var memberships = compounds.ToDictionary(r => r, r => 1d);
            var exposures = FakeDietaryIndividualDayIntakeGenerator.GenerateImputed(individualDays, foods, compounds, 0.5, true, random);

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
            var foods = FakeFoodsGenerator.MockFoods("Apple", "Pear", "Bananas");
            var compounds = FakeSubstancesGenerator.Create(3);
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(20, 2, true, random);
            var rpfs = compounds.ToDictionary(r => r, r => 1d);
            var memberships = compounds.ToDictionary(r => r, r => 1d);
            var exposures = FakeDietaryIndividualDayIntakeGenerator.GenerateImputed(individualDays, foods, compounds, 0.5, true, random);

            var section = new UpperDistributionFoodAsMeasuredSection();
            section.Summarize(foods, exposures, rpfs, memberships, foods, ExposureType.Acute, 2.5, 97.5, 2.5, 97.5, 95, false);
            section.SummarizeUncertainty(foods, exposures, rpfs, memberships, ExposureType.Acute, 97.5, false);
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
            var foods = FakeFoodsGenerator.MockFoods("Apple", "Pear", "Bananas");
            var compounds = FakeSubstancesGenerator.Create(3);
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(20, 2, true, random);
            var rpfs = compounds.ToDictionary(r => r, r => 1d);
            var memberships = compounds.ToDictionary(r => r, r => 1d);
            var exposures = FakeDietaryIndividualDayIntakeGenerator.Create(individualDays, foods, compounds, 0.5, true, random);

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
            var foods = FakeFoodsGenerator.MockFoods("Apple", "Pear", "Bananas");
            var compounds = FakeSubstancesGenerator.Create(3);
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(20, 2, true, random);
            var rpfs = compounds.ToDictionary(r => r, r => 1d);
            var memberships = compounds.ToDictionary(r => r, r => 1d);
            var exposures = FakeDietaryIndividualDayIntakeGenerator.GenerateImputed(individualDays, foods, compounds, 0.5, true, random);
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
            var foods = FakeFoodsGenerator.MockFoods("Apple", "Pear", "Bananas");
            var compounds = FakeSubstancesGenerator.Create(3);
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(20, 2, true, random);
            var rpfs = compounds.ToDictionary(r => r, r => 1d);
            var memberships = compounds.ToDictionary(r => r, r => 1d);
            var exposures = FakeDietaryIndividualDayIntakeGenerator.GenerateImputed(individualDays, foods, compounds, 0.5, true, random);

            var section = new UpperDistributionFoodAsMeasuredSection();
            section.Summarize(foods, exposures, rpfs, memberships, foods, ExposureType.Chronic, 2.5, 97.5, 2.5, 97.5, 95, false);
            section.SummarizeUncertainty(foods, exposures, rpfs, memberships, ExposureType.Chronic, 97.5, false);
            Assert.IsTrue(section.Records.Sum(c => c.ContributionPercentage) > 99.99 && section.Records.Sum(c => c.ContributionPercentage) < 100.01);
            AssertIsValidView(section);
        }
    }
}
