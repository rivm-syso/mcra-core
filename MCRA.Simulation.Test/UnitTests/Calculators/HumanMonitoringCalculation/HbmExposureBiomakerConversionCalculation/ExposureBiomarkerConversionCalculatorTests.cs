using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmExposureBiomarkerConversion;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualDayConcentrationCalculation;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using MCRA.Utils.Statistics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Calculators.HbmExposureBiomarkerConversionCalculator {

    /// <summary>
    /// ProcessingFactorCalculation calculator
    /// </summary>
    [TestClass]
    public class ExposureBiomarkerConversionCalculatorTests {

        [TestMethod]
        public void ExposureBiomarkerConversionCalculator_TestConvertSingle() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var substanceFrom = new Compound("FROM");
            var hbmIndividualDayConcentrationCollections = fakeHbmIndividualDayConcentrationsCollections(random, substanceFrom);

            var conversionFactor = 0.5;
            var substanceTo = new Compound("TO");
            var conversion = new ExposureBiomarkerConversion() {
                ConversionFactor = conversionFactor,
                BiologicalMatrix = BiologicalMatrix.Blood,
                SubstanceFrom = substanceFrom,
                UnitFrom = ExposureUnitTriple.FromDoseUnit(DoseUnit.ugPerL),
                SubstanceTo = substanceTo,
                UnitTo = ExposureUnitTriple.FromDoseUnit(DoseUnit.ugPerL)
            };
            var conversions = new List<ExposureBiomarkerConversion>() { conversion };
            var exposureBiomarkerConversions = conversions?
                .Select(c => ExposureBiomarkerConversionCalculatorFactory.Create(
                    c,
                    false
                )
            ).ToList();
            var calculator = new ExposureBiomarkerConversionCalculator(exposureBiomarkerConversions);
            var result = calculator.Convert(hbmIndividualDayConcentrationCollections, seed);
            foreach (var record in result.First().HbmIndividualDayConcentrations) {
                Assert.AreEqual(
                    record.ConcentrationsBySubstance[substanceTo].Exposure,
                    conversionFactor * record.ConcentrationsBySubstance[substanceFrom].Exposure,
                    double.Epsilon
                );
            }
        }

        [TestMethod]
        public void ExposureBiomarkerConversionCalculator_TestConvertMultiple() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var substancesFrom = new[] {
                new Compound("FROM1"),
                new Compound("FROM2")
            };
            var hbmIndividualDayConcentrationCollections = fakeHbmIndividualDayConcentrationsCollections(random, substancesFrom);

            var substanceTo = new Compound("TO");
            var conversions = new List<ExposureBiomarkerConversion>() {
                new ExposureBiomarkerConversion() {
                    ConversionFactor = 1,
                    BiologicalMatrix = BiologicalMatrix.Blood,
                    SubstanceFrom = substancesFrom[0],
                    UnitFrom = ExposureUnitTriple.FromDoseUnit(DoseUnit.ugPerL),
                    SubstanceTo = substanceTo,
                    UnitTo = ExposureUnitTriple.FromDoseUnit(DoseUnit.ugPerL)
                },
                new ExposureBiomarkerConversion() {
                    ConversionFactor = 1,
                    BiologicalMatrix = BiologicalMatrix.Blood,
                    SubstanceFrom = substancesFrom[1],
                    UnitFrom = ExposureUnitTriple.FromDoseUnit(DoseUnit.ugPerL),
                    SubstanceTo = substanceTo,
                    UnitTo = ExposureUnitTriple.FromDoseUnit(DoseUnit.ugPerL)
                },
            };

            var exposureBiomarkerConversions = conversions?
                .Select(c => ExposureBiomarkerConversionCalculatorFactory.Create(
                    c,
                    false
                )
            ).ToList();
            var calculator = new ExposureBiomarkerConversionCalculator(exposureBiomarkerConversions);
            var result = calculator.Convert(hbmIndividualDayConcentrationCollections, seed);

            foreach (var record in result.First().HbmIndividualDayConcentrations) {
                var expected = record.ConcentrationsBySubstance[substancesFrom[0]].Exposure
                        + record.ConcentrationsBySubstance[substancesFrom[1]].Exposure;
                var observed = record.ConcentrationsBySubstance[substanceTo].Exposure;
                Assert.AreEqual(expected, observed, double.Epsilon);
            }
        }

        private static List<HbmIndividualDayCollection> fakeHbmIndividualDayConcentrationsCollections(
            McraRandomGenerator random,
            params Compound[] substances
        ) {
            var individuals = MockIndividualsGenerator.Create(25, 2, random, useSamplingWeights: true);
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var targetUnit = new TargetUnit(
                new ExposureTarget(BiologicalMatrix.Blood),
                new ExposureUnitTriple(
                    SubstanceAmountUnit.Micrograms,
                    ConcentrationMassUnit.Kilograms,
                    TimeScaleUnit.Peak
                )
            );
            var hbmIndividualDayConcentrations = new List<HbmIndividualDayCollection> {
                FakeHbmIndividualDayConcentrationsGenerator
                .Create(individualDays, substances, null, targetUnit, random)
            };
            return hbmIndividualDayConcentrations;
        }
    }
}

