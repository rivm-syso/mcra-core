using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ConcentrationModelCalculation.ConcentrationModels;
using MCRA.Simulation.Calculators.ConsumerProductExposureCalculation;
using MCRA.Simulation.Objects;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using MCRA.Utils.Statistics;

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

            var useFrequenciesLookup = ConsumerProductExposureCalculator.MatchUseFrequencies(
                [useFrequency],
                [simIndividual],
                ConsumerProductMultipleSurveyMatchingMethod.AtRandom,
                new McraRandomGenerator(1111)
            );

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
                useFrequenciesLookup,
                [ExposureRoute.Oral],
                [substance],
                targetUnit
            );

            var exposure = results[0];
            var perSubstance = exposure.IntakesPerProduct[0]
                .IntakesPerSubstance[ExposureRoute.Oral][0];

            Assert.AreEqual(expectedAmount, perSubstance.Amount, 1e-10);
        }

        [TestMethod]
        [DataRow(ConsumerProductMultipleSurveyMatchingMethod.AtRandom)]
        [DataRow(ConsumerProductMultipleSurveyMatchingMethod.MatchOnGender)]
        [DataRow(ConsumerProductMultipleSurveyMatchingMethod.MatchOnAge)]
        [DataRow(ConsumerProductMultipleSurveyMatchingMethod.MatchOnAgeAndGender)]

        public void ConsumerProductExposureCalculator_MultipleSurveys(ConsumerProductMultipleSurveyMatchingMethod consumerProductMultipleSurveyMatchingMethod) {
            var refCount = 20;
            
            int seed = 1;
            var random = new McraRandomGenerator(seed);
            var cosmetic = FakeConsumerProductsGenerator.Create(2)[0];
            var smoking = FakeConsumerProductsGenerator.Create(2)[1];

            var substance = FakeSubstancesGenerator.Create(1)[0];
            var individuals = FakeIndividualsGenerator.Create(25, 2, random);
            FakeIndividualsGenerator.AddFakeSexProperty(individuals, random);
            var referenceIndividuals = individuals.Take(refCount).ToList();
            var smokingIndividuals = individuals.Skip(refCount).Take(5).ToList();
            var useFrequencyValues = new List<IndividualConsumerProductUseFrequency>();
            for (int i = 0; i < referenceIndividuals.Count; i++) {
                var individual = referenceIndividuals[i];
                individual.CodeFoodSurvey = "CosmeticSurvey";
                var useFrequency = new IndividualConsumerProductUseFrequency {
                    Individual = individual,
                    Product = cosmetic,
                    Frequency = 0.5,
                };
                useFrequencyValues.Add(useFrequency);
            }

            for (int i = 0; i < smokingIndividuals.Count; i++) {
                var individual = smokingIndividuals[i];
                individual.CodeFoodSurvey = "SmokingSurvey";
                var useFrequency = new IndividualConsumerProductUseFrequency {
                    Individual = individual,
                    Product = smoking,
                    Frequency = 0.5,
                };
                useFrequencyValues.Add(useFrequency);
            }

            var simulatedIndividuals = referenceIndividuals
                .Select((ind, ix) => new SimulatedIndividual(ind, ix))
                .ToList();

            var useFrequenciesLookup = ConsumerProductExposureCalculator.MatchUseFrequencies(
                useFrequencyValues,
                simulatedIndividuals,
                consumerProductMultipleSurveyMatchingMethod,
                new McraRandomGenerator(1111)
            );
            //There are 20 reference individuals, each with a use frequency for the cosmetic product and the smoking product,
            //so we expect 40 matched use frequencies in total
            var expectedCount = refCount * 2;
            Assert.AreEqual(expectedCount, useFrequenciesLookup.Values.SelectMany(c => c).Count());
        }
    }
}
