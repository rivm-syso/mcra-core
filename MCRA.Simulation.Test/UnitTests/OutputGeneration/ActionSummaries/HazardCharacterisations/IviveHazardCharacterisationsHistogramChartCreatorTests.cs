using MCRA.General;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.HazardCharacterisations {

    /// <summary>
    /// OutputGeneration, ActionSummaries, HazardCharacterisations
    /// </summary>
    [TestClass]
    public class IviveHazardCharacterisationsHistogramChartCreatorTests : ChartCreatorTestBase {

        /// <summary>
        /// Create chart
        /// </summary>
        [TestMethod]
        public void IviveHazardCharacterisationsHistogramChartCreator_TestCreate() {
            int seed = 1;
            var effect = FakeEffectsGenerator.Create(1).First();
            var targetUnit = TargetUnit.FromExternalExposureUnit(ExternalExposureUnit.ugPerKgBWPerDay, ExposureRoute.Oral);
            var internalTargetUnit = TargetUnit.FromExternalDoseUnit(DoseUnit.umoles, ExposureRoute.Oral);
            var n = new[] { 0, 20, 50, 200 };
            for (int i = 1; i < n.Length; i++) {
                var substances = FakeSubstancesGenerator.Create(n[i]);
                var hazardCharacterisations = FakeIviveHazardCharacterisationsGenerator
                    .Create(
                        substances,
                        ExposureType.Chronic,
                        targetUnit,
                        internalTargetUnit,
                        seed: seed
                    );
                var section = new IviveHazardCharacterisationsSummarySection();
                section.Summarize(
                    effect,
                    substances.FirstOrDefault(),
                    hazardCharacterisations.Select(c => c.Value).ToList(),
                    TargetLevelType.External
                );
                var chart = new IviveTargetDosesHistogramChartCreator(section, "unit", 400, 400);
                Assert.IsNotNull(chart);
                RenderChart(chart, $"TestCreate_{i}");
                AssertIsValidView(section);
            }
        }
    }
}
