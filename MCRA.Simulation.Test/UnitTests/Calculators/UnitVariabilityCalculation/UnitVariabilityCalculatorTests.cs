using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;
using MCRA.Simulation.Calculators.UnitVariabilityCalculation;
using MCRA.Simulation.Test.Mock.MockCalculatorSettings;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.UnitVariabilityCalculation {
    /// <summary>
    /// UnitVariabilityCalculation calculator
    /// </summary>
    [TestClass]
    public class UnitVariabilityCalculatorTests {
        /// <summary>
        /// Calculate unit variability: UnitVariabilityModelType.BernoulliDistribution
        /// </summary>
        [TestMethod]
        public void UnitVariabilityCalculatorTest1() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = MockFoodsGenerator.Create(3);
            var factors = MockUnitVariabilityFactorsGenerator.Create(foods, random);
            var settings = new MockUnitVariabilityCalculatorSettings() {
                UseUnitVariability = true,
                UnitVariabilityModelType = UnitVariabilityModelType.BernoulliDistribution,
                UnitVariabilityType = UnitVariabilityType.VariabilityFactor,
                EstimatesNature = EstimatesNature.Realistic,
                DefaultFactorLow = 3,
                DefaultFactorMid = 3,
                MeanValueCorrectionType = MeanValueCorrectionType.Unbiased,
                UnitVariabilityCorrelationType = UnitVariabilityCorrelationType.NoCorrelation
            };

            var calculator = new UnitVariabilityCalculator(
                settings,
                factors
            );

            var compoundConcentrations = new List<DietaryIntakePerCompound>();
            var compoundConcentration1 = new DietaryIntakePerCompound() {
                Compound = null,
                ProcessingCorrectionFactor = 1,
                ProcessingFactor = .5f,
                ProcessingType = new ProcessingType() { Code = "baking" },
                IntakePortion = new IntakePortion() {
                    Amount = 10,
                    Concentration = 10,
                },
                UnitIntakePortions = new List<IntakePortion>() {
                    new IntakePortion() {
                        Amount = 10,
                        Concentration = 10,
                    }
                }
            };
            var compoundConcentration2 = new DietaryIntakePerCompound() {
                Compound = null,
                ProcessingCorrectionFactor = 1,
                ProcessingFactor = .5f,
                ProcessingType = null,
                IntakePortion = new IntakePortion() {
                    Amount = 10,
                    Concentration = 10,
                },
                UnitIntakePortions = new List<IntakePortion>() {
                    new IntakePortion() {
                        Amount = 10,
                        Concentration = 10,
                    }
                }
            };
            var compoundConcentration3 = new DietaryIntakePerCompound() {
                Compound = null,
                ProcessingCorrectionFactor = 1,
                ProcessingFactor = .5f,
                ProcessingType = new ProcessingType() { Code = "baking", IsBulkingBlending = true },
                IntakePortion = new IntakePortion() {
                    Amount = 10,
                    Concentration = 10,
                },
                UnitIntakePortions = new List<IntakePortion>() {
                    new IntakePortion() {
                        Amount = 10,
                        Concentration = 10,
                    }
                }
            };
            compoundConcentrations.Add(compoundConcentration1);
            compoundConcentrations.Add(compoundConcentration2);
            compoundConcentrations.Add(compoundConcentration3);
            var result = calculator.CalculateResidues(compoundConcentrations, foods.First(), random);
            Assert.AreEqual(3, result.Count);
        }
        /// <summary>
        /// Calculate unit variability: UnitVariabilityModelType.BetaDistribution
        /// </summary>
        [TestMethod]
        public void UnitVariabilityCalculatorTest2() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = MockFoodsGenerator.Create(3);
            var factors = MockUnitVariabilityFactorsGenerator.Create(foods, random);
            var settings = new MockUnitVariabilityCalculatorSettings() {
                UseUnitVariability = true,
                UnitVariabilityModelType = UnitVariabilityModelType.BetaDistribution,
                UnitVariabilityType = UnitVariabilityType.VariabilityFactor,
                EstimatesNature = EstimatesNature.Realistic,
                DefaultFactorLow = 3,
                DefaultFactorMid = 3,
                MeanValueCorrectionType = MeanValueCorrectionType.Unbiased,
                UnitVariabilityCorrelationType = UnitVariabilityCorrelationType.NoCorrelation
            };
            var calculator = new UnitVariabilityCalculator(
                    settings,
                    factors
                );

            var compoundConcentrations = new List<DietaryIntakePerCompound>();
            var compoundConcentration1 = new DietaryIntakePerCompound() {
                Compound = null,
                ProcessingCorrectionFactor = 1,
                ProcessingFactor = .5f,
                ProcessingType = new ProcessingType() { Code = "baking" },
                IntakePortion = new IntakePortion() {
                    Amount = 10,
                    Concentration = 10,
                },
                UnitIntakePortions = new List<IntakePortion>() {
                    new IntakePortion() {
                        Amount = 10,
                        Concentration = 10,
                    }
                }
            };

            compoundConcentrations.Add(compoundConcentration1);
            var result = calculator.CalculateResidues(compoundConcentrations, foods.First(), random);
            Assert.AreEqual(1, result.Count);
        }
        /// <summary>
        /// Calculate unit variability: UnitVariabilityModelType.LogNormalDistribution
        /// </summary>
        [TestMethod]
        public void UnitVariabilityCalculatorTest3() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = MockFoodsGenerator.Create(3);
            var factors = MockUnitVariabilityFactorsGenerator.Create(foods, random);
            var settings = new MockUnitVariabilityCalculatorSettings() {
                UseUnitVariability = true,
                UnitVariabilityModelType = UnitVariabilityModelType.LogNormalDistribution,
                UnitVariabilityType = UnitVariabilityType.VariabilityFactor,
                EstimatesNature = EstimatesNature.Realistic,
                DefaultFactorLow = 3,
                DefaultFactorMid = 3,
                MeanValueCorrectionType = MeanValueCorrectionType.Unbiased,
                UnitVariabilityCorrelationType = UnitVariabilityCorrelationType.NoCorrelation
            };
            var calculator = new UnitVariabilityCalculator(settings, factors);

            var compoundConcentrations = new List<DietaryIntakePerCompound>();
            var compoundConcentration1 = new DietaryIntakePerCompound() {
                Compound = null,
                ProcessingCorrectionFactor = 1,
                ProcessingFactor = .5f,
                ProcessingType = new ProcessingType() { Code = "baking" },
                IntakePortion = new IntakePortion() {
                    Amount = 10,
                    Concentration = 10,
                },
                UnitIntakePortions = new List<IntakePortion>() {
                    new IntakePortion() {
                        Amount = 10,
                        Concentration = 10,
                    }
                }
            };

            compoundConcentrations.Add(compoundConcentration1);
            var result = calculator.CalculateResidues(compoundConcentrations, foods.First(), random);
            Assert.AreEqual(1, result.Count);
        }
    }
}
