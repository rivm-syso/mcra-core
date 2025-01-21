using MCRA.General;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualConcentrationCalculation;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using MCRA.Utils.Statistics;
using MCRA.Simulation.Test.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.HumanMonitoringAnalysis {
    /// <summary>
    /// OutputGeneration, ActionSummaries, HumanMonitoringAnalysis, HbmVsModelledExposureBySubstance
    /// </summary>
    [TestClass]
    public class IndividualConcentrationCorrelationsBySubstanceSectionTests : SectionTestBase {

        private static readonly string _outputPath = TestUtilities.ConcatWithOutputPath("HbmVsModelledIndividualExposures");

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
        public void IndividualConcentrationCorrelationsBySubstanceSection_TestSimple() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = FakeSubstancesGenerator.Create(3);
            var individuals = FakeIndividualsGenerator.Create(50, 2, random);
            var zeroFractions = new double[] { 0, .5, 1 };
            var targetExposureUnit = TargetUnit.FromInternalDoseUnit(DoseUnit.ugPerL, BiologicalMatrix.Blood);
            var targetHbmUnit = TargetUnit.FromInternalDoseUnit(DoseUnit.ugPerL, BiologicalMatrix.Blood);

            for (int i = 0; i < zeroFractions.Length; i++) {
                var exposureZeroFraction = zeroFractions[i];
                for (int j = 0; j < zeroFractions.Length; j++) {
                    var monitoringZeroFraction = zeroFractions[j];
                    var targetExposures = FakeTargetExposuresGenerator.MockIndividualExposures(individuals, substances, random, fractionZeros: exposureZeroFraction);
                    var monitoringExposures = FakeHbmDataGenerator.MockHumanMonitoringIndividualConcentrations(individuals, substances, monitoringZeroFraction, seed: seed + 1);
                    var collection = new List<HbmIndividualCollection>() {
                        new() {
                            TargetUnit = targetHbmUnit,
                            HbmIndividualConcentrations = monitoringExposures
                        }
                    };
                    var section = new IndividualConcentrationCorrelationsBySubstanceSection();
                    section.Summarize(targetExposures, collection, substances, targetExposureUnit, 2.5, 97.5);
                    for (int k = 0; k < substances.Count; k++) {
                        var chartCreator = new IndividualConcentrationCorrelationChartCreator(section, substances[k].Code, "mg/kg bw/day", "mg/kg", 2.5, 97.5, 400, 300);
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
        public void IndividualConcentrationCorrelationsBySubstanceSection_TestModelVariation() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = FakeSubstancesGenerator.Create(5);
            var individuals = FakeIndividualsGenerator.Create(50, 2, random);
            var monitoringIndividualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals, 1);
            var modelledIndividuals = individuals.SelectMany(r => Enumerable.Repeat(r, 10)).ToList();
            var zeroFractions = new double[] { 0, .5, 1 };
            var targetExposureUnit = TargetUnit.FromInternalDoseUnit(DoseUnit.ugPerL, BiologicalMatrix.Blood);
            var targetHbmUnit = TargetUnit.FromInternalDoseUnit(DoseUnit.ugPerL, BiologicalMatrix.Blood);

            for (int i = 0; i < zeroFractions.Length; i++) {
                var exposureZeroFraction = zeroFractions[i];
                for (int j = 0; j < zeroFractions.Length; j++) {
                    var monitoringZeroFraction = zeroFractions[j];
                    var targetExposures = FakeTargetExposuresGenerator.MockIndividualExposures(modelledIndividuals, substances, random, fractionZeros: exposureZeroFraction);
                    var monitoringExposures = FakeHbmDataGenerator.MockHumanMonitoringIndividualConcentrations(individuals, substances, monitoringZeroFraction, seed: seed + 1);
                    var collection = new List<HbmIndividualCollection>() {
                        new() {
                            TargetUnit = targetHbmUnit,
                            HbmIndividualConcentrations = monitoringExposures
                        }
                    };
                    var section = new IndividualConcentrationCorrelationsBySubstanceSection();
                    section.Summarize(targetExposures, collection, substances, targetExposureUnit, 2.5, 97.5);
                    for (int k = 0; k < substances.Count; k++) {
                        var chartCreator = new IndividualConcentrationCorrelationChartCreator(section, substances[k].Code, "mg/kg bw/day", "mg/kg", 2.5, 97.5, 400, 300);
                        chartCreator.CreateToPng(Path.Combine(_outputPath, $"TestModelVariation_{i}_{j}_{k}.png"));
                    }
                    AssertIsValidView(section);
                }
            }
        }
    }
}
