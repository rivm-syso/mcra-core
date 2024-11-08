using MCRA.Utils.Statistics;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.DietaryExposures {
    /// <summary>
    ///  OutputGeneration, ActionSummaries, DietaryExposures, Drilldown, Acute
    /// </summary>
    [TestClass]
    public class DietaryAcuteDrillDownSectionTests : SectionTestBase {
        /// <summary>
        /// Summarize, test DietaryAcuteDrillDownSection view and all related drilldown views
        /// </summary>
        [TestMethod]
        public void DietaryAcuteDrillDownSectionDefault_Test1() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = FakeFoodsGenerator.MockFoods("Apple", "Pear", "Bananas");
            var substances = FakeSubstancesGenerator.Create(3);
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(50, 2, false, random);
            var dietaryIndividualDayIntakes = FakeDietaryIndividualDayIntakeGenerator.Create(individualDays, foods, substances, 0.5, true, random, false);
            var rpfs = substances.ToDictionary(r => r, r => 1d);
            var memberships = substances.ToDictionary(r => r, r => 1d);
            var section = new DietaryAcuteDrillDownSection();
            section.Summarize(dietaryIndividualDayIntakes,
                substances,
                rpfs,
                memberships,
                substances[0],
                null,
                true,
                false,
                true,
                95,
                false
            );
            Assert.AreEqual(9, section.DrillDownSummaryRecords.Count);
            Assert.AreEqual(3, section.DrillDownSummaryRecords[4].IntakeSummaryPerCompoundRecords.Count);
            Assert.AreEqual(3, section.DrillDownSummaryRecords[4].IntakeSummaryPerFoodAsEatenRecords.Count);
            Assert.AreEqual(3, section.DrillDownSummaryRecords[4].IntakeSummaryPerFoodAsMeasuredRecords.Count);
            AssertIsValidView(section);
        }

        /// <summary>
        /// Summarize, test DietaryAcuteDrillDownSection view, unit variability
        /// </summary>
        [TestMethod]
        public void DietaryAcuteDrillDownSectionUnitVariability_Test1() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = FakeFoodsGenerator.MockFoods("Apple", "Pear", "Bananas");
            var substances = FakeSubstancesGenerator.Create(3);
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(50, 2, false, random);
            var dietaryIndividualDayIntakes = FakeDietaryIndividualDayIntakeGenerator.Create(individualDays, foods, substances, 0.5, true, random, false);
            var rpfs = substances.ToDictionary(r => r, r => 1d);
            var memberships = substances.ToDictionary(r => r, r => 1d);
            var unitVariabilityFactors = FakeUnitVariabilityFactorsGenerator.Create(foods, substances, random);
            var section = new DietaryAcuteDrillDownSection();
            section.Summarize(dietaryIndividualDayIntakes,
                activeSubstances: substances,
                relativePotencyFactors: rpfs,
                membershipProbabilities: memberships,
                referenceCompound: substances[0],
                unitVariabilityDictionary: unitVariabilityFactors,
                isProcessing: false,
                isUnitVariability: true,
                isCumulative: true,
                percentageForDrilldown: 95,
                isPerPerson: false
            );
            Assert.AreEqual(9, section.DrillDownSummaryRecords.Count);
            Assert.AreEqual(3, section.DrillDownSummaryRecords[4].IntakeSummaryPerCompoundRecords.Count);
            Assert.AreEqual(3, section.DrillDownSummaryRecords[4].IntakeSummaryPerFoodAsEatenRecords.Count);
            Assert.AreEqual(3, section.DrillDownSummaryRecords[4].IntakeSummaryPerFoodAsMeasuredRecords.Count);
            AssertIsValidView(section);
        }

        /// <summary>
        /// Summarize, test DietaryAcuteDrillDownSection view, screening
        /// </summary>
        [TestMethod]
        public void DietaryAcuteDrillDownSectionIsScreening_Test1() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = FakeFoodsGenerator.MockFoods("Apple", "Pear", "Bananas");
            var substances = FakeSubstancesGenerator.Create(3);
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(50, 2, false, random);
            var dietaryIndividualDayIntakes = FakeDietaryIndividualDayIntakeGenerator.Create(individualDays, foods, substances, 0.5, true, random, false);
            var rpfs = substances.ToDictionary(r => r, r => 1d);
            var memberships = substances.ToDictionary(r => r, r => 1d);
            var section = new DietaryAcuteDrillDownSection();
            section.Summarize(dietaryIndividualDayIntakes,
                activeSubstances: substances,
                relativePotencyFactors: rpfs,
                membershipProbabilities: memberships,
                referenceCompound: substances[0],
                unitVariabilityDictionary: null,
                isProcessing: false,
                isUnitVariability: false,
                isCumulative: true,
                percentageForDrilldown: 95,
                isPerPerson: false
            );
            Assert.AreEqual(9, section.DrillDownSummaryRecords.Count);
            Assert.AreEqual(3, section.DrillDownSummaryRecords[4].IntakeSummaryPerCompoundRecords.Count);
            Assert.AreEqual(3, section.DrillDownSummaryRecords[4].IntakeSummaryPerFoodAsEatenRecords.Count);
            Assert.AreEqual(3, section.DrillDownSummaryRecords[4].IntakeSummaryPerFoodAsMeasuredRecords.Count);
            AssertIsValidView(section);
        }
    }
}
