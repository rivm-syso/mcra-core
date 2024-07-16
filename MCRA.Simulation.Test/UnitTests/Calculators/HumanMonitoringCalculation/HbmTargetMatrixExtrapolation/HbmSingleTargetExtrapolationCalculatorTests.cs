using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualDayConcentrationCalculation;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmKineticConversionFactor;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.KineticConversions;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using MCRA.Utils.Statistics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Calculators.HumanMonitoringCalculation.HbmTargetMatrixExtrapolation {

    /// <summary>
    /// Single target extrapolation calculator tests.
    /// </summary>
    [TestClass]
    public class HbmSingleTargetExtrapolationCalculatorTests {
        /// <summary>
        /// HBM kinetic conversion to a single target matrix that is not present in the source data sampling methods.
        /// 
        /// Example:
        /// HBM CodeBook contains hair and urine samples but not blood. Kinetic conversion factors define factors for 
        /// conversion to blood: from hair to blood and from urine to blood. This should result in an added collection 
        /// for blood.
        /// 
        ///  BEFORE
        ///  -------------------------
        ///  Hair       0   1   -   -
        ///  Urine      -   -   2   3 
        ///  
        ///  AFTER
        ///  -------------------------
        ///  Blood      0   1   2   -  
        ///  
        ///  Conversion factors, no conversion defined for the last substance 3
        ///  CMP0   hair  --> blood
        ///  CMP1   hair  --> blood
        ///  CMP2   urine --> blood
        /// 
        /// </summary>
        [TestMethod]
        public void HbmSingleTargetExtrapolationCalculator_KineticConversionToMissingTargetMatrix_ShouldAddMatrixFromConversionFactors() {
            // Arrange
            var individuals = MockIndividualsGenerator.Create(1, 1, new McraRandomGenerator(1), useSamplingWeights: false);
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var substances = MockSubstancesGenerator.Create(["cmp0", "cmp1", "cmp2", "cmp3"]);
            var substancesHair = substances.Take(2).ToList();
            var substancesUrine = substances.TakeLast(2).ToList();
            var targetHair = new ExposureTarget(BiologicalMatrix.Hair);
            var targetUrine = new ExposureTarget(BiologicalMatrix.Urine);

            var scenarios = new List<(ExposureTarget Target, List<Compound> Substances)> {
                (targetHair, substancesHair),
                (targetUrine, substancesUrine)
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
                    var result = new List<HbmIndividualDayCollection> {
                        FakeHbmIndividualDayConcentrationsGenerator
                        .Create(
                            individualDays,
                            r.Substances,
                            null,
                            targetUnit,
                            random
                        ) };
                    return result;
                })
                .ToList();

            var kineticConversionFactorModelCmp0 = FakeHbmDataGenerator
                .FakeKineticConversionFactorModel(
                BiologicalMatrix.Hair,
                BiologicalMatrix.Blood,
                substances[0], 
                DoseUnit.ugPerg, 
                DoseUnit.ugPerL
                );
            var kineticConversionFactorModelCmp1 = FakeHbmDataGenerator
                .FakeKineticConversionFactorModel(
                BiologicalMatrix.Hair,
                BiologicalMatrix.Blood,
                substances[1],
                DoseUnit.ugPerg,
                DoseUnit.ugPerL
                );
            var kineticConversionFactorModelCmp2 = FakeHbmDataGenerator
                .FakeKineticConversionFactorModel(
                BiologicalMatrix.Urine,
                BiologicalMatrix.Blood,
                substances[2],
                DoseUnit.ugPerL,
                DoseUnit.ugPerL
                );
            var kineticConversionFactorModels = new List<KineticConversionFactorModel> {
                kineticConversionFactorModelCmp0,
                kineticConversionFactorModelCmp1,
                kineticConversionFactorModelCmp2
            };

            // Act
            var result = HbmSingleTargetExtrapolationCalculator
                .Calculate(
                    hbmIndividualDayCollections,
                    kineticConversionFactorModels,
                    individualDays,
                    substances,
                    TargetLevelType.Internal,
                    BiologicalMatrix.Blood
                );

            // Assert
            var targetBlood = new ExposureTarget(BiologicalMatrix.Blood);
            Assert.IsFalse(result.Any(c => c.Target == targetHair));
            Assert.IsFalse(result.Any(c => c.Target == targetUrine));
            Assert.IsTrue(result.Any(c => c.Target == targetBlood));
            var samplesBlood = result
                .FirstOrDefault(h => h.Target == targetBlood)?.HbmIndividualDayConcentrations;
            Assert.IsTrue(samplesBlood.All(s => s.Substances.Any(s => s == substances[0])));
            Assert.IsTrue(samplesBlood.All(s => s.Substances.Any(s => s == substances[1])));
            Assert.IsTrue(samplesBlood.All(s => s.Substances.Any(s => s == substances[2])));
            Assert.IsFalse(samplesBlood.All(s => s.Substances.Any(s => s == substances[3])));

            void AssertConversion(
               HbmIndividualDayCollection collectionFrom,
               ICollection<HbmIndividualDayConcentration> collectionTo,
               ExposureTarget targetExposureFrom,
               ExposureTarget targetExposureTo,
               Compound substance) {
                var recordFrom = collectionFrom.HbmIndividualDayConcentrations
                    .FirstOrDefault(r => r.Individual.Code == "0" && r.Day == "0");
                var recordTo = collectionTo.FirstOrDefault(r => r.Individual.Code == "0" && r.Day == "0");
                var valueFrom = recordFrom.ConcentrationsBySubstance[substance].Exposure;
                var valueTo = recordTo.GetSubstanceExposure(substance);
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

            var hairCollection = hbmIndividualDayCollections.First(r => r.Target == targetHair);
            var urineCollection = hbmIndividualDayCollections.First(r => r.Target == targetUrine);
            AssertConversion(hairCollection, samplesBlood, targetHair, targetBlood, substances[0]);
            AssertConversion(hairCollection, samplesBlood, targetHair, targetBlood, substances[1]);
            AssertConversion(urineCollection, samplesBlood, targetUrine, targetBlood, substances[2]);
        }
    }
}
