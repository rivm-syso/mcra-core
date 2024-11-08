using MCRA.General;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using MCRA.Utils.Statistics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualDayConcentrationCalculation;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.HumanMonitoringAnalysis {

    /// <summary>
    /// OutputGeneration, ActionSummaries, HumanMonitoringAnalysis
    /// </summary>
    [TestClass]
    public class HbmRiskDriversRecordTests : SectionTestBase {

        /// <summary>
        /// Test HbmRiskDriversSection view
        /// </summary>
        [TestMethod]
        public void HHbmRiskDriversSection_Test1() {
            var section = new HbmTotalDistributionRiskDriversSection {
                Records = [],
            };
            AssertIsValidView(section);
        }

        /// <summary>
        /// Test HbmRiskDriversSection view
        /// </summary>
        [TestMethod]
        public void HHbmRiskDriversSection_Test2() {
            var section = new HbmUpperDistributionRiskDriversSection {
                Records = [],
            };
            AssertIsValidView(section);
        }

        /// <summary>
        /// Calculates riskdrivers:
        /// project.AssessmentSettings.ExposureType = ExposureType.Acute;
        /// </summary>
        [TestMethod]
        public void HbmTotalRiskDriversSection_TestAcute() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = MockSubstancesGenerator.Create(3);
            var rpfs = substances.ToDictionary(c => c, c => 1d);
            var individuals = FakeIndividualsGenerator.Create(25, 2, random, useSamplingWeights: true);
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var samplingMethod = FakeHbmDataGenerator.FakeHumanMonitoringSamplingMethod();
            var targetUnit = new TargetUnit(
                new ExposureTarget(BiologicalMatrix.Blood),
                new ExposureUnitTriple(
                    SubstanceAmountUnit.Micrograms,
                    ConcentrationMassUnit.Kilograms,
                    TimeScaleUnit.Peak
                )
            );
            var hbmIndividualDayConcentrations = new List<HbmIndividualDayCollection>{ 
                FakeHbmIndividualDayConcentrationsGenerator
                .Create(individualDays, substances, samplingMethod, targetUnit, random) };
            var hbmCumulativeIndividualDayCalculator = new HbmCumulativeIndividualDayConcentrationCalculator();
            var hbmIndividualDayCumulativeConcentrations = hbmCumulativeIndividualDayCalculator.Calculate(
                hbmIndividualDayConcentrations,
                substances,
                rpfs
            );

            var section = new HbmTotalDistributionRiskDriversSection();
            section.Summarize(
                hbmIndividualDayConcentrations,
                null,
                hbmIndividualDayCumulativeConcentrations,
                null,
                substances,
                rpfs,
                ExposureType.Acute
            );
            
            for (int i = 0; i < 10; i++) {
                var hbmIndividualDayConcentrationsUnc = new List<HbmIndividualDayCollection>{
                    FakeHbmIndividualDayConcentrationsGenerator
                    .Create(individualDays, substances, samplingMethod, targetUnit, random) };
                var hbmIndividualDayCumulativeConcentrationUnc = hbmCumulativeIndividualDayCalculator.Calculate(
                    hbmIndividualDayConcentrations,
                    substances,
                    rpfs
                );
                section.SummarizeUncertainty(
                    hbmIndividualDayConcentrationsUnc,
                    null,
                    hbmIndividualDayCumulativeConcentrationUnc,
                    null,
                    substances,
                    rpfs,
                    ExposureType.Acute,
                    2.5,
                    95
                );
            }
            Assert.AreEqual(100, section.Records.Sum(c => c.Contribution), 0.001);
        }

        /// <summary>
        /// Calculates riskdrivers:
        /// project.AssessmentSettings.ExposureType = ExposureType.Acute;
        /// </summary>
        [TestMethod]
        public void HbmUpperRiskDriversSection_TestAcute() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = MockSubstancesGenerator.Create(3);
            var rpfs = substances.ToDictionary(c => c, c => 1d);
            var individuals = FakeIndividualsGenerator.Create(25, 2, random, useSamplingWeights: true);
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var samplingMethod = FakeHbmDataGenerator.FakeHumanMonitoringSamplingMethod();
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
                .Create(individualDays, substances, samplingMethod, targetUnit, random) };
            var hbmCumulativeIndividualDayCalculator = new HbmCumulativeIndividualDayConcentrationCalculator();
            var hbmIndividualDayCumulativeConcentrations = hbmCumulativeIndividualDayCalculator.Calculate(
                hbmIndividualDayConcentrations,
                substances,
                rpfs
            );

            var section = new HbmUpperDistributionRiskDriversSection();
            section.Summarize(
                hbmIndividualDayConcentrations,
                null,
                hbmIndividualDayCumulativeConcentrations,
                null,
                substances,
                rpfs,
                ExposureType.Acute,
                95
            );
            for (int i = 0; i < 10; i++) {
                var hbmIndividualDayConcentrationsUnc = FakeHbmIndividualDayConcentrationsGenerator
                    .Create(individualDays, substances, samplingMethod, targetUnit, random);
                var hbmIndividualDayCumulativeConcentrationUnc = hbmCumulativeIndividualDayCalculator.Calculate(
                    hbmIndividualDayConcentrations,
                    substances,
                    rpfs
                );
                section.SummarizeUncertainty(
                    hbmIndividualDayConcentrations,
                    null,
                    hbmIndividualDayCumulativeConcentrations,
                    null,
                    substances,
                    rpfs,
                    ExposureType.Acute,
                    95,
                    2.5,
                    95
                );
            }
            Assert.AreEqual(100, section.Records.Sum(c => c.Contribution), 0.001);
        }

        /// <summary>
        /// Calculates riskdrivers:
        /// project.AssessmentSettings.ExposureType = ExposureType.Chronic;
        /// </summary>
        [TestMethod]
        public void HbmTotalRiskDriversSection_TestChronic() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = MockSubstancesGenerator.Create(3);
            var rpfs = substances.ToDictionary(c => c, c => 1d);
            var individuals = FakeIndividualsGenerator.Create(25, 2, random, useSamplingWeights: true);
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var samplingMethod = FakeHbmDataGenerator.FakeHumanMonitoringSamplingMethod();
            var targetUnit = new TargetUnit(
                new ExposureTarget(BiologicalMatrix.Blood),
                new ExposureUnitTriple(
                    SubstanceAmountUnit.Micrograms,
                    ConcentrationMassUnit.Kilograms,
                    TimeScaleUnit.Peak
                )
            );
            var hbmIndividualConcentrations = FakeHbmIndividualConcentrationsGenerator
                .Create(individuals, substances, samplingMethod, targetUnit, random);
            var hbmCumulativeIndividualCalculator = new HbmCumulativeIndividualConcentrationCalculator();
            var hbmIndividualCumulativeConcentrations = hbmCumulativeIndividualCalculator.Calculate(
                hbmIndividualConcentrations,
                substances,
                rpfs
            );

            var section = new HbmTotalDistributionRiskDriversSection();
            section.Summarize(
                null,
                hbmIndividualConcentrations,
                null,
                hbmIndividualCumulativeConcentrations,
                substances,
                rpfs,
                ExposureType.Chronic
            );

            for (int i = 0; i < 10; i++) {
                var hbmIndividualConcentrationsUnc = FakeHbmIndividualConcentrationsGenerator
                    .Create(individuals, substances, samplingMethod, targetUnit, random);
                var hbmIndividualCumulativeConcentrationUnc = hbmCumulativeIndividualCalculator.Calculate(
                    hbmIndividualConcentrations,
                    substances,
                    rpfs
                );
                section.SummarizeUncertainty(
                    null,
                    hbmIndividualConcentrationsUnc,
                    null,
                    hbmIndividualCumulativeConcentrationUnc,
                    substances,
                    rpfs,
                    ExposureType.Chronic,
                    2.5,
                    95
                );
            }
            Assert.AreEqual(100, section.Records.Sum(c => c.Contribution), 0.001);
        }

        /// <summary>
        /// Calculates riskdrivers:
        /// project.AssessmentSettings.ExposureType = ExposureType.Chronic;
        /// </summary>
        [TestMethod]
        public void HbmUpperRiskDriversSection_TestChronic() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = MockSubstancesGenerator.Create(3);
            var rpfs = substances.ToDictionary(c => c, c => 1d);
            var individuals = FakeIndividualsGenerator.Create(25, 2, random, useSamplingWeights: true);
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var samplingMethod = FakeHbmDataGenerator.FakeHumanMonitoringSamplingMethod();
            var targetUnit = new TargetUnit(
                new ExposureTarget(BiologicalMatrix.Blood),
                new ExposureUnitTriple(
                    SubstanceAmountUnit.Micrograms,
                    ConcentrationMassUnit.Kilograms,
                    TimeScaleUnit.Peak
                )
            );
            var hbmIndividualConcentrations = FakeHbmIndividualConcentrationsGenerator
                .Create(individuals, substances, samplingMethod, targetUnit, random);
            var hbmCumulativeIndividualCalculator = new HbmCumulativeIndividualConcentrationCalculator();
            var hbmIndividualCumulativeConcentrations = hbmCumulativeIndividualCalculator.Calculate(
                hbmIndividualConcentrations,
                substances,
                rpfs
            );

            var section = new HbmUpperDistributionRiskDriversSection();
            section.Summarize(
                null,
                hbmIndividualConcentrations,
                null,
                hbmIndividualCumulativeConcentrations,
                substances,
                rpfs,
                ExposureType.Chronic, 
                90
            );

            for (int i = 0; i < 10; i++) {
                var hbmIndividualConcentrationsUnc = FakeHbmIndividualConcentrationsGenerator
                    .Create(individuals, substances, samplingMethod, targetUnit, random);
                var hbmIndividualCumulativeConcentrationUnc = hbmCumulativeIndividualCalculator.Calculate(
                    hbmIndividualConcentrations,
                    substances,
                    rpfs
                );
                section.SummarizeUncertainty(
                    null,
                    hbmIndividualConcentrationsUnc,
                    null,
                    hbmIndividualCumulativeConcentrationUnc,
                    substances,
                    rpfs,
                    ExposureType.Chronic,
                    90,
                    2.5,
                    95
                );
            }

            Assert.AreEqual(100, section.Records.Sum(c => c.Contribution), 0.001);
        }
    }
}