using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.RiskCalculation;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.Risk {
    /// <summary>
    ///  OutputGeneration, ActionSummaries, Risk, RiskBySubstance
    /// </summary>
    [TestClass]
    public class RiskBySubstanceSectionTests : ChartCreatorTestBase {

        /// <summary>
        /// Summarize, test RiskBySubstanceSection view
        /// </summary>
        [TestMethod]
        public void RiskBySubstanceSection_TestHI() {
            int seed = 1;
            var random = new McraRandomGenerator(seed);
            var individuals = FakeIndividualsGenerator.CreateSimulated(25, 1, random);
            var substances = FakeSubstancesGenerator.Create(5);
            var individualEffectsBySubstances = new Dictionary<Compound, List<IndividualEffect>>();
            foreach (var substance in substances) {
                individualEffectsBySubstances[substance] = FakeIndividualEffectsGenerator.Create(individuals, 0.1, random);
            }
            var section = new SubstancesAtRiskSection() { };
            section.SummarizeSubstancesAtRisk(
                individualEffects: individualEffectsBySubstances,
                numberOfCumulativeIndividualEffects: 25,
                healthEffectType: HealthEffectType.Risk,
                riskMetric: RiskMetricType.ExposureHazardRatio,
                threshold: 3
            );

            foreach (var item in section.Records) {
                Assert.IsTrue(item.NotAtRisk > 0);
                Assert.IsTrue(item.AtRiskWithOrWithout > 0);
                Assert.IsTrue(item.AtRiskDueToSubstance > 0);
            }
            AssertIsValidView(section);
        }

        /// <summary>
        /// Summarize, test RiskBySubstanceSection view
        /// </summary>
        [TestMethod]
        public void RiskBySubstanceSection_TestMOE() {
            int seed = 1;
            var random = new McraRandomGenerator(seed);
            var individuals = FakeIndividualsGenerator.CreateSimulated(25, 1, random);
            var substances = FakeSubstancesGenerator.Create(5);
            var individualEffectsBySubstances = new Dictionary<Compound, List<IndividualEffect>>();
            foreach (var substance in substances) {
                individualEffectsBySubstances[substance] = FakeIndividualEffectsGenerator.Create(individuals, 0.1, random);
            }
            var section = new SubstancesAtRiskSection() { };
            section.SummarizeSubstancesAtRisk(
                individualEffects: individualEffectsBySubstances,
                numberOfCumulativeIndividualEffects: 25,
                healthEffectType: HealthEffectType.Risk,
                riskMetric: RiskMetricType.HazardExposureRatio,
                threshold: 0.3
            );

            foreach (var item in section.Records) {
                Assert.IsTrue(item.NotAtRisk > 0);
                Assert.IsTrue(item.AtRiskWithOrWithout > 0);
                Assert.IsTrue(item.AtRiskDueToSubstance > 0);
            }
            AssertIsValidView(section);
        }
    }
}
