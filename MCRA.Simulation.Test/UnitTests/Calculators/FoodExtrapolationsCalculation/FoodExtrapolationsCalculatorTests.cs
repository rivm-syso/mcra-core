using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.FoodExtrapolationsCalculation;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Calculators.FoodExtrapolationCalculation {
    /// <summary>
    /// FoodExtrapolationCalculation calculator
    /// </summary>
    [TestClass]
    public class FoodExtrapolationCalculatorTests {

        internal class MockFoodExtrapolationCandidatesCalculatorSettings : IFoodExtrapolationCandidatesCalculatorSettings {
            public int ThresholdForExtrapolation { get; set; }
            public bool ConsiderAuthorisationsForExtrapolations { get; set; }
            public bool ConsiderMrlForExtrapolations { get; set; }
        }

        /// <summary>
        /// Calculates sample substance extrapolation candidates, needs further implementation by Johannes
        /// </summary>
        [TestMethod]
        public void FoodExtrapolationCalculator_TestCreateExtrapolationRecords() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = MockFoodsGenerator.Create(10);
            var substances = MockSubstancesGenerator.Create(4);
            var sampleCompoundsCollections = MockSampleCompoundCollectionsGenerator
                .Create(foods.Take(4).ToList(), substances, random);
            var maximumConcentrationLimits = MockMaximumConcentrationLimitsGenerator.Create(foods, substances, random);
            var substanceAuthorisations = MockSubstanceAuthorisationsGenerator.Create(foods, substances);
            var substanceAuthorisationsDict = new Dictionary<(Food, Compound), SubstanceAuthorisation>();
            foreach (var item in substanceAuthorisations) {
                substanceAuthorisationsDict[(item.Food, item.Substance)] = item;
            }
            var possibleExtrapolations = new Dictionary<Food, List<FoodSubstanceExtrapolationCandidate>> {
                [foods[2]] = new List<FoodSubstanceExtrapolationCandidate>() {
                new FoodSubstanceExtrapolationCandidate() {
                    ExtrapolationFood = foods[6],
                    MeasuredSubstance = substances[2],
                },
            }
            };
            var extrapolationsCandidates = new List<FoodSubstanceExtrapolationCandidates>() {
                new FoodSubstanceExtrapolationCandidates() {
                    Food = foods[2],
                    Substance = substances.Last(),
                    Measurements= 4,
                    PossibleExtrapolations= possibleExtrapolations,
                }
            };

            var result = SampleCompoundExtrapolationCalculator.CreateExtrapolationRecords(
                sampleCompoundsCollections,
                maximumConcentrationLimits,
                substanceAuthorisationsDict,
                extrapolationsCandidates
            );
            Assert.AreEqual("Cherries", result.Select(c => c.Food).First().Name);
        }

        /// <summary>
        /// Calculates food extrapolation candidates, needs further implementation by Johannes
        /// </summary>
        [TestMethod]
        public void FoodExtrapolationCalculator_TestComputeExtrapolationCandidates() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = MockFoodsGenerator.Create(10);
            var substances = MockSubstancesGenerator.Create(4);
            var sampleCompoundsCollections = MockSampleCompoundCollectionsGenerator
                .Create(foods.Take(4).ToList(), substances, random);
            var maximumConcentrationLimits = MockMaximumConcentrationLimitsGenerator.Create(foods, substances, random);
            var substanceAuthorisations = MockSubstanceAuthorisationsGenerator.Create(foods, substances);
            var substanceAuthorisationsDict = new Dictionary<(Food, Compound), SubstanceAuthorisation>();
            foreach (var item in substanceAuthorisations) {
                substanceAuthorisationsDict[(item.Food, item.Substance)] = item;
            }
            var possibleExtrapolations = new Dictionary<Food, List<FoodSubstanceExtrapolationCandidate>> {
                [foods[2]] = new List<FoodSubstanceExtrapolationCandidate>() {
                new FoodSubstanceExtrapolationCandidate() {
                    ExtrapolationFood = foods[6],
                    MeasuredSubstance = substances[2],
                },
            }
            };
            var foodExtrapolations = new Dictionary<Food, ICollection<Food>> {
                [foods[8]] = new List<Food>() { foods[9] }
            };
            var residuDefinitions = new List<SubstanceConversion>() {
                new SubstanceConversion() {
                    ActiveSubstance = new Compound(){},
                    MeasuredSubstance = substances[0],
                    Proportion = .3,
                }
            };

            var settings = new MockFoodExtrapolationCandidatesCalculatorSettings() {
                ThresholdForExtrapolation = 3
            };
            var calculator = new FoodExtrapolationCandidatesCalculator(settings);
            var result = calculator.ComputeExtrapolationCandidates(
                foods,
                substances,
                sampleCompoundsCollections,
                foodExtrapolations,
                residuDefinitions,
                substanceAuthorisationsDict,
                maximumConcentrationLimits
            );
        }

        /// <summary>
        /// Calculates missing value extrapolation candidates, needs further implementation by Johannes
        /// </summary>
        [TestMethod]
        public void FoodExtrapolationCalculator_TestMissingValueExtrapolation() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = MockFoodsGenerator.Create(10);
            var substances = MockSubstancesGenerator.Create(4);
            var sampleCompoundsCollections = MockSampleCompoundCollectionsGenerator
                .Create(foods.Take(4).ToList(), substances, random);
            var possibleExtrapolations = new Dictionary<Food, List<FoodSubstanceExtrapolationCandidate>> {
                [foods[2]] = new List<FoodSubstanceExtrapolationCandidate>() {
                new FoodSubstanceExtrapolationCandidate() {
                    ExtrapolationFood = foods[6],
                    MeasuredSubstance = substances[2],
                },
            }
            };
            var extrapolationsCandidates = new List<FoodSubstanceExtrapolationCandidates>() {
                new FoodSubstanceExtrapolationCandidates() {
                    Food = foods[2],
                    Substance = substances.Last(),
                    Measurements= 4,
                    PossibleExtrapolations= possibleExtrapolations,
                }
            };

            MissingValueExtrapolationCalculator.ExtrapolateMissingValues(
                sampleCompoundsCollections,
                extrapolationsCandidates,
                random
             );
        }
    }
}
