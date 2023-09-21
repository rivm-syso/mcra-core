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

                var hazardCharacterisationModelsCollection = new List<HazardCharacterisationModelsCollection> {
                    new HazardCharacterisationModelsCollection {
                         HazardCharacterisationModels = MockHazardCharacterisationModelsGenerator.Create(effect, substances, seed: seed),
                         TargetUnit = TargetUnit.CreateDietaryExposureUnit(ConsumptionUnit.g, ConcentrationUnit.mgPerKg, BodyWeightUnit.kg, false)
                    }
                };

                var section = new HazardCharacterisationsSummarySection();
                section.Summarize(
                    effect, 
                    substances, 
                    hazardCharacterisationModelsCollection, 
                    TargetLevelType.External, 
                    ExposureType.Acute, 
                    TargetDosesCalculationMethod.InVivoPods, 
                    false, 
                    false,
                    1.0,
                    false
                    );
                var chart = new HazardCharacterisationsHistogramChartCreator(section.SectionId, section.Records, "unit", 400, 400);

                Assert.IsNotNull(chart);
                RenderChart(chart, $"TestCreate_{i}");
                AssertIsValidView(section);
            }
        }
    }
}
