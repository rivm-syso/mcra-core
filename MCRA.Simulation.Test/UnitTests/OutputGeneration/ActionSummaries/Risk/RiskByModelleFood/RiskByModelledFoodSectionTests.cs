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
            var individuals = MockIndividualsGenerator.Create(25, 1, random);
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
            var expectedAtRiskDueToFood = new List<double> { 16, 12, 8, 8, 4 };
            var expectedAtRiskWithOrWithout = new List<double> { 60, 64, 68, 68, 72 };
            var expectedNotAtRisk = new List<double> { 24, 24, 24, 24, 24 };

            var ix = 0;
            foreach (var item in section.Records) {
                Assert.AreEqual(expectedNotAtRisk[ix], item.NotAtRisk);
                Assert.AreEqual(expectedAtRiskWithOrWithout[ix], item.AtRiskWithOrWithout);
                Assert.AreEqual(expectedAtRiskDueToFood[ix], item.AtRiskDueToFood);
                ix++;
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
            var individuals = MockIndividualsGenerator.Create(25, 1, random);
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
            var expectedAtRiskDueToFood = new List<double> { 12, 12, 8, 8, 8 };
            var expectedAtRiskWithOrWithout = new List<double> { 60, 60, 64, 64, 64 };
            var expectedNotAtRisk = new List<double> { 28, 28, 28, 28, 28 };

            var ix = 0;
            foreach (var item in section.Records) {
                Assert.AreEqual(expectedNotAtRisk[ix], item.NotAtRisk);
                Assert.AreEqual(expectedAtRiskWithOrWithout[ix], item.AtRiskWithOrWithout);
                Assert.AreEqual(expectedAtRiskDueToFood[ix], item.AtRiskDueToFood);
                ix++;
            }
            AssertIsValidView(section);
        }
    }
}
