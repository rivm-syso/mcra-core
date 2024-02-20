using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.RiskCalculation;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.Risk {
    /// <summary>
    ///  OutputGeneration, ActionSummaries, Risk, RiskByModelledFoodSubstanceSection
    /// </summary>
    [TestClass]
    public class RiskByModelledFoodSubstanceSectionTests : SectionTestBase {

        /// <summary>
        /// Summarize, test RiskByModelledFoodSection view
        /// </summary>
        [TestMethod]
        public void RiskByModelledFoodSubstanceSection_TestHI() {
            int seed = 1;
            var random = new McraRandomGenerator(seed);
            var individuals = MockIndividualsGenerator.Create(25, 1, random);
            var foods = MockFoodsGenerator.Create(2);
            var substances = MockSubstancesGenerator.Create(3);
            var individualEffectsByFoodSubstance = new Dictionary<(Food, Compound), List<IndividualEffect>>();
            foreach (var food in foods) {
                foreach (var substance in substances) {
                    individualEffectsByFoodSubstance[(food, substance)] = MockIndividualEffectsGenerator.Create(individuals, 0.1, random);
                }
            }
            var section = new ModelledFoodSubstancesAtRiskSection() { };
            section.SummarizeModelledFoodSubstancesAtRisk(
                individualEffectsByFoodSubstance,
                25,
                HealthEffectType.Risk,
                RiskMetricType.ExposureHazardRatio,
                4
            );

            foreach (var item in section.Records) {
                Assert.IsTrue(item.NotAtRisk >= 0);
                Assert.IsTrue(item.AtRiskWithOrWithout >= 0);
                Assert.IsTrue(item.AtRiskDueToModelledFoodSubstance >= 0);
            }
            AssertIsValidView(section);
        }

        /// <summary>
        /// Summarize, test RiskByModelledFoodSubstanceSection view
        /// </summary>
        [TestMethod]
        public void RiskByModelledFoodSubstanceSection_TestMOE() {
            int seed = 1;
            var random = new McraRandomGenerator(seed);
            var individuals = MockIndividualsGenerator.Create(25, 1, random);
            var foods = MockFoodsGenerator.Create(2);
            var substances = MockSubstancesGenerator.Create(3);
            var individualEffectsByFoodSubstance = new Dictionary<(Food, Compound), List<IndividualEffect>>();
            foreach (var food in foods) {
                foreach (var substance in substances) {
                    individualEffectsByFoodSubstance[(food, substance)] = MockIndividualEffectsGenerator.Create(individuals, 0.1, random);
                }
            }
            var section = new ModelledFoodSubstancesAtRiskSection() { };
            section.SummarizeModelledFoodSubstancesAtRisk(
                individualEffectsByFoodSubstance, 
                25,
                HealthEffectType.Risk, 
                RiskMetricType.HazardExposureRatio, 
                0.2
            );

            foreach (var item in section.Records) {
                Assert.IsTrue(item.NotAtRisk >= 0);
                Assert.IsTrue(item.AtRiskWithOrWithout >= 0);
                Assert.IsTrue(item.AtRiskDueToModelledFoodSubstance >= 0);
            }
            AssertIsValidView(section);
        }
    }
}
