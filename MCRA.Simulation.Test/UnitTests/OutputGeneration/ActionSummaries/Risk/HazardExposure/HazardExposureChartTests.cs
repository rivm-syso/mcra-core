using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
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
            var individuals = MockIndividualsGenerator.Create(100, 1, random);

            var hazardCharacterisations = MockHazardCharacterisationModelsGenerator.Create(new Effect(), substances, seed);

            var individualEffects = MockIndividualEffectsGenerator.Create(individuals, substances, random);

            var reference = substances.First();
            var cumulativeMarginOfExposure = MockIndividualEffectsGenerator.ComputeCumulativeIndividualEffects(
                individuals,
                individualEffects,
                reference
            );

            var section = new HazardExposureSection();
            section.Summarize(
                individualEffectsBySubstance: individualEffects,
                individualEffects: cumulativeMarginOfExposure,
                healthEffectType: HealthEffectType.Risk,
                substances: substances,
                referenceSubstance: reference,
                hazardCharacterisations: hazardCharacterisations,
                riskMetricType: RiskMetricType.MarginOfExposure,
                riskMetricCalculationType: RiskMetricCalculationType.RPFWeighted,
                confidenceInterval: 90,
                thresholdMarginOfExposure: 1,
                numberOfLabels: 10,
                numberOfSubstances: 10,
                uncertaintyLowerBound: 2.5,
                uncertaintyUpperBound: 97.5
            );

            var chart0 = new HazardExposure_TraditionalChartCreator(section, "microg/kg");
            RenderChart(chart0, "TestNominal_Chart0");

            var chart1a = new HazardExposure_ExpvsCed100ExpChartCreator(section, "microg/kg");
            RenderChart(chart1a, "TestNominal_Chart1a");

            var chart1b = new HazardExposure_ExpvsLowerCedChartCreator(section, "microg/kg");
            RenderChart(chart1b, "TestNominal_Chart1b");

            var chart2 = new HazardExposure_CedvsUpperExpChartCreator(section, "microg/kg");
            RenderChart(chart2, "TestNominal_Chart2");

            var chart3a = new HazardExposure_CedExpvsCed100ChartCreator(section, "microg/kg");
            RenderChart(chart3a, "TestNominal_Chart3a");

            var chart3b = new HazardExposure_CedExpvsLowerCedChartCreator(section, "microg/kg");
            RenderChart(chart3b, "TestNominal_Chart3b");

            var chart4 = new HazardExposure_MOEvsUpperExpLowerCedChartCreator(section, "microg/kg");
            RenderChart(chart4, "TestNominal_Chart4");

            var chart5a = new HazardExposure_MOEExpCedvsUpperExpCed100ChartCreator(section, "microg/kg");
            RenderChart(chart5a, "TestNominal_Chart5a");

            var chart5b = new HazardExposure_MOEExpCedvsUpperExpLowerCedChartCreator(section, "microg/kg");
            RenderChart(chart5b, "TestNominal_Chart5b");

            var chart5c = new HazardExposure_EllipsChartCreator(section, "microg/kg", false);
            RenderChart(chart5c, "TestNominal_Chart5c");

            var chart5d = new HazardExposure_EllipsChartCreator(section, "microg/kg", true);
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
            var individuals = MockIndividualsGenerator.Create(100, 1, random);

            var hazardCharacterisations = MockHazardCharacterisationModelsGenerator.Create(new Effect(), substances, seed);

            var individualEffects = MockIndividualEffectsGenerator.Create(individuals, substances, random);

            var reference = substances.First();
            var cumulativeMarginOfExposure = MockIndividualEffectsGenerator.ComputeCumulativeIndividualEffects(
                individuals,
                individualEffects,
                reference
            );

            var section = new HazardExposureSection();
            section.Summarize(
                individualEffects,
                cumulativeMarginOfExposure,
                HealthEffectType.Risk,
                substances,
                reference,
                hazardCharacterisations,
                riskMetricType: RiskMetricType.MarginOfExposure,
                riskMetricCalculationType: RiskMetricCalculationType.RPFWeighted,
                90,
                1,
                10,
                10,
                2.5,
                97.5
            );

            for (int i = 0; i < 100; i++) {
                var substanceIndividualEffectsUncertains = MockIndividualEffectsGenerator
                    .CreateUncertain(substances, individualEffects, random);
                var cumulativeMarginOfExposureUncertains = MockIndividualEffectsGenerator
                    .ComputeCumulativeIndividualEffects(individuals, individualEffects, reference);

                section.SummarizeUncertainty(
                    substanceIndividualEffectsUncertains,
                    cumulativeMarginOfExposureUncertains,
                    hazardCharacterisations,
                    substances,
                    reference,
                    riskMetricType: RiskMetricType.MarginOfExposure,
                    riskMetricCalculationType: RiskMetricCalculationType.RPFWeighted,
                    2.5,
                    97.5
                );
            }

            var chart0 = new HazardExposure_TraditionalChartCreator(section, "microg/kg");
            RenderChart(chart0, "TestUncertain_Chart0");

            var chart1a = new HazardExposure_ExpvsCed100ExpChartCreator(section, "microg/kg");
            RenderChart(chart1a, "TestUncertain_Chart1a");

            var chart1b = new HazardExposure_ExpvsLowerCedChartCreator(section, "microg/kg");
            RenderChart(chart1b, "TestUncertain_Chart1b");

            var chart2 = new HazardExposure_CedvsUpperExpChartCreator(section, "microg/kg");
            RenderChart(chart2, "TestUncertain_Chart2");

            var chart3a = new HazardExposure_CedExpvsCed100ChartCreator(section, "microg/kg");
            RenderChart(chart3a, "TestUncertain_Chart3a");

            var chart3b = new HazardExposure_CedExpvsLowerCedChartCreator(section, "microg/kg");
            RenderChart(chart3b, "TestUncertain_Chart3b");

            var chart4 = new HazardExposure_MOEvsUpperExpLowerCedChartCreator(section, "microg/kg");
            RenderChart(chart4, "TestUncertain_Chart4");

            var chart5a = new HazardExposure_MOEExpCedvsUpperExpCed100ChartCreator(section, "microg/kg");
            RenderChart(chart5a, "TestUncertain_Chart5a");

            var chart5b = new HazardExposure_MOEExpCedvsUpperExpLowerCedChartCreator(section, "microg/kg");
            RenderChart(chart5b, "TestUncertain_Chart5b");

            var chart5c = new HazardExposure_EllipsChartCreator(section, "microg/kg", false);
            RenderChart(chart5c, "TestUncertain_Chart5c");

            var chart5d = new HazardExposure_EllipsChartCreator(section, "microg/kg", true);
            RenderChart(chart5d, "TestUncertain_Chart5d");
            AssertIsValidView(section);
        }
    }
}