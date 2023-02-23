using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.RiskCalculation;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.Risk {
    /// <summary>
    ///  OutputGeneration, ActionSummaries, Risk, RiskBySubstance
    /// </summary>
    [TestClass]
    public class RiskBySubstanceSectionTests : SectionTestBase {

        /// <summary>
        /// Summarize, test RiskBySubstanceSection view
        /// </summary>
        [TestMethod]
        public void RiskBySubstanceSection_TestHI() {
            int seed = 1;
            var random = new McraRandomGenerator(seed);
            var individuals = MockIndividualsGenerator.Create(25, 1, random);
            var substances = MockSubstancesGenerator.Create(5);
            var individualEffectsBySubstances = new Dictionary<Compound, List<IndividualEffect>>();
            foreach (var substance in substances) {
                individualEffectsBySubstances[substance] = MockIndividualEffectsGenerator.Create(individuals, 0.1, random);
            }
            var section = new SubstancesAtRiskSection() { };
            section.SummarizeSubstancesAtRisk(
                individualEffects: individualEffectsBySubstances,
                numberOfCumulativeIndividualEffects: 25,
                healthEffectType: HealthEffectType.Risk,
                riskMetric: RiskMetricType.HazardIndex,
                threshold: 3
            );
            var expectedAtRiskDueToSubstance = new List<double> { 16, 12, 8, 8, 4 };
            var expectedAtRiskWithOrWithout = new List<double> { 60, 64, 68, 68, 72 };
            var expectedNotAtRisk = new List<double> { 24, 24, 24, 24, 24 };

            var ix = 0;
            foreach (var item in section.Records) {
                Assert.AreEqual(expectedNotAtRisk[ix], item.NotAtRisk);
                Assert.AreEqual(expectedAtRiskWithOrWithout[ix], item.AtRiskWithOrWithout);
                Assert.AreEqual(expectedAtRiskDueToSubstance[ix], item.AtRiskDueToSubstance);
                ix++;
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
            var individuals = MockIndividualsGenerator.Create(25, 1, random);
            var substances = MockSubstancesGenerator.Create(5);
            var individualEffectsBySubstances = new Dictionary<Compound, List<IndividualEffect>>();
            foreach (var substance in substances) {
                individualEffectsBySubstances[substance] = MockIndividualEffectsGenerator.Create(individuals, 0.1, random);
            }
            var section = new SubstancesAtRiskSection() { };
            section.SummarizeSubstancesAtRisk(
                individualEffects: individualEffectsBySubstances,
                numberOfCumulativeIndividualEffects: 25,
                healthEffectType: HealthEffectType.Risk,
                riskMetric: RiskMetricType.MarginOfExposure,
                threshold: 0.3
            );
            var expectedAtRiskDueToSubstance = new List<double> { 12, 12, 8, 8, 8 };
            var expectedAtRiskWithOrWithout = new List<double> { 60, 60, 64, 64, 64 };
            var expectedNotAtRisk = new List<double> { 28, 28, 28, 28, 28 };

            var ix = 0;
            foreach (var item in section.Records) {
                Assert.AreEqual(expectedNotAtRisk[ix], item.NotAtRisk);
                Assert.AreEqual(expectedAtRiskWithOrWithout[ix], item.AtRiskWithOrWithout);
                Assert.AreEqual(expectedAtRiskDueToSubstance[ix], item.AtRiskDueToSubstance);
                ix++;
            }
            AssertIsValidView(section);
        }
    }
}
