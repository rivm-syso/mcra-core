using MCRA.General;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.CorrectionCalculators.UrineCorrectionCalculation;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using MCRA.Utils.Statistics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Calculators.HumanMonitoringCalculation.BloodCorrectionCalculation {
    [TestClass]
    public class SpecificGravityFromCreatinineNonLinearCalculatorTests {

        /// <summary>
        /// The algorithm of Busgang et al. for the conversion from creatinine to specific gravity uses
        /// a fixed unit of mg/dL for the creatinine concentration in urine. This test verifies that the creatinine
        /// concentration of the HBM data is corrected and aligned to this fixed mg/dL unit.
        /// </summary>
        /// <param name="creatinineUnit">The concentration unit of creatinine in the HBM data.</param>
        /// <param name="creatinineCorrectionFactor">The correction that should be applied to the HBM creatinine concentration.</param>
        [TestMethod]
        [DataRow(ConcentrationUnit.mgPerdL, 1.0)]
        [DataRow(ConcentrationUnit.mgPerL, 0.1)]
        [DataRow(ConcentrationUnit.gPerL, 100)]
        public void SpecificGravityFromCreatinineNonLinearCalculator_DifferentCreatinineUnits_ShouldUseCorrectCreatinineUnitAlignmentFactor(
            ConcentrationUnit creatinineUnit,
            double creatinineCorrectionFactor
        ) {
            // Arrange
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var individuals = FakeIndividualsGenerator.Create(1, 1, random, useSamplingWeights: true);
            FakeIndividualsGenerator.AddFakeSexProperty(individuals, random);
            FakeIndividualsGenerator.AddFakeAgeProperty(individuals, random);

            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var substances = FakeSubstancesGenerator.Create(1);
            var samplingMethod = FakeHbmDataGenerator.FakeHumanMonitoringSamplingMethod(biologicalMatrix: BiologicalMatrix.Urine);
            var hbmSampleSubstanceCollections = FakeHbmDataGenerator.FakeHbmSampleSubstanceCollections(
                individualDays, substances, samplingMethod, ConcentrationUnit.ugPerL);
            hbmSampleSubstanceCollections.ForEach(s => { s.CreatConcentrationUnit = creatinineUnit; });

            // Act
            var calculator = new SpecificGravityFromCreatinineNonlinearModelTwoCalculator([]);
            var result = calculator.ComputeResidueCorrection(hbmSampleSubstanceCollections);

            // Assert: we have only one sample in the collection
            var sampleIn = hbmSampleSubstanceCollections[0].HumanMonitoringSampleSubstanceRecords
                .Select(r => r.HumanMonitoringSampleSubstances)
                .SelectMany(r => r.Values)
                .FirstOrDefault();
            var sampleOut = result[0].HumanMonitoringSampleSubstanceRecords
                .Select(r => r.HumanMonitoringSampleSubstances)
                .SelectMany(r => r.Values)
                .FirstOrDefault();
            var creatinine = hbmSampleSubstanceCollections[0].HumanMonitoringSampleSubstanceRecords[0].HumanMonitoringSample.Creatinine;
            var age = hbmSampleSubstanceCollections[0].HumanMonitoringSampleSubstanceRecords[0].Individual.GetAge();
            var gender = hbmSampleSubstanceCollections[0].HumanMonitoringSampleSubstanceRecords[0].Individual.GetGender();

            creatinine *= creatinineCorrectionFactor;
            var sg = SpecificGravityFromCreatinineNonlinearModelTwoCalculator.BusgangSpecificGravity.Calculate(creatinine.Value, age.Value, gender);
            var sgCorrectionFactor = (1.024 - 1) / (sg - 1);

            // Expected corrected residue value
            var expectedResidue = sgCorrectionFactor * sampleIn.Residue;
            Assert.AreEqual(expectedResidue, sampleOut.Residue, 0.001);
        }
    }
}
