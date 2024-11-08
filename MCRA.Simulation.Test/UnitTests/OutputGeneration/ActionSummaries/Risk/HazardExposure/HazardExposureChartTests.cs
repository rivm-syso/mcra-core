using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.HazardCharacterisationCalculation;
using MCRA.Simulation.Calculators.RiskCalculation;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using MCRA.Utils.Statistics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.Risk {

    /// <summary>
    /// OutputGeneration, ActionSummaries, Risk, HazardExposure
    /// </summary>
    [TestClass]
    public class HazardExposureChartTests : ChartCreatorTestBase {

        /// <summary>
        /// Create charts CEDExposurePerSubstanceTable
        /// </summary>
        [TestMethod]
        public void HazardExposureChart_TestNominal() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = MockSubstancesGenerator.Create(5);
            var individuals = FakeIndividualsGenerator.Create(100, 1, random);

            var targetUnit = TargetUnit.FromExternalExposureUnit(ExternalExposureUnit.ugPerGBWPerDay, ExposureRoute.Oral);
            var hazardCharacterisations = MockHazardCharacterisationModelsGenerator.Create(new Effect(), substances, seed);
            var hazardCharacterisationCollection = new HazardCharacterisationModelCompoundsCollection() {
                TargetUnit = targetUnit,
                HazardCharacterisationModels = hazardCharacterisations
            };
            var hazardCharacterisationCollections = new List<HazardCharacterisationModelCompoundsCollection> { hazardCharacterisationCollection };

            var individualEffects = MockIndividualEffectsGenerator.Create(individuals, substances, random);
                
            var reference = substances.First();
            var cumulativeHazardExposureRatio = MockIndividualEffectsGenerator.ComputeCumulativeIndividualEffects(
                individuals,
                individualEffects,
                reference
            );

            var individualEffectsBySubstanceCollections = new List<(ExposureTarget Target, Dictionary<Compound, List<IndividualEffect>> IndividualEffects)> {
                (targetUnit.Target, individualEffects)
            };
            var section = new HazardExposureSection();
            section.Summarize(
                [targetUnit.Target],
                individualEffectsBySubstanceCollections,
                cumulativeHazardExposureRatio,
                HealthEffectType.Risk,
                substances,
                hazardCharacterisationCollections,
                hazardCharacterisations[reference],
                riskMetricType: RiskMetricType.HazardExposureRatio,
                riskMetricCalculationType: RiskMetricCalculationType.RPFWeighted,
                confidenceInterval: 90,
                threshold: 1,
                numberOfLabels: 10,
                uncertaintyLowerBound: 2.5,
                uncertaintyUpperBound: 97.5,
                skipPrivacySensitiveOutputs: false,
                isCumulative: false
            );

            var chart0 = new HazardExposure_TraditionalChartCreator(section, targetUnit);
            RenderChart(chart0, "TestNominal_Chart0");

            var chart1a = new HazardExposure_ExpvsCed100ExpChartCreator(section, targetUnit);
            RenderChart(chart1a, "TestNominal_Chart1a");

            var chart1b = new HazardExposure_ExpvsLowerCedChartCreator(section, targetUnit);
            RenderChart(chart1b, "TestNominal_Chart1b");

            var chart2 = new HazardExposure_CedvsUpperExpChartCreator(section, targetUnit);
            RenderChart(chart2, "TestNominal_Chart2");

            var chart3a = new HazardExposure_CedExpvsCed100ChartCreator(section, targetUnit);
            RenderChart(chart3a, "TestNominal_Chart3a");

            var chart3b = new HazardExposure_CedExpvsLowerCedChartCreator(section, targetUnit);
            RenderChart(chart3b, "TestNominal_Chart3b");

            var chart4 = new HazardExposure_MOEvsUpperExpLowerCedChartCreator(section, targetUnit);
            RenderChart(chart4, "TestNominal_Chart4");

            var chart5a = new HazardExposure_MOEExpCedvsUpperExpCed100ChartCreator(section, targetUnit);
            RenderChart(chart5a, "TestNominal_Chart5a");

            var chart5b = new HazardExposureTERExpCedvsUpperExpLowerCedChartCreator(section, targetUnit);
            RenderChart(chart5b, "TestNominal_Chart5b");

            var chart5c = new HazardExposure_EllipsChartCreator(section, targetUnit, false);
            RenderChart(chart5c, "TestNominal_Chart5c");

            var chart5d = new HazardExposure_EllipsChartCreator(section, targetUnit, true);
            RenderChart(chart5d, "TestNominal_Chart5d");
            AssertIsValidView(section);
        }

        /// <summary>
        /// Create charts CEDExposurePerSubstanceTable
        /// </summary>
        [TestMethod]
        public void HazardExposureChart_TestUncertain() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = MockSubstancesGenerator.Create(5);
            var individuals = FakeIndividualsGenerator.Create(100, 1, random);
            var targetUnit = TargetUnit.FromExternalExposureUnit(ExternalExposureUnit.ugPerGBWPerDay, ExposureRoute.Oral);
            var hazardCharacterisations = MockHazardCharacterisationModelsGenerator.Create(new Effect(), substances, seed);
            var hazardCharacterisationCollection = new HazardCharacterisationModelCompoundsCollection() {
                TargetUnit = targetUnit,
                HazardCharacterisationModels = hazardCharacterisations
            };
            var hazardCharacterisationCollections = new List<HazardCharacterisationModelCompoundsCollection> { hazardCharacterisationCollection };
            var individualEffects = MockIndividualEffectsGenerator.Create(individuals, substances, random);

            var reference = substances.First();
            var cumulativeHazardExposureRatio = MockIndividualEffectsGenerator.ComputeCumulativeIndividualEffects(
                individuals,
                individualEffects,
                reference
            );
            var individualEffectsBySubstanceCollections = new List<(ExposureTarget Target, Dictionary<Compound, List<IndividualEffect>> IndividualEffects)> {
                (targetUnit.Target, individualEffects)
            };
            var section = new HazardExposureSection();
            section.Summarize(
                [targetUnit.Target],
                individualEffectsBySubstanceCollections,
                cumulativeHazardExposureRatio,
                HealthEffectType.Risk,
                substances,
                hazardCharacterisationCollections,
                hazardCharacterisations[reference],
                riskMetricType: RiskMetricType.HazardExposureRatio,
                riskMetricCalculationType: RiskMetricCalculationType.RPFWeighted,
                confidenceInterval: 90,
                threshold: 1,
                numberOfLabels: 10,
                uncertaintyLowerBound: 2.5,
                uncertaintyUpperBound: 97.5,
                skipPrivacySensitiveOutputs: false,
                isCumulative: true
            );

            for (int i = 0; i < 100; i++) {
                var substanceIndividualEffectsUncertains = MockIndividualEffectsGenerator
                    .CreateUncertain(substances, individualEffects, random);
                var cumulativeHazardExposureRatioUncertains = MockIndividualEffectsGenerator
                    .ComputeCumulativeIndividualEffects(individuals, individualEffects, reference);
                var individualEffectsBySubstanceCollectionsUncertains = new List<(ExposureTarget Target, Dictionary<Compound, List<IndividualEffect>> IndividualEffects)> {
                    (targetUnit.Target, substanceIndividualEffectsUncertains)
                };
                section.SummarizeUncertainty(
                    individualEffectsBySubstanceCollections.Select(r => r.Target).ToList(),
                    individualEffectsBySubstanceCollectionsUncertains,
                    cumulativeHazardExposureRatioUncertains,
                    hazardCharacterisationCollections,
                    substances,
                    hazardCharacterisations[reference],
                    riskMetricCalculationType: RiskMetricCalculationType.RPFWeighted,
                    riskMetricType: RiskMetricType.HazardExposureRatio,
                    2.5,
                    97.5,
                    true
                );
            }

            var chart0 = new HazardExposure_TraditionalChartCreator(section, targetUnit);
            RenderChart(chart0, "TestUncertain_Chart0");

            var chart1a = new HazardExposure_ExpvsCed100ExpChartCreator(section, targetUnit);
            RenderChart(chart1a, "TestUncertain_Chart1a");

            var chart1b = new HazardExposure_ExpvsLowerCedChartCreator(section, targetUnit);
            RenderChart(chart1b, "TestUncertain_Chart1b");

            var chart2 = new HazardExposure_CedvsUpperExpChartCreator(section, targetUnit);
            RenderChart(chart2, "TestUncertain_Chart2");

            var chart3a = new HazardExposure_CedExpvsCed100ChartCreator(section, targetUnit);
            RenderChart(chart3a, "TestUncertain_Chart3a");

            var chart3b = new HazardExposure_CedExpvsLowerCedChartCreator(section, targetUnit);
            RenderChart(chart3b, "TestUncertain_Chart3b");

            var chart4 = new HazardExposure_MOEvsUpperExpLowerCedChartCreator(section, targetUnit);
            RenderChart(chart4, "TestUncertain_Chart4");

            var chart5a = new HazardExposure_MOEExpCedvsUpperExpCed100ChartCreator(section, targetUnit);
            RenderChart(chart5a, "TestUncertain_Chart5a");

            var chart5b = new HazardExposureTERExpCedvsUpperExpLowerCedChartCreator(section, targetUnit);
            RenderChart(chart5b, "TestUncertain_Chart5b");

            var chart5c = new HazardExposure_EllipsChartCreator(section, targetUnit, false);
            RenderChart(chart5c, "TestUncertain_Chart5c");

            var chart5d = new HazardExposure_EllipsChartCreator(section, targetUnit, true);
            RenderChart(chart5d, "TestUncertain_Chart5d");
            AssertIsValidView(section);
        }
    }
}