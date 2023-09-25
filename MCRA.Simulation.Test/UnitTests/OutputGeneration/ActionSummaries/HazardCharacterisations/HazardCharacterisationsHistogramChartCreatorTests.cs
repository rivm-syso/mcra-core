using MCRA.General;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.HazardCharacterisations {

    /// <summary>
    /// OutputGeneration, ActionSummaries, HazardCharacterisations
    /// </summary>
    [TestClass]
    public class HazardCharacterisationsHistogramChartCreatorTests : ChartCreatorTestBase {

        /// <summary>
        /// Create chart
        /// </summary>
        [TestMethod]
        public void HazardCharacterisationsHistogramChartCreator_TestCreate() {
            int seed = 1;
            var effect = MockEffectsGenerator.Create(1).First();
            var n = new[] { 0, 20, 50, 200 };
            for (int i = 0; i < n.Length; i++) {
                var substances = MockSubstancesGenerator.Create(n[i]);
                var hazardCharacterisations = MockHazardCharacterisationModelsGenerator.Create(effect, substances, seed: seed);

                var section = new HazardCharacterisationsSummarySection();
                section.Summarize(
                    effect,
                    substances,
                    hazardCharacterisations,
                    TargetLevelType.External,
                    ExposureType.Acute,
                    TargetDosesCalculationMethod.InVivoPods,
                    false,
                    false,
                    1,
                    false
                );
                var chart = new HazardCharacterisationsHistogramChartCreator(section, "unit", 400, 400);

                Assert.IsNotNull(chart);
                RenderChart(chart, $"TestCreate_{i}");
                AssertIsValidView(section);
            }
        }
    }
}
