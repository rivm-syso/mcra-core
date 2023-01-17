using System.IO;
using System.Linq;
using System.Threading;
using MCRA.General;
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
    public class IndividualConcentrationCorrelationsBySubstanceSectionTests : SectionTestBase {

        private static string _outputPath = TestUtilities.ConcatWithOutputPath("HbmVsModelledIndividualExposures");

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
            var substances = MockSubstancesGenerator.Create(3);
            var individuals = MockIndividualsGenerator.Create(50, 2, random);
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var zeroFractions = new double[] { 0, .5, 1 };
            var hbmTargetUnit = new TargetUnit(
                SubstanceAmountUnit.Micrograms,
                ConcentrationMassUnit.Kilograms,
                null,
                TimeScaleUnit.Peak
            );
            for (int i = 0; i < zeroFractions.Length; i++) {
                var exposureZeroFraction = zeroFractions[i];
                for (int j = 0; j < zeroFractions.Length; j++) {
                    var monitoringZeroFraction = zeroFractions[j];
                    var targetExposures = MockTargetExposuresGenerator.MockIndividualExposures(individuals, substances, random, fractionZeros: exposureZeroFraction);
                    var monitoringExposures = FakeHbmDataGenerator.MockHumanMonitoringIndividualConcentrations(individuals, substances, monitoringZeroFraction, seed: seed + 1);
                    var section = new IndividualConcentrationCorrelationsBySubstanceSection();
                    section.Summarize(targetExposures, monitoringExposures, substances, hbmTargetUnit, hbmTargetUnit, 2.5, 97.5);
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
            var substances = MockSubstancesGenerator.Create(5);
            var individuals = MockIndividualsGenerator.Create(50, 2, random);
            var monitoringIndividualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals, 1);
            var modelledIndividuals = individuals.SelectMany(r => Enumerable.Repeat(r, 10)).ToList();
            var zeroFractions = new double[] { 0, .5, 1 };
            var hbmTargetUnit = new TargetUnit(
                SubstanceAmountUnit.Micrograms,
                ConcentrationMassUnit.Kilograms,
                null,
                TimeScaleUnit.Peak
            );
            for (int i = 0; i < zeroFractions.Length; i++) {
                var exposureZeroFraction = zeroFractions[i];
                for (int j = 0; j < zeroFractions.Length; j++) {
                    var monitoringZeroFraction = zeroFractions[j];
                    var targetExposures = MockTargetExposuresGenerator.MockIndividualExposures(modelledIndividuals, substances, random, fractionZeros: exposureZeroFraction);
                    var monitoringExposures = FakeHbmDataGenerator.MockHumanMonitoringIndividualConcentrations(individuals, substances, monitoringZeroFraction, seed: seed + 1);
                    var section = new IndividualConcentrationCorrelationsBySubstanceSection();
                    section.Summarize(targetExposures, monitoringExposures, substances, hbmTargetUnit, hbmTargetUnit, 2.5, 97.5);
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
