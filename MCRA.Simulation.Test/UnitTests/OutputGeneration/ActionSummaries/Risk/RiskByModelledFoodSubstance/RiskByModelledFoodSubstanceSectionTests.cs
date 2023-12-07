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
            var expectedAtRiskDueToFoodSubstance = new List<double> { 16, 12, 12, 8, 8, 0 };
            var expectedAtRiskWithOrWithout = new List<double> { 68, 72, 72, 76, 76, 84 };
            var expectedNotAtRisk = new List<double> { 16, 16, 16, 16, 16, 16 };

            var ix = 0;
            foreach (var item in section.Records) {
                Assert.AreEqual(expectedNotAtRisk[ix], item.NotAtRisk);
                Assert.AreEqual(expectedAtRiskWithOrWithout[ix], item.AtRiskWithOrWithout);
                Assert.AreEqual(expectedAtRiskDueToFoodSubstance[ix], item.AtRiskDueToModelledFoodSubstance);
                ix++;
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
            var expectedAtRiskDueToFoodSubstance = new List<double> { 12, 12, 8, 4, 4, 4 };
            var expectedAtRiskWithOrWithout = new List<double> { 60, 60, 64, 68, 68, 68 };
            var expectedNotAtRisk = new List<double> { 28, 28, 28, 28, 28, 28 };

            var ix = 0;
            foreach (var item in section.Records) {
                Assert.AreEqual(expectedNotAtRisk[ix], item.NotAtRisk);
                Assert.AreEqual(expectedAtRiskWithOrWithout[ix], item.AtRiskWithOrWithout);
                Assert.AreEqual(expectedAtRiskDueToFoodSubstance[ix], item.AtRiskDueToModelledFoodSubstance);
                ix++;
            }
            AssertIsValidView(section);
        }
    }
}
