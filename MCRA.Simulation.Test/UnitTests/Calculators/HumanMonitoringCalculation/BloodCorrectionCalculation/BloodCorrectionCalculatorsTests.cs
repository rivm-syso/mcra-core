﻿using MCRA.General;
using MCRA.General.UnitDefinitions.Enums;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.BloodCorrectionCalculation;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using MCRA.Utils.Statistics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Calculators.HumanMonitoringCalculation.BloodCorrectionCalculation {
    [TestClass]
    public class BloodCorrectionCalculatorsTests {
        [TestMethod]
        [DataRow(ConcentrationUnit.ngPermL, 100000.0)]      // 100,000 == factor 100 for dL-to-mL and factor 1000 for mg-to-g
        [DataRow(ConcentrationUnit.mgPerdL, 1000.0)]
        [DataRow(ConcentrationUnit.gPerL, 100.0)]
        [DataRow(ConcentrationUnit.mgPerL, 100.0)]
        [DataRow(ConcentrationUnit.ugPermL, 100000.0)]
        [DataRow(ConcentrationUnit.ugPerL, 100.0)]
        [DataRow(ConcentrationUnit.ngPerL, 100.0)]
        [DataRow(ConcentrationUnit.pgPerL, 100.0)]
        public void GravimetricCorrection_DifferentTargetUnits_ShouldUseCorrectUnitAlignmentFactor(ConcentrationUnit targetUnit, double expectedUnitAlignmentFactor) {
            // Arrange
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var individuals = MockIndividualsGenerator.Create(1, 1, random, useSamplingWeights: true);
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var substances = MockSubstancesGenerator.Create(1, null, null, isLipidSoluble: true);
            var samplingMethod = FakeHbmDataGenerator.FakeHumanMonitoringSamplingMethod(biologicalMatrix: "Blood");
            var hbmSampleSubstanceCollections = FakeHbmDataGenerator.FakeHbmSampleSubstanceCollections(individualDays, substances, samplingMethod, "mg/L");

            // Act
            var calculator = BloodCorrectionCalculatorFactory.Create(StandardiseBloodMethod.GravimetricAnalysis);
            var result = calculator.ComputeTotalLipidCorrection(hbmSampleSubstanceCollections, targetUnit, "Blood", new CompartmentUnitCollector(TimeScaleUnit.PerDay));

            // Assert: we have only one sample in the collection
            var sampleIn = hbmSampleSubstanceCollections[0].HumanMonitoringSampleSubstanceRecords.Select(r => r.HumanMonitoringSampleSubstances)
                                                            .SelectMany(r => r.Select(kvp => kvp.Value)).Where(v => (bool)v.MeasuredSubstance.IsLipidSoluble).FirstOrDefault();
            var sampleOut = result[0].HumanMonitoringSampleSubstanceRecords.Select(r => r.HumanMonitoringSampleSubstances)
                                                            .SelectMany(r => r.Select(kvp => kvp.Value)).Where(v => (bool)v.MeasuredSubstance.IsLipidSoluble).FirstOrDefault();
            var lipidGravity = hbmSampleSubstanceCollections[0].HumanMonitoringSampleSubstanceRecords[0].HumanMonitoringSample.LipidGrav;

            var actualUnitAlignmentFactor = (sampleOut.Residue / sampleIn.Residue) * lipidGravity.Value;
            Assert.AreEqual(expectedUnitAlignmentFactor, actualUnitAlignmentFactor, 0.1);
        }

        [TestMethod]
        [DataRow(ConcentrationUnit.ngPermL, 100000.0)]
        [DataRow(ConcentrationUnit.ugPermL, 100000.0)]
        [DataRow(ConcentrationUnit.mgPerdL, 1000.0)]
        [DataRow(ConcentrationUnit.gPerL, 100.0)]
        [DataRow(ConcentrationUnit.mgPerL, 100.0)]
        [DataRow(ConcentrationUnit.ugPerL, 100.0)]
        [DataRow(ConcentrationUnit.ngPerL, 100.0)]
        [DataRow(ConcentrationUnit.pgPerL, 100.0)]
        public void EnzymaticSummation_DifferentTargetUnits_ShouldUseCorrectUnitAlignmentFactor(ConcentrationUnit targetUnit, double expectedUnitAlignmentFactor) {
            // Arrange
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var individuals = MockIndividualsGenerator.Create(1, 1, random, useSamplingWeights: true);
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var substances = MockSubstancesGenerator.Create(1, null, null, isLipidSoluble: true);
            var samplingMethod = FakeHbmDataGenerator.FakeHumanMonitoringSamplingMethod(biologicalMatrix: "Blood");
            var hbmSampleSubstanceCollections = FakeHbmDataGenerator.FakeHbmSampleSubstanceCollections(individualDays, substances, samplingMethod, "mg/L");

            // Act
            var calculator = BloodCorrectionCalculatorFactory.Create(StandardiseBloodMethod.EnzymaticSummation);
            var result = calculator.ComputeTotalLipidCorrection(hbmSampleSubstanceCollections, targetUnit, "Blood", new CompartmentUnitCollector(TimeScaleUnit.PerDay));

            // Assert: we have only one sample in the collection
            var sampleIn = hbmSampleSubstanceCollections[0].HumanMonitoringSampleSubstanceRecords.Select(r => r.HumanMonitoringSampleSubstances)
                                                            .SelectMany(r => r.Select(kvp => kvp.Value)).Where(v => (bool)v.MeasuredSubstance.IsLipidSoluble).FirstOrDefault();
            var sampleOut = result[0].HumanMonitoringSampleSubstanceRecords.Select(r => r.HumanMonitoringSampleSubstances)
                                                            .SelectMany(r => r.Select(kvp => kvp.Value)).Where(v => (bool)v.MeasuredSubstance.IsLipidSoluble).FirstOrDefault();
            var lipidEnzyme = hbmSampleSubstanceCollections[0].HumanMonitoringSampleSubstanceRecords[0].HumanMonitoringSample.LipidEnz;

            var actualUnitAlignmentFactor = (sampleOut.Residue / sampleIn.Residue) * lipidEnzyme.Value;
            Assert.AreEqual(expectedUnitAlignmentFactor, actualUnitAlignmentFactor, 0.1);
        }

        [TestMethod]
        [DataRow(ConcentrationUnit.ugPermL, 100000.0)]
        [DataRow(ConcentrationUnit.mgPerdL, 1000.0)]
        [DataRow(ConcentrationUnit.ngPermL, 100000.0)]
        [DataRow(ConcentrationUnit.gPerL, 100.0)]
        [DataRow(ConcentrationUnit.mgPerL, 100.0)]
        [DataRow(ConcentrationUnit.ugPerL, 100.0)]
        [DataRow(ConcentrationUnit.ngPerL, 100.0)]
        [DataRow(ConcentrationUnit.pgPerL, 100.0)]
        public void BernertMethod_DifferentTargetUnits_ShouldUseCorrectUnitAlignmentFactor(ConcentrationUnit targetUnit, double expectedUnitAlignmentFactor) {
            // Arrange
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var individuals = MockIndividualsGenerator.Create(1, 1, random, useSamplingWeights: true);
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var substances = MockSubstancesGenerator.Create(1, null, null, isLipidSoluble: true);
            var samplingMethod = FakeHbmDataGenerator.FakeHumanMonitoringSamplingMethod(biologicalMatrix: "Blood");
            var hbmSampleSubstanceCollections = FakeHbmDataGenerator.FakeHbmSampleSubstanceCollections(individualDays, substances, samplingMethod, "mg/L");

            // Act
            var calculator = BloodCorrectionCalculatorFactory.Create(StandardiseBloodMethod.BernertMethod);
            var result = calculator.ComputeTotalLipidCorrection(hbmSampleSubstanceCollections, targetUnit, "Blood", new CompartmentUnitCollector(TimeScaleUnit.PerDay));

            // Assert: we have only one sample in the collection
            var sampleIn = hbmSampleSubstanceCollections[0].HumanMonitoringSampleSubstanceRecords.Select(r => r.HumanMonitoringSampleSubstances)
                                                            .SelectMany(r => r.Select(kvp => kvp.Value)).Where(v => (bool)v.MeasuredSubstance.IsLipidSoluble).FirstOrDefault();
            var sampleOut = result[0].HumanMonitoringSampleSubstanceRecords.Select(r => r.HumanMonitoringSampleSubstances)
                                                            .SelectMany(r => r.Select(kvp => kvp.Value)).Where(v => (bool)v.MeasuredSubstance.IsLipidSoluble).FirstOrDefault();
            var lipidEnzyme = hbmSampleSubstanceCollections[0].HumanMonitoringSampleSubstanceRecords[0].HumanMonitoringSample.LipidEnz;

            // Validation if the correct concentration factor has been applied, based on formula from Bernert et al 2007, see LipidBernertCorrectionCalculator.getSampleSubstance
            var cholesterol = hbmSampleSubstanceCollections[0].HumanMonitoringSampleSubstanceRecords[0].HumanMonitoringSample.Cholesterol.Value;
            var triglycerides = hbmSampleSubstanceCollections[0].HumanMonitoringSampleSubstanceRecords[0].HumanMonitoringSample.Triglycerides.Value;
            var actualUnitAlignmentFactor = sampleOut.Residue * (2.27 * cholesterol + triglycerides + 62.3) / sampleIn.Residue;
            Assert.AreEqual(expectedUnitAlignmentFactor, actualUnitAlignmentFactor, 0.1);
        }

        [TestMethod]
        [DataRow(StandardiseBloodMethod.GravimetricAnalysis)]
        [DataRow(StandardiseBloodMethod.EnzymaticSummation)]
        [DataRow(StandardiseBloodMethod.BernertMethod)]
        public void BloodCorrection_LipidAndNonLipidSolubleSubstances_ShouldApplyAlignmentOnlyToLipidSolubleSubstances(StandardiseBloodMethod standardiseBloodMethod) {
            // Arrange
            var seed = 1;
            var lipidGravity = 6.0;
            var random = new McraRandomGenerator(seed);
            var individuals = MockIndividualsGenerator.Create(5, 2, random, useSamplingWeights: true);
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var substances = MockSubstancesGenerator.Create(7);
            var samplingMethod = FakeHbmDataGenerator.FakeHumanMonitoringSamplingMethod();
            var hbmSampleSubstanceCollections = FakeHbmDataGenerator.FakeHbmSampleSubstanceCollections(individualDays, substances, samplingMethod, "mg/L", lipidGravity);

            // Act
            var calculator = BloodCorrectionCalculatorFactory.Create(standardiseBloodMethod);
            var result = calculator.ComputeTotalLipidCorrection(hbmSampleSubstanceCollections, ConcentrationUnit.ngPermL, "Blood", new CompartmentUnitCollector(TimeScaleUnit.PerDay));

            // Assert
            var samplesIn = hbmSampleSubstanceCollections[0].HumanMonitoringSampleSubstanceRecords.Select(r => r.HumanMonitoringSampleSubstances)
                                                            .SelectMany(r => r.Select(kvp => kvp.Value)).ToList();
            var samplesOut = result[0].HumanMonitoringSampleSubstanceRecords.Select(r => r.HumanMonitoringSampleSubstances)
                                                            .SelectMany(r => r.Select(kvp => kvp.Value)).ToList();

            Assert.AreEqual(samplesIn.Count, samplesOut.Count);
            for (var i = 0; i < samplesIn.Count; ++i) {
                var sampleIn = samplesIn[i];
                var sampleOut = samplesOut[i];

                if ((bool)sampleIn.ActiveSubstance.IsLipidSoluble) {
                    Assert.AreNotEqual(sampleIn.Residue, sampleOut.Residue, 0.1);       // Yes, a correction has been applied for lipid soluble substances
                } else {
                    Assert.AreEqual(sampleIn.Residue, sampleOut.Residue, 0.1);          // No, no correction should have been applied for non-lipid-soluble substances
                }
            }
        }

        [TestMethod]
        [DataRow(StandardiseBloodMethod.GravimetricAnalysis)]
        [DataRow(StandardiseBloodMethod.EnzymaticSummation)]
        [DataRow(StandardiseBloodMethod.BernertMethod)]
        public void BloodStandardisation_GravityOrCreatineValueNull_ShouldYieldMissingValue(StandardiseBloodMethod standardiseBloodMethod) {
            // Arrange
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var individuals = MockIndividualsGenerator.Create(1, 1, random, useSamplingWeights: true);
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var substances = MockSubstancesGenerator.Create(1, null, null, isLipidSoluble: true);
            var samplingMethod = FakeHbmDataGenerator.FakeHumanMonitoringSamplingMethod(biologicalMatrix: "Blood");
            var hbmSampleSubstanceCollections = FakeHbmDataGenerator.FakeHbmSampleSubstanceCollections(individualDays, substances, samplingMethod, "mg/L");
            // we have only one sample in the collection
            var sampleIn = hbmSampleSubstanceCollections[0].HumanMonitoringSampleSubstanceRecords[0].HumanMonitoringSample;
            sampleIn.LipidEnz = null;
            sampleIn.LipidGrav = null;
            sampleIn.Triglycerides = null;
            sampleIn.Cholesterol = null;

            // Act
            var calculator = BloodCorrectionCalculatorFactory.Create(standardiseBloodMethod);
            var result = calculator.ComputeTotalLipidCorrection(hbmSampleSubstanceCollections, ConcentrationUnit.ugPermL, "Blood", new CompartmentUnitCollector(TimeScaleUnit.PerDay));

            // Assert: we have only one sample in the collection
            var sampleOut = result[0].HumanMonitoringSampleSubstanceRecords.Select(r => r.HumanMonitoringSampleSubstances)
                                                            .SelectMany(r => r.Select(kvp => kvp.Value)).Where(v => (bool)v.MeasuredSubstance.IsLipidSoluble).FirstOrDefault();
            Assert.IsTrue(double.IsNaN(sampleOut.Residue));
            Assert.AreEqual(ResType.MV, sampleOut.ResType);
        }
    }
}