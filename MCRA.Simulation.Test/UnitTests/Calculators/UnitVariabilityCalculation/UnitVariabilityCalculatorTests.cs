using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.DietaryExposureCalculation.IndividualDietaryExposureCalculation;
using MCRA.Simulation.Calculators.UnitVariabilityCalculation;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;

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
            var foods = FakeFoodsGenerator.Create(3);
            var factors = FakeUnitVariabilityFactorsGenerator.Create(foods, random);
            var calculator = new UnitVariabilityCalculator(
                unitVariabilityModelType: UnitVariabilityModelType.BernoulliDistribution,
                unitVariabilityType: UnitVariabilityType.VariabilityFactor,
                estimatesNature: EstimatesNature.Realistic,
                defaultFactorLow: 3,
                defaultFactorMid: 3,
                meanValueCorrectionType: MeanValueCorrectionType.Unbiased,
                unitVariabilityCorrelationType: UnitVariabilityCorrelationType.NoCorrelation,
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
                UnitIntakePortions = [
                    new IntakePortion() {
                        Amount = 10,
                        Concentration = 10,
                    }
                ]
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
                UnitIntakePortions = [
                    new IntakePortion() {
                        Amount = 10,
                        Concentration = 10,
                    }
                ]
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
                UnitIntakePortions = [
                    new IntakePortion() {
                        Amount = 10,
                        Concentration = 10,
                    }
                ]
            };
            compoundConcentrations.Add(compoundConcentration1);
            compoundConcentrations.Add(compoundConcentration2);
            compoundConcentrations.Add(compoundConcentration3);
            var result = calculator.CalculateResidues(compoundConcentrations, foods.First(), random);
            Assert.HasCount(3, result);
        }
        /// <summary>
        /// Calculate unit variability: UnitVariabilityModelType.BetaDistribution
        /// </summary>
        [TestMethod]
        public void UnitVariabilityCalculatorTest2() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = FakeFoodsGenerator.Create(3);
            var factors = FakeUnitVariabilityFactorsGenerator.Create(foods, random);

            var calculator = new UnitVariabilityCalculator(
                unitVariabilityModelType: UnitVariabilityModelType.BetaDistribution,
                unitVariabilityType: UnitVariabilityType.VariabilityFactor,
                estimatesNature: EstimatesNature.Realistic,
                defaultFactorLow: 3,
                defaultFactorMid: 3,
                meanValueCorrectionType: MeanValueCorrectionType.Unbiased,
                unitVariabilityCorrelationType: UnitVariabilityCorrelationType.NoCorrelation,
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
                UnitIntakePortions = [
                    new IntakePortion() {
                        Amount = 10,
                        Concentration = 10,
                    }
                ]
            };

            compoundConcentrations.Add(compoundConcentration1);
            var result = calculator.CalculateResidues(compoundConcentrations, foods.First(), random);
            Assert.HasCount(1, result);
        }
        /// <summary>
        /// Calculate unit variability: UnitVariabilityModelType.LogNormalDistribution
        /// </summary>
        [TestMethod]
        public void UnitVariabilityCalculatorTest3() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = FakeFoodsGenerator.Create(3);
            var factors = FakeUnitVariabilityFactorsGenerator.Create(foods, random);
            var calculator = new UnitVariabilityCalculator(
                unitVariabilityModelType: UnitVariabilityModelType.LogNormalDistribution,
                unitVariabilityType: UnitVariabilityType.VariabilityFactor,
                estimatesNature: EstimatesNature.Realistic,
                defaultFactorLow: 3,
                defaultFactorMid: 3,
                meanValueCorrectionType: MeanValueCorrectionType.Unbiased,
                unitVariabilityCorrelationType: UnitVariabilityCorrelationType.NoCorrelation,
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
                UnitIntakePortions = [
                    new IntakePortion() {
                        Amount = 10,
                        Concentration = 10,
                    }
                ]
            };

            compoundConcentrations.Add(compoundConcentration1);
            var result = calculator.CalculateResidues(compoundConcentrations, foods.First(), random);
            Assert.HasCount(1, result);
        }
    }
}
