using MCRA.Utils.Statistics;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;
using MCRA.Simulation.Calculators.IntakeModelling;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
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
            var foods = FakeFoodsGenerator.MockFoods("Apple", "Pear", "Bananas");
            var substances = FakeSubstancesGenerator.Create(3);
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(50, 2, true, random);
            var dietaryIndividualDayIntakes = FakeDietaryIndividualDayIntakeGenerator.Create(individualDays, foods, substances, 0.5, true, random, false);
            var rpfs = substances.ToDictionary(r => r, r => 1d);
            var memberships = substances.ToDictionary(r => r, r => 1d);

            var observedIndividualMeans = dietaryIndividualDayIntakes
               .GroupBy(r => r.SimulatedIndividual)
                   .Select(g => new DietaryIndividualIntake() {
                       SimulatedIndividual = g.Key,
                       NumberOfDays = g.Count(idi => idi.TotalExposure(rpfs, memberships) > 0),
                       DietaryIntakePerMassUnit = g.Average(i => i.TotalExposurePerMassUnit(rpfs, memberships, false)),
                   })
                   .OrderBy(o => o.DietaryIntakePerMassUnit)
                   .ToList();

            var section = new DietaryChronicDrillDownSection();

            section.Summarize(
                new(),
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
                false
            );
            Assert.AreEqual(9, section.OverallIndividualDrillDownRecords.Count);

            AssertIsValidView(section);
        }
        /// <summary>
        /// Summarize, test DietaryChronicDrilldownSection view, BBN
        /// </summary>
        [TestMethod]
        public void DietaryChronicDrillDownSectionBBN_Test2() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = FakeFoodsGenerator.MockFoods("Apple", "Pear", "Bananas");
            var substances = FakeSubstancesGenerator.Create(3);
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(50, 2, true, random);
            var dietaryIndividualDayIntakes = FakeDietaryIndividualDayIntakeGenerator.Create(individualDays, foods, substances, 0.5, true, random, false);
            var rpfs = substances.ToDictionary(r => r, r => 1d);
            var memberships = substances.ToDictionary(r => r, r => 1d);

            var observedIndividualMeans = dietaryIndividualDayIntakes
               .GroupBy(r => r.SimulatedIndividual)
                   .Select(g => new DietaryIndividualIntake() {
                       SimulatedIndividual = g.Key,
                       NumberOfDays = g.Count(idi => idi.TotalExposure(rpfs, memberships) > 0),
                       DietaryIntakePerMassUnit = g.Average(i => i.TotalExposurePerMassUnit(rpfs, memberships, false)),
                   })
                   .OrderBy(o => o.DietaryIntakePerMassUnit)
                   .ToList();

            var section = new DietaryChronicDrillDownSection();
            var usualIntakes = observedIndividualMeans.Select(c => new ModelAssistedIntake {
                SimulatedIndividual = c.SimulatedIndividual,
                UsualIntake = c.DietaryIntakePerMassUnit,
                ShrinkageFactor = 0,
                NDays = 2,
                TransformedOIM = Math.Log(c.DietaryIntakePerMassUnit),
            }).ToList();

            section.Summarize(
                new(),
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
            Assert.AreEqual(9, section.OverallIndividualDrillDownRecords.Count);
            AssertIsValidView(section);
        }
        /// <summary>
        /// Summarize, test DietaryChronicDrilldownSection view, BBN
        /// </summary>
        [TestMethod]
        public void DietaryChronicDrillDownSectionBBN_Test3() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = FakeFoodsGenerator.MockFoods("Apple", "Pear", "Bananas");
            var substances = FakeSubstancesGenerator.Create(3);
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(50, 2, true, random);
            var dietaryIndividualDayIntakes = FakeDietaryIndividualDayIntakeGenerator.Create(individualDays, foods, substances, 0.5, true, random, false);
            var rpfs = substances.ToDictionary(r => r, r => 1d);
            var memberships = substances.ToDictionary(r => r, r => 1d);

            var observedIndividualMeans = dietaryIndividualDayIntakes
               .GroupBy(r => r.SimulatedIndividual)
                   .Select(g => new DietaryIndividualIntake() {
                       SimulatedIndividual = g.Key,
                       NumberOfDays = g.Count(idi => idi.TotalExposure(rpfs, memberships) > 0),
                       DietaryIntakePerMassUnit = g.Average(i => i.TotalExposurePerMassUnit(rpfs, memberships, false)),
                   })
                   .OrderBy(o => o.DietaryIntakePerMassUnit)
                   .ToList();

            var section = new DietaryChronicDrillDownSection();
            var usualIntakes = observedIndividualMeans.Select(c => new ModelAssistedIntake {
                UsualIntake = c.DietaryIntakePerMassUnit,
                ShrinkageFactor = 0,
                NDays = 2,
                TransformedOIM = Math.Log(c.DietaryIntakePerMassUnit),
                SimulatedIndividual = c.SimulatedIndividual
            }).ToList();

            section.Summarize(
                new(),
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
            Assert.AreEqual(9, section.OverallIndividualDrillDownRecords.Count);
            AssertIsValidView(section);
        }
    }
}
