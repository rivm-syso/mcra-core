using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;
using MCRA.General;
using MCRA.Simulation.Calculators.SingleValueDietaryExposuresCalculation;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Calculators.SingleValueDietaryExposuresCalculation {

    /// <summary>
    /// Tests IESTI single value dietary exposures calculator.
    /// </summary>
    [TestClass]
    public class IestiSingleValueDietaryExposureCalculatorTests {

        [TestMethod]
        public void IestiSingleValueDietaryExposureCalculator_TestCompute() {
            var population = MockPopulationsGenerator.Create(1);
            var substances = MockSubstancesGenerator.Create(1);
            var food = new Food() {
                Name = "A",
                Properties = new FoodProperty() {
                    UnitWeight = 80
                }
            };
            var consumption = new SingleValueConsumptionModel() {
                BodyWeight = 77,
                Food = food,
                LargePortion = 80,
            };
            var concentration = new SingleValueConcentrationModel() {
                Substance = substances.First(),
                Food = food,
                HighestConcentration = 0.065
            };
            var concentrations = new Dictionary<(Food, Compound), SingleValueConcentrationModel> {
                [(food, substances.First())] = concentration
            };
            var calculator = new IestiSingleValueDietaryExposureCalculator(
                null,
                null,
                null,
                false
            );
            var result = calculator
                .Compute(
                    population.First(),
                    substances,
                    new List<SingleValueConsumptionModel>() { consumption },
                    concentrations,
                    null,
                    ConsumptionIntakeUnit.gPerDay,
                    ConcentrationUnit.mgPerKg,
                    BodyWeightUnit.kg,
                    TargetUnit.CreateDietaryExposureUnit(
                        ConsumptionUnit.g,
                        ConcentrationUnit.mgPerKg,
                        BodyWeightUnit.kg,
                        false
                    )
                )
                .First() as AcuteSingleValueDietaryExposureResult;
            Assert.AreEqual(IESTIType.Case2b, result.IESTICase);
        }
    }
}
