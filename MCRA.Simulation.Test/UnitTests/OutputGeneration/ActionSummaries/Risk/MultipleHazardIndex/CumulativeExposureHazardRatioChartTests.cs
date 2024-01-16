using MCRA.Utils.Statistics;
using MCRA.General;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.RiskCalculation;
using System.Collections.Generic;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.Risk {

    /// <summary>
    /// Tests for cumulative exposure-hazard ratio chart creator.
    /// </summary>
    [TestClass]
    public class CumulativeExposureHazardRatioChartTests : ChartCreatorTestBase {

        /// <summary>
        /// Create cumulative exposure hazard ratio chart (sum of substance risk percentiles).
        /// </summary>
        [TestMethod]
        public void CumulativeExposureHazardRatioChartCreator_TestCreateMedian() {
            var section = fakeCumulativeExposureHazardRatioSection();
            var chartCreator = new CumulativeExposureHazardRatioMedianChartCreator(section, false);
            RenderChart(chartCreator, "TestCreateMedian");
        }

        /// <summary>
        /// Create chart
        /// </summary>
        [TestMethod]
        public void CumulativeExposureHazardRatioChartCreator_TestCreateUpper() {
            var section = fakeCumulativeExposureHazardRatioSection();
            var chartCreator = new CumulativeExposureHazardRatioUpperChartCreator(section, false);
            RenderChart(chartCreator, "TestCreateUpper");
        }

        private static CumulativeExposureHazardRatioSection fakeCumulativeExposureHazardRatioSection() {
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
            var targetUnit = TargetUnit.FromExternalExposureUnit(ExternalExposureUnit.ugPerGBWPerDay, ExposurePathType.Dietary);
            var section = new CumulativeExposureHazardRatioSection();
            section.Summarize(
                new List<TargetUnit> { targetUnit },
                individualEffectsBySubstanceCollections: new List<(ExposureTarget Target, Dictionary<Compound, List<IndividualEffect>> IndividualEffects)> { (targetUnit.Target, individualEffects) },
                individualEffects: null,
                substances: substances,
                focalEffect: null,
                riskMetricCalculationType: RiskMetricCalculationType.RPFWeighted,
                riskMetricType: RiskMetricType.ExposureHazardRatio,
                confidenceInterval: 95,
                threshold: .1,
                leftMargin: 0.00001,
                rightMargin: 100,
                isInverseDistribution: false,
                isCumulative: true
            );
            return section;
        }
    }
}