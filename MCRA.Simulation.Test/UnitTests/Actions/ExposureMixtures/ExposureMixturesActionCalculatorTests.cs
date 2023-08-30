using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Actions.ExposureMixtures;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualConcentrationCalculation;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualDayConcentrationCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.TargetExposuresCalculators;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using MCRA.Simulation.Units;
using MCRA.Utils.Statistics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Actions {
    /// <summary>
    /// Runs the ExposureMixtures action
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

            var dietaryExposureUnit = TargetUnit.CreateDietaryExposureUnit(ConsumptionUnit.g, ConcentrationUnit.mgPerKg, BodyWeightUnit.kg, false);
            dietaryExposureUnit.BiologicalMatrix = BiologicalMatrix.WholeBody;
            var hbmConcentrationUnits = new TargetUnitsModel();
            hbmConcentrationUnits.SubstanceTargetUnits.Add(dietaryExposureUnit, new HashSet<Compound>());

            var data = new ActionData() {
                DietaryExposureUnit = dietaryExposureUnit,
                DietaryIndividualDayIntakes = dietaryIndividualDayIntakes,
                CorrectedRelativePotencyFactors = correctedRelativePotencyFactors,
                MembershipProbabilities = membershipProbabilities,
                ActiveSubstances = substances,
                HbmTargetConcentrationUnits = hbmConcentrationUnits,
            };
            var project = new ProjectDto();
            project.MixtureSelectionSettings.K = 4;
            project.MixtureSelectionSettings.NumberOfIterations = 100;
            project.MixtureSelectionSettings.SW = .21;
            project.MixtureSelectionSettings.Epsilon = 1e-10;
            project.AssessmentSettings.ExposureType = exposureType;
            project.MixtureSelectionSettings.ClusterMethodType = ClusterMethodType.Hierarchical;

            var calculator = new ExposureMixturesActionCalculator(project);
            var header = TestRunUpdateSummarizeNominal(project, calculator, data, "ExposureMixtures");
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
            var allRoutes = new List<ExposureRouteType>() { ExposureRouteType.Dietary, ExposureRouteType.Dermal, ExposureRouteType.Oral, ExposureRouteType.Inhalation };
            var exposureRoutes = allRoutes.Where(r => random.NextDouble() > .5).ToList();
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(numberOfIndividuals, 2, false, random);
            var rpfs = substances.ToDictionary(r => r, r => 1d);
            var memberships = substances.ToDictionary(r => r, r => 1d);
            var absorptionFactors = MockKineticModelsGenerator.CreateAbsorptionFactors(substances, .1);
            var kineticModelCalculators = MockKineticModelsGenerator.CreateAbsorptionFactorKineticModelCalculators(substances, absorptionFactors);
            var targetUnit = new TargetUnit(ExposureUnit.ugPerKgBWPerDay);
            var aggregateIndividualExposures = MockAggregateIndividualIntakeGenerator.Create(
                individualDays,
                substances,
                exposureRoutes,
                kineticModelCalculators,
                targetUnit,
                random
            );

            var targetExposuresCalculator = new InternalTargetExposuresCalculator(kineticModelCalculators);
            var aggregateIndividualDayExposures = MockAggregateIndividualDayIntakeGenerator.Create(
                      individualDays,
                      substances,
                      exposureRoutes,
                      targetExposuresCalculator,
                      new TargetUnit(ExposureUnit.ugPerKgBWPerDay),
                      random
                  );

            var dietaryExposureUnit = TargetUnit.CreateDietaryExposureUnit(ConsumptionUnit.g, ConcentrationUnit.mgPerKg, BodyWeightUnit.kg, false);
            dietaryExposureUnit.BiologicalMatrix = BiologicalMatrix.WholeBody;

            var hbmConcentrationUnits = new TargetUnitsModel();
            hbmConcentrationUnits.SubstanceTargetUnits.Add(dietaryExposureUnit, new HashSet<Compound>());

            var data = new ActionData() {
                DietaryExposureUnit = dietaryExposureUnit,
                AggregateIndividualExposures = aggregateIndividualExposures,
                AggregateIndividualDayExposures = aggregateIndividualDayExposures,
                CorrectedRelativePotencyFactors = rpfs,
                MembershipProbabilities = memberships,
                ActiveSubstances = substances,
                HbmTargetConcentrationUnits = hbmConcentrationUnits,
                HazardCharacterisationsUnit = new TargetUnit(ExposureUnit.ugPerKgBWPerDay),
                TargetExposureUnit = new TargetUnit(ExposureUnit.mgPerGBWPerDay)
            };
            var project = new ProjectDto();
            project.MixtureSelectionSettings.K = 4;
            project.MixtureSelectionSettings.NumberOfIterations = 100;
            project.MixtureSelectionSettings.SW = .21;
            project.MixtureSelectionSettings.Epsilon = 1e-10;
            project.MixtureSelectionSettings.ClusterMethodType = ClusterMethodType.Hierarchical;
            project.AssessmentSettings.ExposureType = exposureType;
            project.AssessmentSettings.InternalConcentrationType = InternalConcentrationType.ModelledConcentration;
            project.EffectSettings.TargetDoseLevelType = TargetLevelType.Internal;
            var calculator = new ExposureMixturesActionCalculator(project);
            var header = TestRunUpdateSummarizeNominal(project, calculator, data, "ExposureMixtures");
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

            var hbmConcentrationUnits = new TargetUnitsModel();
            hbmConcentrationUnits.SubstanceTargetUnits.Add(new TargetUnit(ExposureUnit.ugPerKgBWPerDay), new HashSet<Compound>());


            var data = new ActionData() {
                CorrectedRelativePotencyFactors = rpfs,
                MembershipProbabilities = memberships,
                ActiveSubstances = substances,
                HazardCharacterisationsUnit = new TargetUnit(ExposureUnit.ugPerKgBWPerDay),
                HbmIndividualDayCollections = new List<HbmIndividualDayCollection> { new HbmIndividualDayCollection {
                    HbmIndividualDayConcentrations = monitoringDayConcentrations
                    }
                },
                HbmIndividualCollections = new List<HbmIndividualCollection> { new HbmIndividualCollection {
                    HbmIndividualConcentrations = monitoringExposures
                    }
                },
                HbmTargetConcentrationUnits = hbmConcentrationUnits
            };
            var project = new ProjectDto();
            project.MixtureSelectionSettings.K = 4;
            project.MixtureSelectionSettings.NumberOfIterations = 100;
            project.MixtureSelectionSettings.SW = .21;
            project.MixtureSelectionSettings.Epsilon = 1e-10;
            project.MixtureSelectionSettings.ClusterMethodType = ClusterMethodType.Hierarchical;
            project.AssessmentSettings.ExposureType = exposureType;
            project.AssessmentSettings.InternalConcentrationType = InternalConcentrationType.MonitoringConcentration;
            project.EffectSettings.TargetDoseLevelType = TargetLevelType.Internal;
            project.HumanMonitoringSettings.SamplingMethodCodes = new List<string> { "Liver_Pooled" };
            var calculator = new ExposureMixturesActionCalculator(project);
            var header = TestRunUpdateSummarizeNominal(project, calculator, data, "ExposureMixtures");
        }
    }
}




