using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualDayConcentrationCalculation;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmKineticConversionFactor;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.KineticConversions;
using MCRA.Simulation.Calculators.HumanMonitoringSampleCompoundCollections;
using MCRA.Simulation.Test.Helpers;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using MCRA.Utils.Statistics;
using MCRA.Utils.Test;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Calculators.HumanMonitoringCalculation.HbmTargetMatrixExtrapolation {

    /// <summary>
    /// KineticConversionFactor calculator
    /// </summary>
    [TestClass]
    public class HbmMultipleTargetExtrapolationCalculatorTests {


        /// <summary>
        /// Apply kinetic conversion for specific biomarkers:
        /// 
        ///  BEFORE
        ///  --------------------------------
        ///  Blood      0   1   2   -   -   -
        ///  Urine      -   -   2   3   4   -
        ///  Hair       -   -   -   -   -   5
        ///  
        ///  AFTER
        ///  --------------------------------
        ///  Blood      0   1   2   3   -   5
        ///  Urine      0   -   2   3   4   5
        ///  Hair       -   -   -   -   -   5
        ///  
        ///  Conversion factors:
        ///  CMP0   blood --> urine         - new in urine
        ///  CMP2   blood --> urine         - already present in urine, no conversion is applied and original urine concentration is preserved
        ///  CMP3   urine --> blood         - new in blood
        ///  CMP5   hair --> blood          - new in blood
        ///  CMP5   hair --> urine          - new in urine
        /// 
        /// </summary>
        [TestMethod]
        public void HumanMonitoringAnalysisActionCalculator_ApplyKineticConversionNotSingleTarget_ShouldApplyConversionForSpecificBiomarkers() {
            // Arrange
            var individuals = MockIndividualsGenerator.Create(1, 1, new McraRandomGenerator(1), useSamplingWeights: false);
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var substances = MockSubstancesGenerator.Create(new[] { "cmp0", "cmp1", "cmp2", "cmp3", "cmp4", "cmp5" });
            var substancesBlood = substances.Take(3).ToList();
            var substancesUrine = (substances.TakeLast(4)).Take(3).ToList();
            var substancesHair = substances.TakeLast(1).ToList();

            var scenarios = new List<(ExposureTarget Target, List<Compound> Substances)> {
                (new ExposureTarget(BiologicalMatrix.Blood), substancesBlood),
                (new ExposureTarget(BiologicalMatrix.Urine), substancesUrine),
                (new ExposureTarget(BiologicalMatrix.Hair), substancesHair)
            };

            var random = new McraRandomGenerator(seed: 1);
            var hbmIndividualDayCollections = scenarios
                .SelectMany(r => {
                    var matrix = r.Target.BiologicalMatrix;
                    var targetUnit = new TargetUnit(
                        r.Target,
                        matrix.GetTargetConcentrationUnit().GetSubstanceAmountUnit(),
                        matrix.GetTargetConcentrationUnit().GetConcentrationMassUnit(),
                        TimeScaleUnit.Unspecified
                    );
                    var result = FakeHbmIndividualDayConcentrationsGenerator
                        .Create(
                            individualDays,
                            r.Substances,
                            null,
                            targetUnit,
                            random
                        );
                    return result;
                })
                .ToList();

            var kineticConversionFactorModelCmp0 = FakeHbmDataGenerator.FakeKineticConversionFactorModel(BiologicalMatrix.Blood, BiologicalMatrix.Urine, substances[0]);
            var kineticConversionFactorModelCmp1 = FakeHbmDataGenerator.FakeKineticConversionFactorModel(BiologicalMatrix.Blood, BiologicalMatrix.Urine, substances[2]);
            var kineticConversionFactorModelCmp3 = FakeHbmDataGenerator.FakeKineticConversionFactorModel(BiologicalMatrix.Urine, BiologicalMatrix.Blood, substances[3]);
            var kineticConversionFactorModelCmp5b = FakeHbmDataGenerator.FakeKineticConversionFactorModel(BiologicalMatrix.Hair, BiologicalMatrix.Blood, doseUnitTo: DoseUnit.ugPerg, substance: substances[5]);
            var kineticConversionFactorModelCmp5u = FakeHbmDataGenerator.FakeKineticConversionFactorModel(BiologicalMatrix.Hair, BiologicalMatrix.Urine, doseUnitTo: DoseUnit.ugPerg, substance: substances[5]);
            var kineticConversionFactorModels = new List<KineticConversionFactorModel> {
                kineticConversionFactorModelCmp0,
                kineticConversionFactorModelCmp1,
                kineticConversionFactorModelCmp3,
                kineticConversionFactorModelCmp5b,
                kineticConversionFactorModelCmp5u
            };

            // Act
            var result = HbmMultipleTargetExtrapolationCalculator
                .Calculate(
                    hbmIndividualDayCollections,
                    kineticConversionFactorModels,
                    individualDays,
                    substances,
                    seed: 1
                );

            // Assert
            var samplesBlood = result.FirstOrDefault(h => h.Target.BiologicalMatrix == BiologicalMatrix.Blood).HbmIndividualDayConcentrations;
            var samplesUrine = result.FirstOrDefault(h => h.Target.BiologicalMatrix == BiologicalMatrix.Urine).HbmIndividualDayConcentrations;
            Assert.IsTrue(samplesBlood.All(s => s.Substances.Any(s => s == substances[3])));
            Assert.IsTrue(samplesBlood.All(s => s.Substances.Any(s => s == substances[5])));
            Assert.IsTrue(samplesUrine.All(s => s.Substances.Any(s => s == substances[0])));
            Assert.IsTrue(samplesUrine.All(s => s.Substances.Any(s => s == substances[2])));
            Assert.IsTrue(samplesUrine.All(s => s.Substances.Any(s => s == substances[5])));

            void AssertConversion(
               HbmIndividualDayCollection collectionFrom,
               ICollection<HbmIndividualDayConcentration> collectionTo,
               ExposureTarget targetExposureFrom,
               ExposureTarget targetExposureTo,
               Compound substance) {
                var recordFrom = collectionFrom.HbmIndividualDayConcentrations.FirstOrDefault(r => r.Individual.Code == "0" && r.Day == "0");
                var recordTo = collectionTo.FirstOrDefault(r => r.Individual.Code == "0" && r.Day == "0");
                var valueFrom = recordFrom.ConcentrationsBySubstance[substance].Concentration;
                var valueTo = recordTo.GetExposureForSubstance(substance);
                var conversionFactor = kineticConversionFactorModels
                    .FirstOrDefault(
                           k => k.ConversionRule.SubstanceFrom == substance
                        && k.ConversionRule.SubstanceTo == substance
                        && k.ConversionRule.TargetFrom == targetExposureFrom
                        && k.ConversionRule.TargetTo == targetExposureTo
                    );

                var conversionFactorExpected = conversionFactor.ConversionRule.ConversionFactor;
                var conversionFactorMeasured = valueTo / valueFrom;

                Assert.AreEqual(conversionFactorExpected, conversionFactorMeasured, 0.001);
            };

            var bloodCollection = hbmIndividualDayCollections.First(r => r.Target.BiologicalMatrix == BiologicalMatrix.Blood);
            AssertConversion(bloodCollection, samplesUrine, new ExposureTarget(BiologicalMatrix.Blood), new ExposureTarget(BiologicalMatrix.Urine), substances[0]);

            var urineCollection = hbmIndividualDayCollections.First(r => r.Target.BiologicalMatrix == BiologicalMatrix.Urine);
            AssertConversion(urineCollection, samplesBlood, new ExposureTarget(BiologicalMatrix.Urine), new ExposureTarget(BiologicalMatrix.Blood), substances[3]);

            var hairCollection = hbmIndividualDayCollections.First(r => r.Target.BiologicalMatrix == BiologicalMatrix.Hair);
            AssertConversion(hairCollection, samplesBlood, new ExposureTarget(BiologicalMatrix.Hair), new ExposureTarget(BiologicalMatrix.Blood), substances[5]);
            AssertConversion(hairCollection, samplesUrine, new ExposureTarget(BiologicalMatrix.Hair), new ExposureTarget(BiologicalMatrix.Urine), substances[5]);
        }
    }
}
