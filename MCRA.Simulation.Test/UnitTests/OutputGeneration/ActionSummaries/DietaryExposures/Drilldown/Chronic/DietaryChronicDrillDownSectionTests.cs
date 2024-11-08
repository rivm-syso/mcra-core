using MCRA.Utils.Statistics;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;
using MCRA.Simulation.Calculators.IntakeModelling;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.DietaryExposures {
    /// <summary>
    /// OutputGeneration, ActionSummaries, DietaryExposures, Drilldown, Chronic
    /// </summary>
    [TestClass]
    public class DietaryChronicDrillDownSectionTests : SectionTestBase {

        /// <summary>
        /// Summarize, test DietaryChronicDrilldownSection view all related drilldown views, OIM
        /// </summary>
        [TestMethod]
        public void DietaryChronicDrillDownSectionOIM_Test2() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = MockFoodsGenerator.MockFoods("Apple", "Pear", "Bananas");
            var substances = MockSubstancesGenerator.Create(3);
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(50, 2, true, random);
            var dietaryIndividualDayIntakes = MockDietaryIndividualDayIntakeGenerator.Create(individualDays, foods, substances, 0.5, true, random, false);
            var rpfs = substances.ToDictionary(r => r, r => 1d);
            var memberships = substances.ToDictionary(r => r, r => 1d);

            var observedIndividualMeans = dietaryIndividualDayIntakes
               .GroupBy(r => r.SimulatedIndividualId)
                   .Select(g => new DietaryIndividualIntake() {
                       SimulatedIndividualId = g.Key,
                       IndividualSamplingWeight = g.First().IndividualSamplingWeight,
                       NumberOfDays = g.Count(idi => idi.TotalExposure(rpfs, memberships) > 0),
                       Individual = g.First().Individual,
                       DietaryIntakePerMassUnit = g.Average(i => i.TotalExposurePerMassUnit(rpfs, memberships, false)),
                   })
                   .OrderBy(o => o.DietaryIntakePerMassUnit)
                   .ToList();

            var section = new DietaryChronicDrilldownSection();
            section.IndividualDrillDownRecords = [new DietaryIndividualDrillDownRecord() { }];

            section.Summarize(
                observedIndividualMeans,
                dietaryIndividualDayIntakes,
                null,
                null,
                substances,
                rpfs,
                memberships,
                substances[0],
                true,
                true,
                50,
                false);
            Assert.AreEqual(9, section.ChronicDrillDownRecords.Count);
            Assert.AreEqual(3, section.ChronicDrillDownRecords[0].DayDrillDownRecords[1].ChronicIntakePerFoodRecords.Count);
            Assert.AreEqual(3, section.ChronicDrillDownRecords[0].DayDrillDownRecords[1].IntakeSummaryPerFoodAsEatenRecords.Count);
            Assert.AreEqual(3, section.ChronicDrillDownRecords[0].DayDrillDownRecords[1].IntakeSummaryPerFoodAsMeasuredRecords.Count);

            AssertIsValidView(section);
        }
        /// <summary>
        /// Summarize, test DietaryChronicDrilldownSection view, BBN
        /// </summary>
        [TestMethod]
        public void DietaryChronicDrillDownSectionBBN_Test2() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = MockFoodsGenerator.MockFoods("Apple", "Pear", "Bananas");
            var substances = MockSubstancesGenerator.Create(3);
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(50, 2, true, random);
            var dietaryIndividualDayIntakes = MockDietaryIndividualDayIntakeGenerator.Create(individualDays, foods, substances, 0.5, true, random, false);
            var rpfs = substances.ToDictionary(r => r, r => 1d);
            var memberships = substances.ToDictionary(r => r, r => 1d);

            var observedIndividualMeans = dietaryIndividualDayIntakes
               .GroupBy(r => r.SimulatedIndividualId)
                   .Select(g => new DietaryIndividualIntake() {
                       SimulatedIndividualId = g.Key,
                       IndividualSamplingWeight = g.First().IndividualSamplingWeight,
                       NumberOfDays = g.Count(idi => idi.TotalExposure(rpfs, memberships) > 0),
                       Individual = g.First().Individual,
                       DietaryIntakePerMassUnit = g.Average(i => i.TotalExposurePerMassUnit(rpfs, memberships, false)),
                   })
                   .OrderBy(o => o.DietaryIntakePerMassUnit)
                   .ToList();

            var section = new DietaryChronicDrilldownSection();
            var usualIntakes = observedIndividualMeans.Select(c => new ModelAssistedIntake() {
                Individual = c.Individual,
                IndividualSamplingWeight = c.IndividualSamplingWeight,
                UsualIntake = c.DietaryIntakePerMassUnit,
                ShrinkageFactor = 0,
                NDays = 2,
                TransformedOIM = Math.Log(c.DietaryIntakePerMassUnit),
                SimulatedIndividualId = c.SimulatedIndividualId
            }).ToList();

            section.Summarize(
                usualIntakes,
                observedIndividualMeans,
                dietaryIndividualDayIntakes,
                null,
                null,
                substances,
                rpfs,
                memberships,
                substances[0],
                false,
                true,
                50,
                false
            );
            Assert.AreEqual(9, section.ChronicDrillDownRecords.Count);
            Assert.AreEqual(3, section.ChronicDrillDownRecords[0].DayDrillDownRecords[1].ChronicIntakePerFoodRecords.Count);
            Assert.AreEqual(3, section.ChronicDrillDownRecords[0].DayDrillDownRecords[1].IntakeSummaryPerFoodAsEatenRecords.Count);
            Assert.AreEqual(3, section.ChronicDrillDownRecords[0].DayDrillDownRecords[1].IntakeSummaryPerFoodAsMeasuredRecords.Count);
            AssertIsValidView(section);
        }
        /// <summary>
        /// Summarize, test DietaryChronicDrilldownSection view, BBN
        /// </summary>
        [TestMethod]
        public void DietaryChronicDrillDownSectionBBN_Test3() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = MockFoodsGenerator.MockFoods("Apple", "Pear", "Bananas");
            var substances = MockSubstancesGenerator.Create(3);
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(50, 2, true, random);
            var dietaryIndividualDayIntakes = MockDietaryIndividualDayIntakeGenerator.Create(individualDays, foods, substances, 0.5, true, random, false);
            var rpfs = substances.ToDictionary(r => r, r => 1d);
            var memberships = substances.ToDictionary(r => r, r => 1d);

            var observedIndividualMeans = dietaryIndividualDayIntakes
               .GroupBy(r => r.SimulatedIndividualId)
                   .Select(g => new DietaryIndividualIntake() {
                       SimulatedIndividualId = g.Key,
                       IndividualSamplingWeight = g.First().IndividualSamplingWeight,
                       NumberOfDays = g.Count(idi => idi.TotalExposure(rpfs, memberships) > 0),
                       Individual = g.First().Individual,
                       DietaryIntakePerMassUnit = g.Average(i => i.TotalExposurePerMassUnit(rpfs, memberships, false)),
                   })
                   .OrderBy(o => o.DietaryIntakePerMassUnit)
                   .ToList();

            var section = new DietaryChronicDrilldownSection();
            var usualIntakes = observedIndividualMeans.Select(c => new ModelAssistedIntake() {
                Individual = c.Individual,
                IndividualSamplingWeight = c.IndividualSamplingWeight,
                UsualIntake = c.DietaryIntakePerMassUnit,
                ShrinkageFactor = 0,
                NDays = 2,
                TransformedOIM = Math.Log(c.DietaryIntakePerMassUnit),
                SimulatedIndividualId = c.SimulatedIndividualId
            }).ToList();

            section.Summarize(
                usualIntakes,
                observedIndividualMeans,
                dietaryIndividualDayIntakes,
                null,
                null,
                substances,
                rpfs,
                memberships,
                substances[0],
                false,
                true,
                50,
                false);
            Assert.AreEqual(9, section.ChronicDrillDownRecords.Count);
            Assert.AreEqual(3, section.ChronicDrillDownRecords[0].DayDrillDownRecords[1].ChronicIntakePerFoodRecords.Count);
            Assert.AreEqual(3, section.ChronicDrillDownRecords[0].DayDrillDownRecords[1].IntakeSummaryPerFoodAsEatenRecords.Count);
            Assert.AreEqual(3, section.ChronicDrillDownRecords[0].DayDrillDownRecords[1].IntakeSummaryPerFoodAsMeasuredRecords.Count);
            AssertIsValidView(section);
        }
    }
}
