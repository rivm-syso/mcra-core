using MCRA.General;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.CorrectionCalculators.BloodCorrectionCalculation;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using MCRA.Utils.Statistics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Calculators.HumanMonitoringCalculation.BloodCorrectionCalculation {
    [TestClass]
    public class BloodCorrectionCalculatorsTests {
        [TestMethod]
        [DataRow(ConcentrationUnit.ugPermL, 100000.0)]
        [DataRow(ConcentrationUnit.ngPermL, 100000.0)]
        [DataRow(ConcentrationUnit.ugPerL, 100.0)]
        [DataRow(ConcentrationUnit.ngPerL, 100.0)]
        public void GravimetricCorrection_DifferentTargetUnits_ShouldUseCorrectUnitAlignmentFactor(
            ConcentrationUnit targetUnit,
            double expectedUnitAlignmentFactor
        ) {
            // Arrange
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var individuals = FakeIndividualsGenerator.Create(1, 1, random, useSamplingWeights: true);
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var substances = FakeSubstancesGenerator.Create(1, null, null, lipidSoluble: true);
            var samplingMethod = FakeHbmDataGenerator.FakeHumanMonitoringSamplingMethod(biologicalMatrix: BiologicalMatrix.Blood);
            var hbmSampleSubstanceCollections = FakeHbmDataGenerator.FakeHbmSampleSubstanceCollections(
                individualDays, substances, samplingMethod, targetUnit
            );

            // Act
            var calculator = new LipidGravimetricCorrectionCalculator([]);
            var result = calculator.ComputeResidueCorrection(hbmSampleSubstanceCollections);

            // Assert: we have only one sample in the collection
            var sampleIn = hbmSampleSubstanceCollections[0].HumanMonitoringSampleSubstanceRecords
                .Select(r => r.HumanMonitoringSampleSubstances)
                .SelectMany(r => r.Values)
                .FirstOrDefault(v => v.MeasuredSubstance.IsLipidSoluble);
            var sampleOut = result[0].HumanMonitoringSampleSubstanceRecords
                .Select(r => r.HumanMonitoringSampleSubstances)
                .SelectMany(r => r.Values)
                .FirstOrDefault(v => v.MeasuredSubstance.IsLipidSoluble);
            var lipidGravity = hbmSampleSubstanceCollections[0]
                .HumanMonitoringSampleSubstanceRecords[0]
                .HumanMonitoringSample
                .LipidGrav;

            // Expected corrected value
            var expected = expectedUnitAlignmentFactor * sampleIn.Residue / lipidGravity.Value;
            Assert.AreEqual(expected, sampleOut.Residue, 0.1);
        }

        [TestMethod]
        [DataRow(ConcentrationUnit.ngPermL, 100000.0)]
        [DataRow(ConcentrationUnit.ugPermL, 100000.0)]
        [DataRow(ConcentrationUnit.ugPerL, 100.0)]
        [DataRow(ConcentrationUnit.ngPerL, 100.0)]
        public void EnzymaticSummation_DifferentTargetUnits_ShouldUseCorrectUnitAlignmentFactor(
            ConcentrationUnit targetUnit,
            double expectedUnitAlignmentFactor
        ) {
            // Arrange
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var individuals = FakeIndividualsGenerator.Create(1, 1, random, useSamplingWeights: true);
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var substances = FakeSubstancesGenerator.Create(1, null, null, lipidSoluble: true);
            var samplingMethod = FakeHbmDataGenerator.FakeHumanMonitoringSamplingMethod(biologicalMatrix: BiologicalMatrix.Blood);
            var hbmSampleSubstanceCollections = FakeHbmDataGenerator.FakeHbmSampleSubstanceCollections(individualDays, substances, samplingMethod, targetUnit);

            // Act
            var calculator = BloodCorrectionCalculatorFactory.Create(StandardiseBloodMethod.EnzymaticSummation, []);
            var result = calculator.ComputeResidueCorrection(hbmSampleSubstanceCollections);

            // Assert: we have only one sample in the collection
            var sampleIn = hbmSampleSubstanceCollections[0].HumanMonitoringSampleSubstanceRecords
                .Select(r => r.HumanMonitoringSampleSubstances)
                .SelectMany(r => r.Values)
                .FirstOrDefault(v => v.MeasuredSubstance.IsLipidSoluble);
            var sampleOut = result[0].HumanMonitoringSampleSubstanceRecords
                .Select(r => r.HumanMonitoringSampleSubstances)
                .SelectMany(r => r.Values)
                .FirstOrDefault(v => v.MeasuredSubstance.IsLipidSoluble);
            var lipidEnzyme = hbmSampleSubstanceCollections[0]
                .HumanMonitoringSampleSubstanceRecords[0]
                .HumanMonitoringSample
                .LipidEnz;

            var actualUnitAlignmentFactor = (sampleOut.Residue / sampleIn.Residue) * lipidEnzyme.Value;
            Assert.AreEqual(expectedUnitAlignmentFactor, actualUnitAlignmentFactor, 0.1);
        }

        [TestMethod]
        [DataRow(ConcentrationUnit.ugPerL, 100.0)]
        [DataRow(ConcentrationUnit.ngPerL, 100.0)]
        [DataRow(ConcentrationUnit.ugPermL, 100000.0)]
        [DataRow(ConcentrationUnit.ngPermL, 100000.0)]
        public void BernertMethod_DifferentTargetUnits_ShouldUseCorrectUnitAlignmentFactor(
            ConcentrationUnit concentrationUnit,
            double expectedUnitAlignmentFactor
        ) {
            // Note: furthermore the concentration should be in L or something, never g

            // Arrange
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var individuals = FakeIndividualsGenerator.Create(1, 1, random, useSamplingWeights: true);
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var substances = FakeSubstancesGenerator.Create(1, lipidSoluble: true);
            var samplingMethod = FakeHbmDataGenerator.FakeHumanMonitoringSamplingMethod(biologicalMatrix: BiologicalMatrix.Blood);
            var hbmSampleSubstanceCollections = FakeHbmDataGenerator
                .FakeHbmSampleSubstanceCollections(individualDays, substances, samplingMethod, concentrationUnit);

            // Act
            var calculator = BloodCorrectionCalculatorFactory.Create(StandardiseBloodMethod.BernertMethod, []);
            var result = calculator.ComputeResidueCorrection(hbmSampleSubstanceCollections);

            // Assert: we have only one sample in the collection
            var sampleIn = hbmSampleSubstanceCollections[0].HumanMonitoringSampleSubstanceRecords
                .Select(r => r.HumanMonitoringSampleSubstances)
                .SelectMany(r => r.Values)
                .FirstOrDefault(v => v.MeasuredSubstance.IsLipidSoluble);
            var sampleOut = result[0].HumanMonitoringSampleSubstanceRecords
                .Select(r => r.HumanMonitoringSampleSubstances)
                .SelectMany(r => r.Values)
                .FirstOrDefault(v => v.MeasuredSubstance.IsLipidSoluble);
            var lipidEnzyme = hbmSampleSubstanceCollections[0]
                .HumanMonitoringSampleSubstanceRecords[0]
                .HumanMonitoringSample
                .LipidEnz;

            // Validation if the correct concentration factor has been applied, based on
            // formula from Bernert et al 2007, see LipidBernertCorrectionCalculator.getSampleSubstance
            var cholesterol = hbmSampleSubstanceCollections[0].HumanMonitoringSampleSubstanceRecords[0].HumanMonitoringSample.Cholesterol.Value;
            var triglycerides = hbmSampleSubstanceCollections[0].HumanMonitoringSampleSubstanceRecords[0].HumanMonitoringSample.Triglycerides.Value;
            var actualUnitAlignmentFactor = sampleOut.Residue * (2.27 * cholesterol + triglycerides + 62.3) / sampleIn.Residue;
            Assert.AreEqual(expectedUnitAlignmentFactor, actualUnitAlignmentFactor, 0.1);
        }

        [TestMethod]
        [DataRow(StandardiseBloodMethod.GravimetricAnalysis)]
        [DataRow(StandardiseBloodMethod.EnzymaticSummation)]
        [DataRow(StandardiseBloodMethod.BernertMethod)]
        public void BloodCorrection_LipidAndNonLipidSolubleSubstances_ShouldApplyAlignmentOnlyToLipidSolubleSubstances(
            StandardiseBloodMethod standardiseBloodMethod
        ) {
            // Arrange
            var seed = 1;
            var lipidGravity = 6.0;
            var random = new McraRandomGenerator(seed);
            var individuals = FakeIndividualsGenerator.Create(5, 2, random, useSamplingWeights: true);
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var substances = FakeSubstancesGenerator.Create(7);
            var samplingMethod = FakeHbmDataGenerator.FakeHumanMonitoringSamplingMethod();
            var hbmSampleSubstanceCollections = FakeHbmDataGenerator.FakeHbmSampleSubstanceCollections(individualDays, substances, samplingMethod, ConcentrationUnit.ugPerL, lipidGravity);

            // Act
            var calculator = BloodCorrectionCalculatorFactory.Create(standardiseBloodMethod, []);
            var result = calculator.ComputeResidueCorrection(hbmSampleSubstanceCollections);

            // Assert
            var samplesIn = hbmSampleSubstanceCollections[0].HumanMonitoringSampleSubstanceRecords
                .Select(r => r.HumanMonitoringSampleSubstances)
                .SelectMany(r => r.Values)
                .OrderBy(c => c.MeasuredSubstance.Code)
                .ThenBy(c => c.Residue)
                .ToList();
            var samplesOut = result.SelectMany(r => r.HumanMonitoringSampleSubstanceRecords)
                .Select(r => r.HumanMonitoringSampleSubstances)
                .SelectMany(r => r.Values)
                .OrderBy(c => c.MeasuredSubstance.Code)
                .ThenBy(c => c.Residue)
                .ToList();

            Assert.AreEqual(samplesIn.Count, samplesOut.Count);
            for (var i = 0; i < samplesIn.Count; ++i) {
                var sampleIn = samplesIn[i];
                var sampleOut = samplesOut[i];

                if (sampleIn.ActiveSubstance.IsLipidSoluble) {
                    // Yes, a correction has been applied for lipid soluble substances
                    Assert.AreNotEqual(sampleIn.Residue, sampleOut.Residue * 100, 0.1);
                } else {
                    // No, no correction should have been applied for non-lipid-soluble substances
                    Assert.AreEqual(sampleIn.Residue, sampleOut.Residue, 0.1);
                }
            }
        }

        [TestMethod]
        [DataRow(StandardiseBloodMethod.GravimetricAnalysis)]
        [DataRow(StandardiseBloodMethod.EnzymaticSummation)]
        [DataRow(StandardiseBloodMethod.BernertMethod)]
        public void BloodStandardisation_GravityOrCreatineValueNull_ShouldYieldMissingValue(
            StandardiseBloodMethod standardiseBloodMethod
        ) {
            // Arrange
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var individuals = FakeIndividualsGenerator.Create(1, 1, random, useSamplingWeights: true);
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var substances = FakeSubstancesGenerator.Create(1, null, null, lipidSoluble: true);
            var samplingMethod = FakeHbmDataGenerator.FakeHumanMonitoringSamplingMethod(biologicalMatrix: BiologicalMatrix.Blood);
            var hbmSampleSubstanceCollections = FakeHbmDataGenerator.FakeHbmSampleSubstanceCollections(individualDays, substances, samplingMethod, ConcentrationUnit.ngPermL);
            // we have only one sample in the collection
            var sampleIn = hbmSampleSubstanceCollections[0].HumanMonitoringSampleSubstanceRecords[0].HumanMonitoringSample;
            sampleIn.LipidEnz = null;
            sampleIn.LipidGrav = null;
            sampleIn.Triglycerides = null;
            sampleIn.Cholesterol = null;

            // Act
            var calculator = BloodCorrectionCalculatorFactory.Create(standardiseBloodMethod, []);
            var result = calculator.ComputeResidueCorrection(hbmSampleSubstanceCollections);

            // Assert: we have only one sample in the collection
            var sampleOut = result[0].HumanMonitoringSampleSubstanceRecords
                .Select(r => r.HumanMonitoringSampleSubstances)
                .SelectMany(r => r.Values)
                .FirstOrDefault(v => v.MeasuredSubstance.IsLipidSoluble);
            Assert.IsTrue(double.IsNaN(sampleOut.Residue));
            Assert.AreEqual(ResType.MV, sampleOut.ResType);
        }

        [TestMethod]
        [DataRow(StandardiseBloodMethod.GravimetricAnalysis)]
        [DataRow(StandardiseBloodMethod.EnzymaticSummation)]
        [DataRow(StandardiseBloodMethod.BernertMethod)]
        public void BloodCorrection_SubstancesExcludedFromLipidStandardisation_ShouldSkipLipidStandardisation(
            StandardiseBloodMethod standardiseBloodMethod
        ) {
            // Arrange
            var random = new McraRandomGenerator(1);
            var individuals = FakeIndividualsGenerator.Create(5, 2, random, useSamplingWeights: true);
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var substances = FakeSubstancesGenerator.Create(10);
            var samplingMethod = FakeHbmDataGenerator.FakeHumanMonitoringSamplingMethod();
            var hbmSampleSubstanceCollections = FakeHbmDataGenerator.FakeHbmSampleSubstanceCollections(individualDays, substances, samplingMethod, ConcentrationUnit.ugPerL, 6.0);

            var substancesExcludedFromLipidStandardisation = substances
                .Where(s => s.IsLipidSoluble).Take(2).Select(s => s.Code).ToList();
            Assert.AreNotEqual(0, substancesExcludedFromLipidStandardisation.Count, "Precondition failed: this test requires at least one substance excluded from lipid standardisation.");

            // Act
            var calculator = BloodCorrectionCalculatorFactory.Create(standardiseBloodMethod, substancesExcludedFromLipidStandardisation);
            var hbmLipidCorrectedSampleSubstanceCollections = calculator.ComputeResidueCorrection(hbmSampleSubstanceCollections);

            // Assert: we check that the residue values, rounded to 4 digits, have not been changed, i.e., not been standardised
            foreach (var substanceCode in substancesExcludedFromLipidStandardisation) {
                var samplesIn = hbmSampleSubstanceCollections
                    .SelectMany(c => c.HumanMonitoringSampleSubstanceRecords)
                    .SelectMany(r => r.HumanMonitoringSampleSubstances.Where(s => s.Key.Code == substanceCode).Select(s => Math.Round(s.Value.Residue, 4)))
                    .ToList();
                var samplesOut = hbmLipidCorrectedSampleSubstanceCollections
                    .SelectMany(c => c.HumanMonitoringSampleSubstanceRecords)
                    .SelectMany(r => r.HumanMonitoringSampleSubstances.Where(s => s.Key.Code == substanceCode).Select(s => Math.Round(s.Value.Residue, 4)))
                    .ToList();
                Assert.IsTrue(samplesOut.SequenceEqual(samplesIn));
            }
        }
    }
}
