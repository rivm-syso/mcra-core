using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.HazardCharacterisationCalculation;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.HazardCharacterisations {

    /// <summary>
    /// OutputGeneration, ActionSummaries, HazardCharacterisations
    /// </summary>
    [TestClass]
    public class AvailableHazardCharacterisationsHistogramChartCreatorTests : ChartCreatorTestBase {

        /// <summary>
        /// Create chart
        /// </summary>
        [TestMethod]
        public void AvailableHazardCharacterisationsHistogramChartCreator_TestCreate() {
            int seed = 1;
            var targetUnit = new TargetUnit(ExposureTarget.DietaryExposureTarget, SubstanceAmountUnit.Milligrams, ConcentrationMassUnit.Kilograms);
            var effect = MockEffectsGenerator.Create(1).First();
            var n = new[] { 0, 20, 50, 200 };
            for (int i = 0; i < n.Length; i++) {
                var substances = MockSubstancesGenerator.Create(n[i]);
                var hazardCharacterisations = MockHazardCharacterisationModelsGenerator.Create(new Effect(), substances, seed: seed);
                var section = new AvailableHazardCharacterisationsSummarySection();

                var records = hazardCharacterisations.Select(c => c.Value).ToList();
                section.Summarize(effect, [
                    new HazardCharacterisationModelsCollection {
                        TargetUnit = targetUnit,
                        HazardCharacterisationModels = records} ]);

                var chart = new AvailableHazardCharacterisationsHistogramChartCreator(section.SectionId, section.Records, "unit", 400, 400);
                Assert.IsNotNull(chart);
                RenderChart(chart, $"TestCreate_{i}");
                AssertIsValidView(section);
            }
        }
    }
}
