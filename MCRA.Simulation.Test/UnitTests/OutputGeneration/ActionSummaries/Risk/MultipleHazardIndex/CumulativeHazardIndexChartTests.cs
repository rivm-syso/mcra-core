using MCRA.Utils.Statistics;
using MCRA.General;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.Risk {

    /// <summary>
    /// OutputGeneration, ActionSummaries, Risk, MultipleHazardIndex
    /// </summary>
    [TestClass]
    public class CumulativeHazardIndexChartTests : ChartCreatorTestBase {

        
        /// <summary>
        /// Create chart
        /// </summary>
        [TestMethod]
        public void CumulativeHazardIndexChartCreator_Test1() {
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
            var section = new MultipleHazardIndexSection();
            section.SummarizeMultipleSubstances(
                substanceIndividualEffects: individualEffects,
                cumulativeIndividualEffects: null,
                substances: substances,
                focalEffect: null,
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

            var chartCreator = new CumulativeHazardIndexMedianChartCreator(section, false);
            RenderChart(chartCreator, "TestCumulativeMedian");
            var chartCreator1 = new CumulativeHazardIndexUpperChartCreator(section, false);
            RenderChart(chartCreator1, "TestCumulativeUpper");
        }

    }
}