using MCRA.Data.Compiled.Objects;
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
            var effect = MockEffectsGenerator.Create(1).First();
            var n = new[] { 0, 20, 50, 200 };
            for (int i = 0; i < n.Length; i++) {
                var substances = MockSubstancesGenerator.Create(n[i]);
                var hazardCharacterisations = MockHazardCharacterisationModelsGenerator.Create(new Effect(), substances, seed: seed);
                var section = new AvailableHazardCharacterisationsSummarySection();
                section.Summarize(effect, hazardCharacterisations.Select(c => c.Value).ToList());
                var chart = new AvailableHazardCharacterisationsHistogramChartCreator(section, "unit", 400, 400);
                Assert.IsNotNull(chart);
                RenderChart(chart, $"TestCreate_{i}");
                AssertIsValidView(section);
            }
        }
    }
}
