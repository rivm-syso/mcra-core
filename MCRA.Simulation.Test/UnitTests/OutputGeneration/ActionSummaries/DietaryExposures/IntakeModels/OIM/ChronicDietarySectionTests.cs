using MCRA.Utils.Statistics;
using MCRA.General;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.DietaryExposures {
    /// <summary>
    /// OutputGeneration, ActionSummaries, DietaryExposures, IntakeModels, OIM
    /// </summary>
    [TestClass]
    public class ChronicDietarySectionTests : SectionTestBase {
        /// <summary>
        /// Summarize (uncertainty), test ChronicDietarySection view
        /// </summary>
        [TestMethod]
        public void ChronicDietarySection_Test1() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = FakeSubstancesGenerator.Create(3);
            var rpfs = substances.ToDictionary(r => r, r => 1d);
            var memberships = substances.ToDictionary(r => r, r => 1d);
            var foods = FakeFoodsGenerator.Create(3);
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(20, 2, true, random);
            var dietaryIndividualDayIntakes = FakeDietaryIndividualDayIntakeGenerator.Create(individualDays, foods, substances, 0.5, true, random);
            var dietaryObservedIndividualMeans = dietaryIndividualDayIntakes
              .GroupBy(idi => idi.SimulatedIndividualId)
              .Select(g => new DietaryIndividualIntake() {
                  SimulatedIndividualId = g.Key,
                  Individual = g.First().Individual,
                  IndividualSamplingWeight = g.First().IndividualSamplingWeight,
                  DietaryIntakePerMassUnit = g.Average(idi => idi.GetTotalDietaryIntakePerMassUnitPerCategory(foods, rpfs, memberships, false)),
              })
              .ToList();

            var section = new ChronicDietarySection();
            var header = new SectionHeader();
            var subHeader = header.AddEmptySubSectionHeader("Test percentiles", 1);
            section.Summarize(
                subHeader,
                dietaryObservedIndividualMeans,
                ExposureMethod.Automatic,
                substances.First(),
                [005, .1,],
                [50, 90, 95],
                95,
                false
            );
            subHeader = header.GetSubSectionHeader<IntakePercentileSection>();
            var percentileSection = subHeader.GetSummarySection() as IntakePercentileSection;
            Assert.IsTrue(!double.IsNaN(percentileSection.Percentiles[0].ReferenceValue));
            section.SummarizeUncertainty(
                header,
                dietaryObservedIndividualMeans,
                5,
                95
            );
            AssertIsValidView(percentileSection);
        }
    }
}