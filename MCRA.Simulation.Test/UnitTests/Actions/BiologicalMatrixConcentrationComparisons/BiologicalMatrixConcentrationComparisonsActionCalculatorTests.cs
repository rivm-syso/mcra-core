using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Actions.BiologicalMatrixConcentrationComparisons;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.TargetExposuresCalculators;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Test.UnitTests.Actions {

    /// <summary>
    /// Runs the HumanMonitoringAnalysis action
    /// </summary>
    [TestClass]
    public class BiologicalMatrixConcentrationComparisonsActionCalculatorTests : ActionCalculatorTestsBase {

        /// <summary>
        /// Runs the BiologicalMatrixConcentrationComparisons action:
        /// project.AssessmentSettings.ExposureType = ExposureType.Acute;
        /// </summary>
        [TestMethod]
        public void BiologicalMatrixConcentrationComparisonsActionCalculator_TestAcute() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = FakeSubstancesGenerator.Create(3);
            var rpfs = substances.ToDictionary(c => c, c => 1d);
            var individuals = FakeIndividualsGenerator.Create(25, 2, random, useSamplingWeights: true);
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var samplingMethod = FakeHbmDataGenerator.FakeHumanMonitoringSamplingMethod();

            var targetUnit = TargetUnit.FromInternalDoseUnit(DoseUnit.ugPerL, BiologicalMatrix.Blood);
            var hbmIndividualDayCollection = FakeHbmIndividualDayConcentrationsGenerator
                .Create(individualDays, substances, samplingMethod, targetUnit, random);
            var hbmIndividualDayCumulativeConcentrations = FakeHbmCumulativeIndividualDayConcentrationsGenerator
                .Create(individualDays, random);

            var kineticConversionFactors = FakeKineticModelsGenerator.CreateAbsorptionFactors(substances, 1);
            var kineticModelCalculators = FakeKineticModelsGenerator.CreateAbsorptionFactorKineticModelCalculators(
                substances,
                kineticConversionFactors,
                targetUnit
            );
            var targetExposuresCalculator = new InternalTargetExposuresCalculator(kineticModelCalculators);

            var externalExposuresUnit = ExposureUnitTriple.FromExposureUnit(ExternalExposureUnit.ugPerKgBWPerDay);
            var individualDayTargetExposures = FakeAggregateIndividualDayExposuresGenerator
                .Create(
                    individualDays,
                    substances,
                    [ExposureRoute.Oral],
                    kineticModelCalculators,
                    externalExposuresUnit,
                    targetUnit,
                    random
                );

            var project = new ProjectDto();
            var config = project.BiologicalMatrixConcentrationComparisonsSettings;
            config.ExposureType = ExposureType.Acute;

            var data = new ActionData() {
                ActiveSubstances = substances,
                CorrectedRelativePotencyFactors = rpfs,
                ReferenceSubstance = substances.First(),
                MembershipProbabilities = substances.ToDictionary(c => c, c => 1d),
                HbmSamplingMethods = [samplingMethod],
                HbmIndividualDayCollections = [hbmIndividualDayCollection],
                HbmCumulativeIndividualDayCollection = hbmIndividualDayCumulativeConcentrations,
                AggregateIndividualDayExposures = individualDayTargetExposures,
                TargetExposureUnit = targetUnit,
            };

            var calculator = new BiologicalMatrixConcentrationComparisonsActionCalculator(project);
            var header = TestRunUpdateSummarizeNominal(project, calculator, data, "TestAcute");
            var result = calculator.Run(data, new CompositeProgressState());
        }

        /// <summary>
        /// Runs the BiologicalMatrixConcentrationComparisons action:
        /// project.AssessmentSettings.ExposureType = ExposureType.Chronic;
        /// </summary>
        [TestMethod]
        public void BiologicalMatrixConcentrationComparisonsActionCalculator_TestChronic() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var individuals = FakeIndividualsGenerator.CreateSimulated(25, 2, random, useSamplingWeights: true);
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var substances = FakeSubstancesGenerator.Create(3);
            var targetUnit = TargetUnit.FromInternalDoseUnit(DoseUnit.ugPerL, BiologicalMatrix.Blood);

            var samplingMethod = FakeHbmDataGenerator.FakeHumanMonitoringSamplingMethod();
            var hbmCumulativeIndividualConcentrations = FakeHbmCumulativeIndividualConcentrationsGenerator
                .Create(individuals, random);
            var hbmIndividualConcentrations = FakeHbmIndividualConcentrationsGenerator
                .Create(individuals, substances, samplingMethod, targetUnit, random);

            var individualTargetExposures = FakeAggregateIndividualExposuresGenerator
                .Create(individualDays, substances, targetUnit, random);

            var project = new ProjectDto();
            var config = project.BiologicalMatrixConcentrationComparisonsSettings;
            config.ExposureType = ExposureType.Chronic;

            var data = new ActionData() {
                ActiveSubstances = substances,
                ReferenceSubstance = substances.First(),
                CorrectedRelativePotencyFactors = substances.ToDictionary(c => c, c => 1d),
                HbmIndividualCollections = hbmIndividualConcentrations,
                HbmCumulativeIndividualCollection = hbmCumulativeIndividualConcentrations,
                HbmSamplingMethods = [samplingMethod],
                AggregateIndividualExposures = individualTargetExposures,
                MembershipProbabilities = substances.ToDictionary(c => c, c => 1d),
                TargetExposureUnit = targetUnit,
            };

            var calculator = new BiologicalMatrixConcentrationComparisonsActionCalculator(project);
            var header = TestRunUpdateSummarizeNominal(project, calculator, data, "TestChronic");
            var result = calculator.Run(data, new CompositeProgressState());
        }
    }
}
