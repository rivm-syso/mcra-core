using MCRA.General;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using MCRA.Utils.Statistics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.Risk {

    [TestClass]
    public class ExposuresAndHazardsByAgeChartCreatorTests : ChartCreatorTestBase {

        [TestMethod]
        [DataRow(RiskMetricType.ExposureHazardRatio)]
        [DataRow(RiskMetricType.HazardExposureRatio)]
        public void ExposuresAndHazardsByAgeChartCreator_TestCreate(RiskMetricType riskMetricType) {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var effect = MockEffectsGenerator.Create(1).First();
            var substances = MockSubstancesGenerator.Create(1);
            var referenceCompound = substances.First();
            var individuals = MockIndividualsGenerator.Create(100, 1, random);

            var targetUnit = TargetUnit.FromExternalExposureUnit(ExternalExposureUnit.ugPerGBWPerDay, ExposureRoute.Oral);
            var hazardCharacterisationModel = MockHazardCharacterisationModelsGenerator
                .CreateSingle(effect, referenceCompound, 1.5, targetUnit.Target, targetUnit.ExposureUnit, ageDependent: true);

            var individualEffects = MockIndividualEffectsGenerator.Create(
                individuals,
                0.1,
                random,
                hazardCharacterisationModel.Value
            );

            var section = new ExposuresAndHazardsByAgeSection();
            section.Summarize(
                individualEffects,
                riskMetricType,
                targetUnit,
                hazardCharacterisationModel,
                false
            );

            var chartCreator = new ExposuresAndHazardsByAgeChartCreator(section);
            RenderChart(chartCreator, $"TestNominal_{riskMetricType}");
        }
    }
}