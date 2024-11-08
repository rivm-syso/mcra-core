using MCRA.General;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualDayConcentrationCalculation;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using MCRA.Utils.Statistics;
using MCRA.Utils.Test;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.HumanMonitoringAnalysis {
    /// <summary>
    /// OutputGeneration, ActionSummaries, HumanMonitoringAnalysis, HbmVsModelledExposureBySubstance
    /// </summary>
    [TestClass]
    public class DayConcentrationCorrelationsBySubstanceSectionTests : SectionTestBase {

        private static string _outputPath = TestUtilities.ConcatWithOutputPath("HbmVsModelledIndividualDayExposures");

        [ClassInitialize]
        public static void Initialize(TestContext testContext) {
            if (Directory.Exists(_outputPath)) {
                Directory.Delete(_outputPath, true);
                Thread.Sleep(100);
            }
            Directory.CreateDirectory(_outputPath);
        }

        /// <summary>
        /// Test monitoring vs modelled exposures section for case in which for each monitoring exposure
        /// there is precisely one modelled exposure.
        /// </summary>
        [TestMethod]
        public void DayConcentrationCorrelationsBySubstanceSection_TestSimple() {
            int seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = MockSubstancesGenerator.Create(3);
            var individuals = FakeIndividualsGenerator.Create(50, 2, random);
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var hbmTargetUnit = TargetUnit.FromInternalDoseUnit(DoseUnit.ugPerL, BiologicalMatrix.Blood);
            var modelledExposuresUnit = TargetUnit.FromInternalDoseUnit(DoseUnit.ugPerL, BiologicalMatrix.Blood);
            var zeroFractions = new double[] { 0, .5, 1 };
            for (int i = 0; i < zeroFractions.Length; i++) {
                var exposureZeroFraction = zeroFractions[i];
                for (int j = 0; j < zeroFractions.Length; j++) {
                    var monitoringZeroFraction = zeroFractions[j];
                    var targetExposures = MockTargetExposuresGenerator.MockIndividualDayExposures(
                        individualDays, 
                        substances, 
                        random, 
                        fractionZeros: exposureZeroFraction
                    );
                    var samplingMethod = FakeHbmDataGenerator.FakeHumanMonitoringSamplingMethod();
                    var monitoringExposures = FakeHbmDataGenerator.MockHumanMonitoringIndividualDayConcentrations(individualDays, substances, samplingMethod, monitoringZeroFraction, seed: seed + 1);
                    var section = new DayConcentrationCorrelationsBySubstanceSection();
                    section.Summarize(
                        targetExposures: targetExposures,
                        hbmIndividualDayConcentrationsCollections: [ 
                            new HbmIndividualDayCollection() { 
                                TargetUnit = hbmTargetUnit,
                                HbmIndividualDayConcentrations = monitoringExposures 
                            } 
                        ],
                        substances: substances,
                        targetExposureUnit: hbmTargetUnit,
                        lowerPercentage: 2.5,
                        upperPercentage: 97.5
                    );
                    for (int k = 0; k < substances.Count; k++) {
                        var chartCreator = new DayConcentrationCorrelationsChartCreator(
                            section, 
                            substances[k].Code,
                            modelledExposuresUnit.GetShortDisplayName(),
                            hbmTargetUnit.GetShortDisplayName(), 
                            2.5, 
                            97.5, 
                            400, 
                            300
                        );
                        chartCreator.CreateToPng(Path.Combine(_outputPath, $"TestSimple_{i}_{j}_{k}.png"));
                    }
                    AssertIsValidView(section);
                }
            }
        }

        /// <summary>
        /// Test monitoring vs modelled exposures section for case in which for each monitoring exposure
        /// there are multiple modelled exposure (variability).
        /// </summary>
        [TestMethod]
        public void DayConcentrationCorrelationsBySubstanceSection_TestModelVariation() {
            int seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = MockSubstancesGenerator.Create(5);
            var individuals = FakeIndividualsGenerator.Create(50, 2, random);
            var monitoringIndividualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals, 1);
            var modelledIndividualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals, 10);
            var zeroFractions = new double[] { 0, .5, 1 };
            var hbmTargetUnit = TargetUnit.FromInternalDoseUnit(DoseUnit.ugPerL, BiologicalMatrix.Blood);

            for (int i = 0; i < zeroFractions.Length; i++) {
                var exposureZeroFraction = zeroFractions[i];
                for (int j = 0; j < zeroFractions.Length; j++) {
                    var monitoringZeroFraction = zeroFractions[j];
                    var targetExposures = MockTargetExposuresGenerator.MockIndividualDayExposures(modelledIndividualDays, substances, random, fractionZeros: exposureZeroFraction);
                    var samplingMethod = FakeHbmDataGenerator.FakeHumanMonitoringSamplingMethod();
                    var monitoringExposures = FakeHbmDataGenerator.MockHumanMonitoringIndividualDayConcentrations(
                        monitoringIndividualDays, 
                        substances, 
                        samplingMethod, 
                        monitoringZeroFraction, 
                        seed: seed + 1
                    );
                    var section = new DayConcentrationCorrelationsBySubstanceSection();
                    section.Summarize(
                        targetExposures: targetExposures,
                        hbmIndividualDayConcentrationsCollections: [
                            new HbmIndividualDayCollection() {
                                TargetUnit = hbmTargetUnit,
                                HbmIndividualDayConcentrations = monitoringExposures
                            }
                        ], substances: substances,
                        targetExposureUnit: hbmTargetUnit,
                        lowerPercentage: 2.5,
                        upperPercentage: 97.5
                    );
                    for (int k = 0; k < substances.Count; k++) {
                        var chartCreator = new DayConcentrationCorrelationsChartCreator(section, substances[k].Code, "mg/kg bw/day", "mg/kg", 2.5, 97.5, 400, 300);
                        chartCreator.CreateToPng(Path.Combine(_outputPath, $"TestModelVariation_{i}_{j}_{k}.png"));
                    }
                    AssertIsValidView(section);
                }
            }
        }
    }
}
