using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Actions.ExposureMixtures;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualConcentrationCalculation;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualDayConcentrationCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.TargetExposuresCalculators;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using MCRA.Utils.Statistics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Actions {

    /// <summary>
    /// Tests for the exposure mixtures action calculator.
    /// </summary>
    [TestClass]
    public class ExposureMixturesActionCalculatorTests : ActionCalculatorTestsBase {

        /// <summary>
        /// Runs the ExposureMixtures action: run, summarize action result
        /// project.MixtureSelectionSettings.K = 4;
        /// project.MixtureSelectionSettings.NumberOfIterations = 100;
        /// project.MixtureSelectionSettings.SW = .21;
        /// project.MixtureSelectionSettings.Epsilon = 1e-10;
        /// </summary>
        [DataRow(ExposureType.Acute)]
        [DataRow(ExposureType.Chronic)]
        [TestMethod]
        public void ExposureMixturesActionCalculator_TestDietary(ExposureType exposureType) {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var numberOfSubstanses = 6;
            var numberOfIndividuals = 100;
            var substances = MockSubstancesGenerator.Create(numberOfSubstanses);
            var individuals = MockIndividualsGenerator.Create(numberOfIndividuals, 2, random);

            var correctedRelativePotencyFactors = substances.ToDictionary(c => c, c => 1d);
            var membershipProbabilities = substances.ToDictionary(c => c, c => 1d);
            var foodsAsMeasured = MockFoodsGenerator.Create(3);
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var dietaryIndividualDayIntakes = MockDietaryIndividualDayIntakeGenerator.Create(individualDays, foodsAsMeasured, substances, 0, true, random);

            var data = new ActionData() {
                DietaryExposureUnit = TargetUnit.FromExternalExposureUnit(ExternalExposureUnit.ugPerGBWPerDay),
                DietaryIndividualDayIntakes = dietaryIndividualDayIntakes,
                CorrectedRelativePotencyFactors = correctedRelativePotencyFactors,
                MembershipProbabilities = membershipProbabilities,
                ActiveSubstances = substances,
                TargetExposureUnit = TargetUnit.FromInternalDoseUnit(DoseUnit.ugPerKgBWPerDay)
            };
            var project = new ProjectDto();
            project.MixtureSelectionSettings.K = 4;
            project.MixtureSelectionSettings.NumberOfIterations = 100;
            project.MixtureSelectionSettings.SW = .21;
            project.MixtureSelectionSettings.Epsilon = 1e-10;
            project.AssessmentSettings.ExposureType = exposureType;
            project.MixtureSelectionSettings.ClusterMethodType = ClusterMethodType.Hierarchical;

            var calculator = new ExposureMixturesActionCalculator(project);
            var header = TestRunUpdateSummarizeNominal(project, calculator, data, "ExposureMixtures_TestDietary");
        }

        /// <summary>
        /// Runs the ExposureMixtures action: run, summarize action result
        /// project.MixtureSelectionSettings.K = 4;
        /// project.MixtureSelectionSettings.NumberOfIterations = 100;
        /// project.MixtureSelectionSettings.SW = .21;
        /// project.MixtureSelectionSettings.Epsilon = 1e-10;
        /// </summary>
        [DataRow(ExposureType.Acute)]
        [DataRow(ExposureType.Chronic)]
        [TestMethod]
        public void ExposureMixturesActionCalculator_TestAggregate(ExposureType exposureType) {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var numberOfSubstanses = 6;
            var numberOfIndividuals = 100;
            var substances = MockSubstancesGenerator.Create(numberOfSubstanses);
            var individuals = MockIndividualsGenerator.Create(numberOfIndividuals, 2, random);
            var exposureRoutes = new List<ExposurePathType>() { ExposurePathType.Dermal, ExposurePathType.Oral, ExposurePathType.Inhalation };
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(numberOfIndividuals, 2, false, random);
            var rpfs = substances.ToDictionary(r => r, r => 1d);
            var memberships = substances.ToDictionary(r => r, r => 1d);
            var kineticConversionFactors = MockKineticModelsGenerator.CreateAbsorptionFactors(substances, .1);
            var kineticModelCalculators = MockKineticModelsGenerator.CreateAbsorptionFactorKineticModelCalculators(substances, kineticConversionFactors);

            var externalExposuresUnit = ExposureUnitTriple.FromExposureUnit(ExternalExposureUnit.ugPerKgBWPerDay);
            var targetUnit = TargetUnit.FromInternalDoseUnit(DoseUnit.ugPerL, BiologicalMatrix.Liver);
            var aggregateIndividualExposures = FakeAggregateIndividualExposuresGenerator
                .Create(
                    individualDays,
                    substances,
                    exposureRoutes,
                    kineticModelCalculators,
                    externalExposuresUnit,
                    targetUnit,
                    random
                );

            var targetExposuresCalculator = new InternalTargetExposuresCalculator(kineticModelCalculators);
            var aggregateIndividualDayExposures = FakeAggregateIndividualDayExposuresGenerator
                .Create(
                      individualDays,
                      substances,
                      exposureRoutes,
                      targetExposuresCalculator,
                      externalExposuresUnit,
                      targetUnit,
                      random
                  );

            var data = new ActionData() {
                AggregateIndividualExposures = aggregateIndividualExposures,
                AggregateIndividualDayExposures = aggregateIndividualDayExposures,
                CorrectedRelativePotencyFactors = rpfs,
                MembershipProbabilities = memberships,
                ActiveSubstances = substances,
                TargetExposureUnit = targetUnit
            };

            var project = new ProjectDto();
            project.MixtureSelectionSettings.K = 4;
            project.MixtureSelectionSettings.NumberOfIterations = 100;
            project.MixtureSelectionSettings.SW = .21;
            project.MixtureSelectionSettings.Epsilon = 1e-10;
            project.MixtureSelectionSettings.ClusterMethodType = ClusterMethodType.Hierarchical;
            project.AssessmentSettings.ExposureType = exposureType;
            project.AssessmentSettings.ExposureCalculationMethod = ExposureCalculationMethod.ModelledConcentration;
            project.EffectSettings.TargetDoseLevelType = TargetLevelType.Internal;

            var calculator = new ExposureMixturesActionCalculator(project);
            var header = TestRunUpdateSummarizeNominal(project, calculator, data, "ExposureMixtures_TestAggregate");
        }

        /// <summary>
        /// Runs the ExposureMixtures action: run, summarize action result
        /// project.MixtureSelectionSettings.K = 4;
        /// project.MixtureSelectionSettings.NumberOfIterations = 100;
        /// project.MixtureSelectionSettings.SW = .21;
        /// project.MixtureSelectionSettings.Epsilon = 1e-10;
        /// </summary>
        [DataRow(ExposureType.Acute)]
        [DataRow(ExposureType.Chronic)]
        [TestMethod]
        public void ExposureMixturesActionCalculator_TestHBM(ExposureType exposureType) {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var numberOfSubstanses = 6;
            var numberOfIndividuals = 100;
            var substances = MockSubstancesGenerator.Create(numberOfSubstanses);
            var individuals = MockIndividualsGenerator.Create(numberOfIndividuals, 2, random);
            var rpfs = substances.ToDictionary(r => r, r => 1d);
            var memberships = substances.ToDictionary(r => r, r => 1d);
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var samplingMethod = FakeHbmDataGenerator.FakeHumanMonitoringSamplingMethod();
            var monitoringExposures = FakeHbmDataGenerator.MockHumanMonitoringIndividualConcentrations(individuals, substances);
            var monitoringDayConcentrations = FakeHbmDataGenerator.MockHumanMonitoringIndividualDayConcentrations(individualDays, substances, samplingMethod);
            var targetUnit = TargetUnit.FromInternalDoseUnit(DoseUnit.ugPerL, BiologicalMatrix.Blood);
            var data = new ActionData() {
                CorrectedRelativePotencyFactors = rpfs,
                MembershipProbabilities = memberships,
                ActiveSubstances = substances,
                TargetExposureUnit = targetUnit,
                HbmIndividualDayCollections = new List<HbmIndividualDayCollection> {
                    new HbmIndividualDayCollection {
                        TargetUnit = targetUnit,
                        HbmIndividualDayConcentrations = monitoringDayConcentrations
                    }
                },
                HbmIndividualCollections = new List<HbmIndividualCollection> {
                    new HbmIndividualCollection {
                        TargetUnit = targetUnit,
                        HbmIndividualConcentrations = monitoringExposures
                    }
                },
            };
            var project = new ProjectDto();
            project.MixtureSelectionSettings.K = 4;
            project.MixtureSelectionSettings.NumberOfIterations = 100;
            project.MixtureSelectionSettings.SW = .21;
            project.MixtureSelectionSettings.Epsilon = 1e-10;
            project.MixtureSelectionSettings.ClusterMethodType = ClusterMethodType.Hierarchical;
            project.MixtureSelectionSettings.TotalExposureCutOff = 50;
            project.AssessmentSettings.ExposureType = exposureType;
            project.AssessmentSettings.ExposureCalculationMethod = ExposureCalculationMethod.MonitoringConcentration;
            project.EffectSettings.TargetDoseLevelType = TargetLevelType.Internal;

            var calculator = new ExposureMixturesActionCalculator(project);
            var header = TestRunUpdateSummarizeNominal(project, calculator, data, "ExposureMixtures_TestHBM");
        }
    }
}
