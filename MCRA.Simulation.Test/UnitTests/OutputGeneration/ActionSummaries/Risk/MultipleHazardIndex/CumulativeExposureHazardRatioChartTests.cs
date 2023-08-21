using MCRA.Utils.Statistics;
using MCRA.General;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.Risk {

    /// <summary>
    /// Tests for cumulative exposure-hazard ratio chart creator.
    /// </summary>
    [TestClass]
    public class CumulativeExposureHazardRatioChartTests : ChartCreatorTestBase {

        /// <summary>
        /// Create chart
        /// </summary>
        [TestMethod]
        public void CumulativeExposureHazardRatioChartCreator_TestCreate() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = MockSubstancesGenerator.Create(20);
            var individuals = MockIndividualsGenerator.Create(10, 2, random);

            var individualEffects = MockIndividualEffectsGenerator
                .Create(
                    individuals,
                    substances,
                    random
                );
            var section = new MultipleExposureHazardRatioSection();
            section.SummarizeMultipleSubstances(
                individualEffectsBySubstance: individualEffects,
                individualEffects: null,
                substances: substances,
                focalEffect: null,
                riskMetricCalculationType: RiskMetricCalculationType.RPFWeighted,
                riskMetricType: RiskMetricType.HazardIndex,
                confidenceInterval: 95,
                threshold: .1,
                healthEffectType: HealthEffectType.Risk,
                leftMargin: 0.00001,
                rightMargin: 100,
                isInverseDistribution: false,
                useIntraSpeciesFactor: false,
                isCumulative: true,
                onlyCumulativeOutput: false
            );

            var chartCreator = new CumulativeExposureHazardRatioMedianChartCreator(section, false);
            RenderChart(chartCreator, "TestCumulativeMedian");
            var chartCreator1 = new CumulativeExposureHazardRatioUpperChartCreator(section, false);
            RenderChart(chartCreator1, "TestCumulativeUpper");
        }
    }
}