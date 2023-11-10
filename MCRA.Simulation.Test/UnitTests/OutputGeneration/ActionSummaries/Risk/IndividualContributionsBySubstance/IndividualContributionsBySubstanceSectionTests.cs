using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.RiskCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using MCRA.Utils.Statistics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.Risk {
    /// <summary>
    ///  OutputGeneration, ActionSummaries, Risk, IndividualContributionsBySubstance
    /// </summary>
    [TestClass]
    public class IndividualContributionsBySubstanceSectionTests : ChartCreatorTestBase {

        /// <summary>
        /// Summarize, test RiskBySubstanceSection view
        /// </summary>
        [TestMethod]
        public void RiskBySubstanceSection_TestHIChart() {
            int seed = 1;
            var random = new McraRandomGenerator(seed);
            var individuals = MockIndividualsGenerator.Create(25, 1, random);
            var substances = MockSubstancesGenerator.Create(5);
            var individualEffectsByTargetSubstance = new List<(
                ExposureTarget Target, 
                Dictionary<Compound, List<IndividualEffect>> SubstanceIndividualEffects
            )>();
            var individualEffectsBySubstances = new Dictionary<Compound, List<IndividualEffect>>();
            foreach (var substance in substances) {
                individualEffectsBySubstances[substance] = MockIndividualEffectsGenerator.Create(individuals, 0.1, random);
            }
            individualEffectsByTargetSubstance.Add((ExposureTarget.DietaryExposureTarget, individualEffectsBySubstances));

            var riskCalculator = new RiskCalculator<TargetIndividualDayExposure>(HealthEffectType.Risk);
            var cumulativeIndividualEffects = riskCalculator
                .ComputeSumOfRatios(
                    individualEffectsByTargetSubstance.First().Item2,
                    substances.ToDictionary(r => r, r => 1D)
                );
            var section = new IndividualRiskContributionsBySubstanceSection();
            section.SummarizeBoxPlots(
                cumulativeIndividualEffects,
                individualEffectsByTargetSubstance
            );
            var chart = new IndividualContributionsBySubstanceBoxPlotChartCreator(section);
            RenderChart(chart, "ContributionsBoxPlot");
            AssertIsValidView(section);
        }
    }
}
