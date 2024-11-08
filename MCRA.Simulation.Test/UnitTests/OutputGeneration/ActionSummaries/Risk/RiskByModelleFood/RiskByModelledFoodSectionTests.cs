using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.RiskCalculation;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.Risk {
    /// <summary>
    ///  OutputGeneration, ActionSummaries, Risk, RiskByModelledFood
    /// </summary>
    [TestClass]
    public class RiskByModelledFoodSectionTests : SectionTestBase {

        /// <summary>
        /// Summarize, test RiskByModelledFoodSection view
        /// </summary>
        [TestMethod]
        public void RiskByModelledFoodSection_TestHI() {
            int seed = 1;
            var random = new McraRandomGenerator(seed);
            var individuals = FakeIndividualsGenerator.Create(25, 1, random);
            var foods = MockFoodsGenerator.Create(5);
            var individualEffectsByFoods = new Dictionary<Food, List<IndividualEffect>>();
            foreach (var food in foods) {
                individualEffectsByFoods[food] = MockIndividualEffectsGenerator.Create(individuals, 0.1, random);
            }
            var section = new ModelledFoodsAtRiskSection() { };
            section.SummarizeModelledFoodsAtRisk(
                individualEffectsByFoods,
                25,
                HealthEffectType.Risk,
                RiskMetricType.ExposureHazardRatio,
                3
            );

            foreach (var item in section.Records) {
                Assert.IsTrue(item.NotAtRisk > 0);
                Assert.IsTrue(item.AtRiskWithOrWithout > 0);
                Assert.IsTrue(item.AtRiskDueToFood > 0);
            }
            AssertIsValidView(section);
        }

        /// <summary>
        /// Summarize, test RiskByModelledFoodSection view
        /// </summary>
        [TestMethod]
        public void RiskByModelledFoodSection_TestMOE() {
            int seed = 1;
            var random = new McraRandomGenerator(seed);
            var individuals = FakeIndividualsGenerator.Create(25, 1, random);
            var foods = MockFoodsGenerator.Create(5);
            var individualEffectsByFoods = new Dictionary<Food, List<IndividualEffect>>();
            foreach (var food in foods) {
                individualEffectsByFoods[food] = MockIndividualEffectsGenerator.Create(individuals, 0.1, random);
            }
            var section = new ModelledFoodsAtRiskSection() { };
            section.SummarizeModelledFoodsAtRisk(
                individualEffectsByFoods,
                25,
                HealthEffectType.Risk,
                RiskMetricType.HazardExposureRatio,
                0.3
            );

            foreach (var item in section.Records) {
                Assert.IsTrue(item.NotAtRisk > 0);
                Assert.IsTrue(item.AtRiskWithOrWithout > 0);
                Assert.IsTrue(item.AtRiskDueToFood > 0);
            }
            AssertIsValidView(section);
        }
    }
}
