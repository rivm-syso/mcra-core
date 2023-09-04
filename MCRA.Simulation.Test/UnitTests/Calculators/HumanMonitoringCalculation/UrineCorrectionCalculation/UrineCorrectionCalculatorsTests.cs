using MCRA.General;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.UrineCorrectionCalculation;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using MCRA.Utils.Statistics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Calculators.HumanMonitoringCalculation.BloodCorrectionCalculation {
    [TestClass]
    public class UrineCorrectionCalculatorsTests {
        /// <summary>
        /// Correction by specific gravity correction factor is independent from the concentration unit.
        /// </summary>
        [TestMethod]
        [DataRow(ConcentrationUnit.ugPermL)]
        [DataRow(ConcentrationUnit.ugPerg)]
        [DataRow(ConcentrationUnit.mgPerdL)]
        [DataRow(ConcentrationUnit.ngPermL)]
        [DataRow(ConcentrationUnit.ngPerg)]
        [DataRow(ConcentrationUnit.gPerL)]
        [DataRow(ConcentrationUnit.mgPerL)]
        [DataRow(ConcentrationUnit.ugPerL)]
        [DataRow(ConcentrationUnit.ngPerL)]
        [DataRow(ConcentrationUnit.pgPerL)]
        public void SpecificGravityCorrection_BySpecificGravityCorrectionFactor_ShouldApplyConstCorrection(ConcentrationUnit targetUnit) {
            // Arrange
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var individuals = MockIndividualsGenerator.Create(1, 1, random, useSamplingWeights: true);
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var substances = MockSubstancesGenerator.Create(1, null, null);
            var samplingMethod = FakeHbmDataGenerator.FakeHumanMonitoringSamplingMethod(biologicalMatrix: BiologicalMatrix.Urine);
            var hbmSampleSubstanceCollections = FakeHbmDataGenerator.FakeHbmSampleSubstanceCollections(individualDays, substances, samplingMethod, targetUnit);
            Assert.IsNotNull(hbmSampleSubstanceCollections[0].HumanMonitoringSampleSubstanceRecords[0].HumanMonitoringSample.SpecificGravityCorrectionFactor, "This test assumes a value for SpecificGravityCorrectionFactor in the fake test data.");
            var exptectedSpecificGravityCorrectionFactor = hbmSampleSubstanceCollections[0].HumanMonitoringSampleSubstanceRecords[0].HumanMonitoringSample.SpecificGravityCorrectionFactor.Value;

            // Act
            var calculator = UrineCorrectionCalculatorFactory.Create(StandardiseUrineMethod.SpecificGravity, new());
            var result = calculator.ComputeResidueCorrection(hbmSampleSubstanceCollections);

            // Assert
            // Note: we have only one sample in the collection
            var sampleIn = hbmSampleSubstanceCollections[0].HumanMonitoringSampleSubstanceRecords.Select(r => r.HumanMonitoringSampleSubstances)
                                                            .SelectMany(r => r.Select(kvp => kvp.Value)).FirstOrDefault();
            var sampleOut = result[0].HumanMonitoringSampleSubstanceRecords.Select(r => r.HumanMonitoringSampleSubstances)
                                                            .SelectMany(r => r.Select(kvp => kvp.Value)).FirstOrDefault();

            var actualSpecificGravityCorrectionFactor = (sampleOut.Residue / sampleIn.Residue);
            Assert.AreEqual(exptectedSpecificGravityCorrectionFactor, actualSpecificGravityCorrectionFactor, 0.1);
        }

        /// <summary>
        /// Urine correction by specific gravity is independent from the concentration unit.
        /// </summary>
        [TestMethod]
        [DataRow(ConcentrationUnit.ugPermL)]
        [DataRow(ConcentrationUnit.ugPerg)]
        [DataRow(ConcentrationUnit.mgPerdL)]
        [DataRow(ConcentrationUnit.ngPermL)]
        [DataRow(ConcentrationUnit.ngPerg)]
        [DataRow(ConcentrationUnit.gPerL)]
        [DataRow(ConcentrationUnit.mgPerL)]
        [DataRow(ConcentrationUnit.ugPerL)]
        [DataRow(ConcentrationUnit.ngPerL)]
        [DataRow(ConcentrationUnit.pgPerL)]
        public void SpecificGravityCorrection_BySpecificGravity_ShouldApplyCorrectFactor(ConcentrationUnit targetUnit) {
            // Arrange
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var individuals = MockIndividualsGenerator.Create(1, 1, random, useSamplingWeights: true);
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var substances = MockSubstancesGenerator.Create(1, null, null);
            var samplingMethod = FakeHbmDataGenerator.FakeHumanMonitoringSamplingMethod(biologicalMatrix: BiologicalMatrix.Urine);
            var hbmSampleSubstanceCollections = FakeHbmDataGenerator.FakeHbmSampleSubstanceCollections(individualDays, substances, samplingMethod, targetUnit);
            Assert.IsNotNull(hbmSampleSubstanceCollections[0].HumanMonitoringSampleSubstanceRecords[0].HumanMonitoringSample.SpecificGravity, "This test assumes a value for SpecificGravity in the fake test data.");
            hbmSampleSubstanceCollections[0].HumanMonitoringSampleSubstanceRecords[0].HumanMonitoringSample.SpecificGravityCorrectionFactor = null;
            var expectedSpecificGravity = hbmSampleSubstanceCollections[0].HumanMonitoringSampleSubstanceRecords[0].HumanMonitoringSample.SpecificGravity.Value;

            // Act
            var calculator = UrineCorrectionCalculatorFactory.Create(StandardiseUrineMethod.SpecificGravity, new());
            var result = calculator.ComputeResidueCorrection(hbmSampleSubstanceCollections);

            // Assert
            // Note: we have only one sample in the collection
            var sampleIn = hbmSampleSubstanceCollections[0].HumanMonitoringSampleSubstanceRecords.Select(r => r.HumanMonitoringSampleSubstances)
                                                            .SelectMany(r => r.Select(kvp => kvp.Value)).FirstOrDefault();
            var sampleOut = result[0].HumanMonitoringSampleSubstanceRecords.Select(r => r.HumanMonitoringSampleSubstances)
                                                            .SelectMany(r => r.Select(kvp => kvp.Value)).FirstOrDefault();

            var actualSpecificGravity = (sampleIn.Residue / sampleOut.Residue) * (1.024 -1) + 1;
            Assert.AreEqual(expectedSpecificGravity, actualSpecificGravity, 0.1);
        }

        /// <summary>
        /// Creatinine correction does depend on the target concentration unit.
        /// </summary>
        /// <param name="targetUnit"></param>
        /// <param name="expectedUnitAlignmentFactor"></param>
        [TestMethod]
        [DataRow(ConcentrationUnit.ugPermL, 100000.0)]
        [DataRow(ConcentrationUnit.ugPerg, 100000.0)]
        [DataRow(ConcentrationUnit.mgPerdL, 1000.0)]
        [DataRow(ConcentrationUnit.ngPermL, 100000.0)]
        [DataRow(ConcentrationUnit.ngPerg, 100000.0)]
        [DataRow(ConcentrationUnit.gPerL, 100.0)]
        [DataRow(ConcentrationUnit.mgPerL, 100.0)]
        [DataRow(ConcentrationUnit.ugPerL, 100.0)]
        [DataRow(ConcentrationUnit.ngPerL, 100.0)]
        [DataRow(ConcentrationUnit.pgPerL, 100.0)]
        public void CreatinineStandardisation_BySpecificGravity_ShouldApplyCorrectFactor(ConcentrationUnit targetUnit, double expectedUnitAlignmentFactor) {
            // Arrange
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var individuals = MockIndividualsGenerator.Create(1, 1, random, useSamplingWeights: true);
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var substances = MockSubstancesGenerator.Create(1, null, null);
            var samplingMethod = FakeHbmDataGenerator.FakeHumanMonitoringSamplingMethod(biologicalMatrix: BiologicalMatrix.Urine);
            var hbmSampleSubstanceCollections = FakeHbmDataGenerator.FakeHbmSampleSubstanceCollections(individualDays, substances, samplingMethod, targetUnit);
            var creatinineConcentration = hbmSampleSubstanceCollections[0].HumanMonitoringSampleSubstanceRecords[0].HumanMonitoringSample.Creatinine;

            // Act
            var calculator = UrineCorrectionCalculatorFactory.Create(StandardiseUrineMethod.CreatinineStandardisation, new());
            var result = calculator.ComputeResidueCorrection(hbmSampleSubstanceCollections);

            // Assert: we have only one sample in the collection
            var sampleIn = hbmSampleSubstanceCollections[0].HumanMonitoringSampleSubstanceRecords.Select(r => r.HumanMonitoringSampleSubstances)
                                                            .SelectMany(r => r.Select(kvp => kvp.Value)).FirstOrDefault();
            var sampleOut = result[0].HumanMonitoringSampleSubstanceRecords.Select(r => r.HumanMonitoringSampleSubstances)
                                                            .SelectMany(r => r.Select(kvp => kvp.Value)).FirstOrDefault();

            var actualUnitAlignmentFactor = (sampleOut.Residue / sampleIn.Residue) * creatinineConcentration.Value;
            Assert.AreEqual(expectedUnitAlignmentFactor, actualUnitAlignmentFactor, 0.1);
        }

        [TestMethod]
        [DataRow(StandardiseUrineMethod.SpecificGravity)]
        [DataRow(StandardiseUrineMethod.CreatinineStandardisation)]
        public void UrineStandardisation_GravityOrCreatineValueNull_ShouldYieldMissingValue(StandardiseUrineMethod standardiseUrineMethod) {
            // Arrange
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var individuals = MockIndividualsGenerator.Create(1, 1, random, useSamplingWeights: true);
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var substances = MockSubstancesGenerator.Create(1, null, null);
            var samplingMethod = FakeHbmDataGenerator.FakeHumanMonitoringSamplingMethod(biologicalMatrix: BiologicalMatrix.Urine);
            var hbmSampleSubstanceCollections = FakeHbmDataGenerator.FakeHbmSampleSubstanceCollections(individualDays, substances, samplingMethod, ConcentrationUnit.ugPermL);
            // we have only one sample in the collection
            var sampleIn = hbmSampleSubstanceCollections[0].HumanMonitoringSampleSubstanceRecords[0].HumanMonitoringSample;
            sampleIn.SpecificGravityCorrectionFactor = null;
            sampleIn.SpecificGravity = null;
            sampleIn.Creatinine = null;

            // Act
            var calculator = UrineCorrectionCalculatorFactory.Create(standardiseUrineMethod, new());
            var result = calculator.ComputeResidueCorrection(hbmSampleSubstanceCollections);

            // Assert
            var sampleOut = result[0].HumanMonitoringSampleSubstanceRecords.Select(r => r.HumanMonitoringSampleSubstances)
                                                            .SelectMany(r => r.Select(kvp => kvp.Value)).FirstOrDefault();
            Assert.IsTrue(double.IsNaN(sampleOut.Residue));
            Assert.AreEqual(ResType.MV, sampleOut.ResType);
        }

        [TestMethod]
        [DataRow(StandardiseUrineMethod.SpecificGravity)]
        [DataRow(StandardiseUrineMethod.CreatinineStandardisation)]
        public void UrineCorrection_SubstancesExcludedFromStandardisation_ShouldSkipStandardisation(StandardiseUrineMethod standardiseUrineMethod) {
            // Arrange
            var biologicalMatrix = BiologicalMatrix.Urine;
            var timeScaleUnit = TimeScaleUnit.SteadyState;
            var concentrationUnit = ConcentrationUnit.ugPerL;
            var random = new McraRandomGenerator(1);
            var individuals = MockIndividualsGenerator.Create(1, 1, random, useSamplingWeights: true);
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var substances = MockSubstancesGenerator.Create(10);
            var samplingMethod = FakeHbmDataGenerator.FakeHumanMonitoringSamplingMethod(biologicalMatrix);
            var hbmSampleSubstanceCollections = FakeHbmDataGenerator.FakeHbmSampleSubstanceCollections(individualDays, substances, samplingMethod, ConcentrationUnit.ugPermL);
            var targetUnitDefault = new TargetUnit(concentrationUnit.GetSubstanceAmountUnit(), concentrationUnit.GetConcentrationMassUnit(), timeScaleUnit, biologicalMatrix);
            var substancesExcludedFromStandardisation = substances.Take(2).Select(s => s.Code).ToList();

            // Act
            var calculator = UrineCorrectionCalculatorFactory.Create(standardiseUrineMethod, substancesExcludedFromStandardisation);
            var hbmUrineCorrectedSampleSubstanceCollections = calculator.ComputeResidueCorrection(hbmSampleSubstanceCollections);

            // Assert: we check that the residue values, rounded to 4 digits, have not been changed, i.e., not been standardised
            foreach (var substanceCode in substancesExcludedFromStandardisation) {
                var samplesIn = hbmSampleSubstanceCollections[0].HumanMonitoringSampleSubstanceRecords.SelectMany(r => r.HumanMonitoringSampleSubstances.Where(s => s.Key.Code == substanceCode).Select(s => Math.Round(s.Value.Residue, 4))).ToList();
                var samplesOut = hbmUrineCorrectedSampleSubstanceCollections[0].HumanMonitoringSampleSubstanceRecords.SelectMany(r => r.HumanMonitoringSampleSubstances.Where(s => s.Key.Code == substanceCode).Select(s => Math.Round(s.Value.Residue, 4))).ToList();
                Assert.IsTrue(samplesOut.SequenceEqual(samplesIn));
            }
        }
    }
}
