using MCRA.Utils.Statistics;
using MCRA.General;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
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
            var substances = MockSubstancesGenerator.Create(3);
            var rpfs = substances.ToDictionary(r => r, r => 1d);
            var memberships = substances.ToDictionary(r => r, r => 1d);
            var foods = MockFoodsGenerator.Create(3);
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(20, 2, true, random);
            var dietaryIndividualDayIntakes = MockDietaryIndividualDayIntakeGenerator.Create(individualDays, foods, substances, 0.5, true, random);
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
                new double[] { 005, .1, },
                new double[] { 50, 90, 95 },
                95,
                false
            );
            subHeader = header.GetSubSectionHeader<IntakePercentileSection>();
            var percentileSection = subHeader.GetSummarySection() as IntakePercentileSection;
            Assert.AreEqual(0.267, percentileSection.Percentiles[0].ReferenceValue, 1e-3);
            section.SummarizeUncertainty(
                header,
                dietaryObservedIndividualMeans,
                5,
                95);
            AssertIsValidView(percentileSection);
        }
    }
}