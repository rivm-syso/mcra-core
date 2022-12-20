using MCRA.Utils.Statistics;
using MCRA.General;
using MCRA.Simulation.Calculators.SingleValueConcentrationsCalculation;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace MCRA.Simulation.Test.UnitTests.Calculators.SingleValueConcentrationsCalculation {

    [TestClass]
    public class SingleValueConcentrationConversionCalculatorTests {

        /// <summary>
        /// Tests compute highest residue single value concentrations using single value concentrations
        /// calculator.
        /// </summary>
        [TestMethod]
        public void SingleValueConcentrationsCalculator_TestCompute() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = MockFoodsGenerator.Create(3);
            var substances = MockSubstancesGenerator.Create(8);
            var measuredSubstances = substances.Take(4).ToList();
            var activeSubstance = substances.Skip(4).ToList();

            var singleValueConcentrationModels = MockSingleValueConcentrationModelsGenerator.Create(foods, measuredSubstances, random);
            var conversionFactors = MockDeterministicSubstanceConversionFactorsGenerator.Create(measuredSubstances, activeSubstance, random);

            var calculator = new SingleValueConcentrationConversionCalculator();
            var result = calculator.Compute(activeSubstance, singleValueConcentrationModels, conversionFactors);
            Assert.AreEqual(singleValueConcentrationModels.Count, result.Count);
        }
    }
}
