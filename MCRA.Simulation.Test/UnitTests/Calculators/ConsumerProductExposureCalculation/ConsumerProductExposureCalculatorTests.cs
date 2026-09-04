using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ConcentrationModelCalculation.ConcentrationModels;
using MCRA.Simulation.Calculators.ConsumerProductExposureCalculation;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;

namespace MCRA.Simulation.Test.UnitTests.Calculators.ConsumerProductExposureCalculation {

    /// <summary>
    /// Testing the consumer product exposure calculation
    /// </summary>
    [TestClass]
    public class ConsumerProductExposureCalculatorTests {

        /// <summary>
        /// Tests Compute() with different combinations of application amount units, concentration units, and target 
        /// substance amount units. Verifies that the unit conversions yield correct absolute amounts.
        /// Amount = useFrequency * exposureFraction * applicationAmount * concentration
        /// </summary>
        [TestMethod]
        [DataRow(0.5, ApplicationAmountUnit.g, 10.0, ConsumerProductConcentrationUnit.mgPerKg, SubstanceAmountUnit.Micrograms, 2.0, 0.8, 8.0)]
        [DataRow(0.5, ApplicationAmountUnit.mg, 10.0, ConsumerProductConcentrationUnit.ugPerKg, SubstanceAmountUnit.Nanograms, 2.0, 0.8, 0.008)]
        [DataRow(0.5, ApplicationAmountUnit.kg, 10.0, ConsumerProductConcentrationUnit.gPerKg, SubstanceAmountUnit.Milligrams, 2.0, 0.8, 8000)]
        [DataRow(0.5, ApplicationAmountUnit.g, 10.0, ConsumerProductConcentrationUnit.mgPerUnit, SubstanceAmountUnit.Micrograms, 2.0, 0.8, 16000)]
        [DataRow(0.5, ApplicationAmountUnit.mg, 10.0, ConsumerProductConcentrationUnit.ugPerUnit, SubstanceAmountUnit.Nanograms, 2.0, 0.8, 16000)]
        [DataRow(0.5, ApplicationAmountUnit.g, 10.0, ConsumerProductConcentrationUnit.gPerUnit, SubstanceAmountUnit.Micrograms, 2.0, 0.8, 16000000)]
        [DataRow(0.5, ApplicationAmountUnit.mL, 10.0, ConsumerProductConcentrationUnit.ugPerL, SubstanceAmountUnit.Micrograms, 2.0, 0.8, 0.008)]
        [DataRow(0.5, ApplicationAmountUnit.cL, 10.0, ConsumerProductConcentrationUnit.ugPerL, SubstanceAmountUnit.Micrograms, 2.0, 0.8, 0.08)]
        [DataRow(0.5, ApplicationAmountUnit.cL, 10.0, ConsumerProductConcentrationUnit.mgPerdL, SubstanceAmountUnit.Milligrams, 2.0, 0.8, 0.8)]
        [DataRow(0.5, ApplicationAmountUnit.dL, 10.0, ConsumerProductConcentrationUnit.ugPermL, SubstanceAmountUnit.Milligrams, 2.0, 0.8, 0.8)]
        [DataRow(0.5, ApplicationAmountUnit.mL, 10.0, ConsumerProductConcentrationUnit.gPerL, SubstanceAmountUnit.Micrograms, 2.0, 0.8, 8000)]
        public void ConsumerProductExposureCalculator_DifferentUnits_ShouldAlignCorrectlyToTargetUnit(
            double applicationAmount,
            ApplicationAmountUnit amountUnit,
            double concentrationMean,
            ConsumerProductConcentrationUnit concUnit,
            SubstanceAmountUnit targetAmountUnit,
            double useFrequencyValue,
            double exposureFractionValue,
            double expectedAmount
        ) {
            var product = FakeConsumerProductsGenerator.Create(1)[0];
            var substance = FakeSubstancesGenerator.Create(1, molecularWeights: [1])[0];
            var simIndividual = FakeIndividualsGenerator.CreateSingle();

            var useFrequency = new IndividualConsumerProductUseFrequency {
                Individual = simIndividual.Individual,
                Product = product,
                Frequency = useFrequencyValue,
            };

            var exposureFraction = new ConsumerProductExposureFraction {
                Product = product,
                Substance = substance,
                Route = ExposureRoute.Oral,
                ExposureFraction = exposureFractionValue,
                Sex = GenderType.Undefined
            };

            var appAmount = new ConsumerProductApplicationAmount {
                Product = product,
                Amount = applicationAmount,
                Unit = amountUnit,
                DistributionType = ApplicationAmountDistributionType.Constant,
            };

            var concModel = new CMConstant {
                Compound = substance,
                Unit = new ConsumerProductConcentrationUnitSpec(concUnit),
                ConcentrationDistribution = new ConcentrationDistribution { Mean = concentrationMean },
                CorrectedOccurenceFraction = 1,
            };
            concModel.CalculateParameters();

            var concModels = new Dictionary<(ConsumerProduct, Compound), ConcentrationModel> {
                [(product, substance)] = concModel,
            };

            var calculator = new ConsumerProductExposureCalculator(
                [exposureFraction],
                [appAmount],
                concModels
            );

            var targetUnit = new ExposureUnitTriple(
                targetAmountUnit,
                ConcentrationMassUnit.PerUnit
            );

            var results = calculator.Compute(
                [simIndividual],
                [useFrequency],
                [ExposureRoute.Oral],
                [substance],
                targetUnit
            );

            var exposure = results[0];
            var perSubstance = exposure.IntakesPerProduct[0]
                .IntakesPerSubstance[ExposureRoute.Oral][0];

            Assert.AreEqual(expectedAmount, perSubstance.Amount, 1e-10);
        }
    }
}
