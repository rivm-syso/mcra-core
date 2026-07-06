using MCRA.General;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualDayConcentrationCalculation;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.HumanMonitoringAnalysis {

    /// <summary>
    /// OutputGeneration, ActionSummaries, HumanMonitoringAnalysis
    /// </summary>
    [TestClass]
    public class HbmContributionsBySubstanceTotalSectionTests : SectionTestBase {

        /// <summary>
        /// Test HbmRiskDriversSection view
        /// </summary>
        [TestMethod]
        public void HbmContributionsBySubstanceTotalSection_Test1() {
            var section = new HbmContributionsBySubstanceTotalSection {
                Records = [],
            };
            AssertIsValidView(section);
        }

        /// <summary>
        /// Calculates riskdrivers:
        /// project.AssessmentSettings.ExposureType = ExposureType.Acute;
        /// </summary>
        [TestMethod]
        public void HbmContributionsBySubstanceTotalSection_TestAcute() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = FakeSubstancesGenerator.Create(3);
            var rpfs = substances.ToDictionary(c => c, c => 1d);
            var memberships = substances.ToDictionary(c => c, c => 1d);
            var individuals = FakeIndividualsGenerator.Create(25, 2, random, useSamplingWeights: false);
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
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

            var section = new HbmContributionsBySubstanceTotalSection();
            section.Summarize(
                null,
                hbmIndividualDayConcentrations,
                null,
                hbmIndividualDayCumulativeConcentrations,
                null,
                substances,
                rpfs,
                memberships,
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
                    null,
                    hbmIndividualDayConcentrationsUnc,
                    null,
                    hbmIndividualDayCumulativeConcentrationUnc,
                    null,
                    substances,
                    rpfs,
                    memberships,
                    ExposureType.Acute,
                    2.5,
                    95,
                    i
                );
            }
            Assert.AreEqual(100, section.Records.Sum(c => c.Contribution), 0.001);
        }

        /// <summary>
        /// Calculates riskdrivers:
        /// project.AssessmentSettings.ExposureType = ExposureType.Chronic;
        /// </summary>
        [TestMethod]
        public void HbmContributionsBySubstanceTotalSection_TestChronic() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = FakeSubstancesGenerator.Create(3);
            var rpfs = substances.ToDictionary(c => c, c => 1d);
            var memberships = substances.ToDictionary(c => c, c => 1d);
            var individuals = FakeIndividualsGenerator.CreateSimulated(25, 2, random, useSamplingWeights: false);
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
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

            var section = new HbmContributionsBySubstanceTotalSection();
            section.Summarize(
                null,
                null,
                hbmIndividualConcentrations,
                null,
                hbmIndividualCumulativeConcentrations,
                substances,
                rpfs,
                memberships,
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
                    null,
                    hbmIndividualConcentrationsUnc,
                    null,
                    hbmIndividualCumulativeConcentrationUnc,
                    substances,
                    rpfs,
                    memberships,
                    ExposureType.Chronic,
                    2.5,
                    95, 
                    i
                );
            }
            Assert.AreEqual(100, section.Records.Sum(c => c.Contribution), 0.001);
        }
    }
}