using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Action.UncertaintyFactorial;
using MCRA.Simulation.Actions.TargetExposures;
using MCRA.Simulation.Calculators.IntakeModelling;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using MCRA.Utils.Statistics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Actions {

    /// <summary>
    /// Runs the TargetExposures action
    /// </summary>
    [TestClass]
    public class TargetExposuresActionCalculatorTests : ActionCalculatorTestsBase {

        /// <summary>
        /// Runs the TargetExposures action: run, update simulation data, summarize action result, 
        /// run uncertain, update simulation data uncertain, summarize action result uncertain method
        /// Acute, target dose level is internal, no RPFs are provided
        /// </summary>
        [TestMethod]
        public void TargetExposuresActionCalculator_TestAcuteInternalSingleSubstanceNoRpfs() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = MockFoodsGenerator.Create(3);
            var substances = MockSubstancesGenerator.Create(1);
            var individuals = MockIndividualsGenerator.Create(25, 2, random, useSamplingWeights: true);
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var dietaryIndividualDayIntakes = MockDietaryIndividualDayIntakeGenerator.Create(individualDays, foods, substances, 0.5, false, random, false);
            var exposureRoutes = new List<ExposureRouteType>() { ExposureRouteType.Dietary };
            var absorptionFactors = MockAbsorptionFactorsGenerator.Create(exposureRoutes, substances);
            var data = new ActionData() {
                ActiveSubstances = substances,
                DietaryIndividualDayIntakes = dietaryIndividualDayIntakes,
                ExternalExposureUnit = ExposureUnitTriple.FromExposureUnit(ExternalExposureUnit.ugPerKgBWPerDay),
                DietaryExposureUnit = TargetUnit.FromExternalExposureUnit(ExternalExposureUnit.ugPerKgBWPerDay),
                SelectedPopulation = new Population { NominalBodyWeight = 70 },
                AbsorptionFactors = absorptionFactors
            };

            var project = new ProjectDto();
            project.EffectSettings.TargetDoseLevelType = TargetLevelType.Internal;
            project.KineticModelSettings.InternalModelType = InternalModelType.AbsorptionFactorModel;

            var calculatorNom = new TargetExposuresActionCalculator(project);
            _ = TestRunUpdateSummarizeNominal(project, calculatorNom, data, "TestAcuteInternalSingleSubstanceNoRpfs");

            var calculator = new TargetExposuresActionCalculator(project);
            var (header, _) = TestRunUpdateSummarizeNominal(project, calculator, data, null);

            var factorialSet = new UncertaintyFactorialSet(UncertaintySource.Individuals);
            var uncertaintySourceGenerators = new Dictionary<UncertaintySource, IRandom> {
                [UncertaintySource.Individuals] = random
            };
            TestRunUpdateSummarizeUncertainty(calculator, data, header, random, factorialSet, uncertaintySourceGenerators, reportFileName: "TestAcuteSingleSubstanceNoRpfsUnc");
        }

        /// <summary>
        /// Runs the TargetExposures action: run, update simulation data, summarize action result, 
        /// run uncertain, update simulation data uncertain, summarize action result uncertain method
        /// Acute, TargetDoseLevelType = TargetDoseLevelType.Internal
        /// </summary>
        [TestMethod]
        public void TargetExposuresActionCalculator_TestChronicInternal() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = MockSubstancesGenerator.Create(5);
            var correctedRelativePotencyFactors = substances.ToDictionary(c => c, c => 1d);
            var membershipProbabilities = substances.ToDictionary(c => c, c => 1d);
            var individuals = MockIndividualsGenerator.Create(25, 2, random, useSamplingWeights: true);
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var foodsAsMeasured = MockFoodsGenerator.Create(3);
            var dietaryIndividualDayIntakes = MockDietaryIndividualDayIntakeGenerator.Create(individualDays, foodsAsMeasured, substances, 0, true, random);
            var dietaryExposureUnit = TargetUnit.FromExternalExposureUnit(ExternalExposureUnit.ugPerKgBWPerDay);
            var exposureRoutes = new List<ExposureRouteType>() { ExposureRouteType.Dietary, ExposureRouteType.Dermal, ExposureRouteType.Oral, ExposureRouteType.Inhalation };
            var absorptionFactors = MockAbsorptionFactorsGenerator.Create(exposureRoutes, substances);
            var referenceCompound = substances.First();

            var data = new ActionData() {
                ActiveSubstances = substances,
                CorrectedRelativePotencyFactors = correctedRelativePotencyFactors,
                MembershipProbabilities = membershipProbabilities,
                DietaryIndividualDayIntakes = dietaryIndividualDayIntakes,
                DietaryExposureUnit = dietaryExposureUnit,
                AbsorptionFactors = absorptionFactors,
                ReferenceSubstance = referenceCompound,
                SelectedPopulation = new Population { NominalBodyWeight = 70 }
            };

            var project = new ProjectDto();
            project.AssessmentSettings.Cumulative = true;
            project.AssessmentSettings.ExposureType = ExposureType.Chronic;
            project.IntakeModelSettings.IntakeModelType = IntakeModelType.OIM;
            project.EffectSettings.TargetDoseLevelType = TargetLevelType.Internal;
            project.KineticModelSettings.InternalModelType = InternalModelType.AbsorptionFactorModel;
            var calculatorNom = new TargetExposuresActionCalculator(project);
            _ = TestRunUpdateSummarizeNominal(project, calculatorNom, data, "TestChronicInternalOIM");

            var calculator = new TargetExposuresActionCalculator(project);
            var (header, _) = TestRunUpdateSummarizeNominal(project, calculator, data, null);

            var factorialSet = new UncertaintyFactorialSet(UncertaintySource.Individuals);
            var uncertaintySourceGenerators = new Dictionary<UncertaintySource, IRandom> {
                [UncertaintySource.Individuals] = random
            };
            TestRunUpdateSummarizeUncertainty(calculator, data, header, random, factorialSet, uncertaintySourceGenerators, reportFileName: "TestChronicInternalOIMUnc");
        }

        /// <summary>
        /// Runs the TargetExposures action: run, update simulation data, summarize action result, 
        /// run uncertain, update simulation data uncertain, summarize action result uncertain method
        /// Chronic, TargetDoseLevelType = TargetDoseLevelType.External, LNN
        /// Including nondietary exposures (aggregate)
        /// </summary>
        [TestMethod]
        public void TargetExposuresActionCalculator_TestChronicInternalAggregate() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = MockSubstancesGenerator.Create(5);
            var correctedRelativePotencyFactors = substances.ToDictionary(c => c, c => 1d);
            var membershipProbabilities = substances.ToDictionary(c => c, c => 1d);
            var individuals = MockIndividualsGenerator.Create(200, 2, random, useSamplingWeights: true);
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var foodsAsMeasured = MockFoodsGenerator.Create(3);
            var dietaryIndividualDayIntakes = MockDietaryIndividualDayIntakeGenerator.Create(individualDays, foodsAsMeasured, substances, 0, true, random);
            var dietaryExposureUnit = TargetUnit.FromExternalExposureUnit(ExternalExposureUnit.ugPerKgBWPerDay);
            var intakes = dietaryIndividualDayIntakes.Select(c => c.TotalExposurePerMassUnit(correctedRelativePotencyFactors, membershipProbabilities, false)).ToList();
            var dietaryModelBasedIntakeResults = new List<ModelBasedIntakeResult> { new ModelBasedIntakeResult() { ModelBasedIntakes = intakes, CovariateGroup = new CovariateGroup() } };
            var referenceCompound = substances.First();
            var exposureRoutes = new List<ExposureRouteType>() { ExposureRouteType.Dietary, ExposureRouteType.Dermal, ExposureRouteType.Oral, ExposureRouteType.Inhalation };
            var absorptionFactors = MockAbsorptionFactorsGenerator.Create(exposureRoutes, substances);
            var nonDietaryExposureRoutes = new List<ExposureRouteType>() { ExposureRouteType.Dermal, ExposureRouteType.Oral, ExposureRouteType.Inhalation };
            var routes = new HashSet<ExposureRouteType>() { ExposureRouteType.Dermal, ExposureRouteType.Oral, ExposureRouteType.Inhalation };
            var nonDietaryExposures = MockNonDietaryExposureSetsGenerator.MockNonDietarySurveys(individuals, substances, routes, random, ExternalExposureUnit.ugPerKgBWPerDay, 1, true);

            var data = new ActionData() {
                ActiveSubstances = substances,
                CorrectedRelativePotencyFactors = correctedRelativePotencyFactors,
                MembershipProbabilities = membershipProbabilities,
                DietaryIndividualDayIntakes = dietaryIndividualDayIntakes,
                DietaryExposureUnit = dietaryExposureUnit,
                ReferenceSubstance = referenceCompound,
                DietaryModelBasedIntakeResults = dietaryModelBasedIntakeResults,
                AbsorptionFactors = absorptionFactors,
                NonDietaryExposureRoutes = nonDietaryExposureRoutes,
                NonDietaryExposures = nonDietaryExposures,
                SelectedPopulation = new Population { NominalBodyWeight = 70 }
            };

            var project = new ProjectDto();
            project.AssessmentSettings.Cumulative = true;
            project.AssessmentSettings.ExposureType = ExposureType.Chronic;
            project.IntakeModelSettings.IntakeModelType = IntakeModelType.LNN0;
            project.EffectSettings.TargetDoseLevelType = TargetLevelType.Internal;
            project.AssessmentSettings.Aggregate = true;
            project.OutputDetailSettings.IsDetailedOutput = true;
            project.OutputDetailSettings.StoreIndividualDayIntakes = true;
            project.KineticModelSettings.InternalModelType = InternalModelType.AbsorptionFactorModel;
            var calculatorNom = new TargetExposuresActionCalculator(project);
            _ = TestRunUpdateSummarizeNominal(project, calculatorNom, data, "TestChronicInternalAggregateNom");

            var calculator = new TargetExposuresActionCalculator(project);
            var (header, _) = TestRunUpdateSummarizeNominal(project, calculator, data, null);

            var factorialSet = new UncertaintyFactorialSet(UncertaintySource.Individuals);
            var uncertaintySourceGenerators = new Dictionary<UncertaintySource, IRandom> {
                [UncertaintySource.Individuals] = random
            };
            TestRunUpdateSummarizeUncertainty(calculator, data, header, random, factorialSet, uncertaintySourceGenerators, reportFileName: "TestChronicInternalAggregateUnc");
        }

        /// <summary>
        /// Runs the TargetExposures action: run, update simulation data, summarize action result, 
        /// run uncertain, update simulation data uncertain, summarize action result uncertain method
        /// Acute, TargetDoseLevelType = TargetDoseLevelType.External, LNN
        /// Including nondietary exposures (aggregate)
        /// </summary>
        [TestMethod]
        public void TargetExposuresActionCalculator_TestAcuteInternalAggregate() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);

            var substances = MockSubstancesGenerator.Create(5);
            var correctedRelativePotencyFactors = substances.ToDictionary(c => c, c => 1d);
            var membershipProbabilities = substances.ToDictionary(c => c, c => 1d);
            var referenceCompound = substances.First();

            var individuals = MockIndividualsGenerator.Create(200, 2, random, useSamplingWeights: true);
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var foodsAsMeasured = MockFoodsGenerator.Create(3);
            var dietaryIndividualDayIntakes = MockDietaryIndividualDayIntakeGenerator.Create(individualDays, foodsAsMeasured, substances, 0, true, random);
            var dietaryExposureUnit = TargetUnit.FromExternalExposureUnit(ExternalExposureUnit.ugPerKgBWPerDay);

            var exposureRoutes = new List<ExposureRouteType>() { ExposureRouteType.Dietary, ExposureRouteType.Dermal, ExposureRouteType.Oral, ExposureRouteType.Inhalation };
            var nonDietaryExposureRoutes = new List<ExposureRouteType>() { ExposureRouteType.Dermal, ExposureRouteType.Oral, ExposureRouteType.Inhalation };
            var routes = new HashSet<ExposureRouteType>() { ExposureRouteType.Dermal, ExposureRouteType.Oral, ExposureRouteType.Inhalation };
            var nonDietaryExposures = MockNonDietaryExposureSetsGenerator.MockNonDietarySurveys(individuals, substances, routes, random, ExternalExposureUnit.ugPerKgBWPerDay, 1, true);

            var absorptionFactors = MockAbsorptionFactorsGenerator.Create(exposureRoutes, substances);

            var data = new ActionData() {
                ActiveSubstances = substances,
                CorrectedRelativePotencyFactors = correctedRelativePotencyFactors,
                MembershipProbabilities = membershipProbabilities,
                DietaryIndividualDayIntakes = dietaryIndividualDayIntakes,
                DietaryExposureUnit = dietaryExposureUnit,
                ReferenceSubstance = referenceCompound,
                AbsorptionFactors = absorptionFactors,
                NonDietaryExposureRoutes = nonDietaryExposureRoutes,
                NonDietaryExposures = nonDietaryExposures,
                SelectedPopulation = new Population { NominalBodyWeight = 70 }
            };

            var project = new ProjectDto();
            project.AssessmentSettings.ExposureType = ExposureType.Acute;
            project.AssessmentSettings.Aggregate = true;
            project.AssessmentSettings.Cumulative = true;
            project.EffectSettings.TargetDoseLevelType = TargetLevelType.Internal;
            project.OutputDetailSettings.IsDetailedOutput = true;
            project.OutputDetailSettings.StoreIndividualDayIntakes = true;
            project.KineticModelSettings.InternalModelType = InternalModelType.AbsorptionFactorModel;
            var calculatorNom = new TargetExposuresActionCalculator(project);
            _ = TestRunUpdateSummarizeNominal(project, calculatorNom, data, "TestAcuteInternalAggregateNom");

            var calculator = new TargetExposuresActionCalculator(project);
            var (header, _) = TestRunUpdateSummarizeNominal(project, calculator, data, null);

            var factorialSet = new UncertaintyFactorialSet(UncertaintySource.Individuals);
            var uncertaintySourceGenerators = new Dictionary<UncertaintySource, IRandom> {
                [UncertaintySource.Individuals] = random
            };
            TestRunUpdateSummarizeUncertainty(calculator, data, header, random, factorialSet, uncertaintySourceGenerators, reportFileName: "TestAcuteInternalAggregate");
        }
        /// <summary>
        /// Runs the TargetExposures action: run, update simulation data, summarize action result, 
        /// run uncertain, update simulation data uncertain, summarize action result uncertain method
        /// Acute, TargetDoseLevelType = TargetDoseLevelType.External, LNN
        /// Including nondietary exposures (aggregate)
        /// </summary>
        [TestMethod]
        public void TargetExposuresActionCalculator_TestAcuteInternalAggregateKineticModel() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);

            var substances = MockSubstancesGenerator.Create(5);
            var correctedRelativePotencyFactors = substances.ToDictionary(c => c, c => 1d);
            var membershipProbabilities = substances.ToDictionary(c => c, c => 1d);
            var referenceCompound = substances.First();

            var individuals = MockIndividualsGenerator.Create(200, 2, random, useSamplingWeights: true);
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var foodsAsMeasured = MockFoodsGenerator.Create(3);
            var dietaryIndividualDayIntakes = MockDietaryIndividualDayIntakeGenerator.Create(individualDays, foodsAsMeasured, substances, 0, true, random);
            var dietaryExposureUnit = TargetUnit.FromExternalExposureUnit(ExternalExposureUnit.ugPerKgBWPerDay);

            var exposureRoutes = new List<ExposureRouteType>() { ExposureRouteType.Dietary, ExposureRouteType.Dermal, ExposureRouteType.Oral, ExposureRouteType.Inhalation };
            var nonDietaryExposureRoutes = new List<ExposureRouteType>() { ExposureRouteType.Dermal, ExposureRouteType.Oral, ExposureRouteType.Inhalation };
            var routes = new HashSet<ExposureRouteType>() { ExposureRouteType.Dermal, ExposureRouteType.Oral, ExposureRouteType.Inhalation };
            var nonDietaryExposures = MockNonDietaryExposureSetsGenerator.MockNonDietarySurveys(individuals, substances, routes, random, ExternalExposureUnit.ugPerKgBWPerDay, 1, true);

            var absorptionFactors = MockAbsorptionFactorsGenerator.Create(exposureRoutes, substances);

            var kineticModelInstances = new List<KineticModelInstance>();
            var instance = MockKineticModelsGenerator.CreateFakeEuroMixPBTKv5KineticModelInstance(substances.First());
            instance.CodeCompartment = "CLiver";

            var data = new ActionData() {
                ActiveSubstances = substances,
                CorrectedRelativePotencyFactors = correctedRelativePotencyFactors,
                MembershipProbabilities = membershipProbabilities,
                DietaryIndividualDayIntakes = dietaryIndividualDayIntakes,
                DietaryExposureUnit = dietaryExposureUnit,
                ReferenceSubstance = referenceCompound,
                AbsorptionFactors = absorptionFactors,
                NonDietaryExposureRoutes = nonDietaryExposureRoutes,
                NonDietaryExposures = nonDietaryExposures,
                SelectedPopulation = new Population { NominalBodyWeight = 70 },
                KineticModelInstances = new List<KineticModelInstance>() { instance }
            };

            var project = new ProjectDto();
            project.AssessmentSettings.ExposureType = ExposureType.Acute;
            project.AssessmentSettings.Aggregate = true;
            project.AssessmentSettings.Cumulative = true;
            project.EffectSettings.TargetDoseLevelType = TargetLevelType.Internal;
            project.OutputDetailSettings.IsDetailedOutput = true;
            project.OutputDetailSettings.StoreIndividualDayIntakes = true;
            project.KineticModelSettings.CodeCompartment = "CLiver";
            project.KineticModelSettings.InternalModelType = InternalModelType.PBKModel;

            var calculator = new TargetExposuresActionCalculator(project);
            var (header, _) = TestRunUpdateSummarizeNominal(project, calculator, data, "TestAcuteInternalAggregateNomPBK");

            var factorialSet = new UncertaintyFactorialSet(UncertaintySource.Individuals);
            var uncertaintySourceGenerators = new Dictionary<UncertaintySource, IRandom> {
                [UncertaintySource.Individuals] = random
            };
            TestRunUpdateSummarizeUncertainty(calculator, data, header, random, factorialSet, uncertaintySourceGenerators, reportFileName: "TestAcuteInternalAggregate");
        }

        /// <summary>
        /// Runs the TargetExposures action: run, update simulation data, summarize action result, 
        /// run uncertain, update simulation data uncertain, summarize action result uncertain method
        /// Acute, TargetDoseLevelType = TargetDoseLevelType.External, LNN
        /// Including nondietary exposures (aggregate)
        /// </summary>
        [TestMethod]
        public void TargetExposuresActionCalculator_TestChronicInternalAggregateKineticModel() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);

            var substances = MockSubstancesGenerator.Create(5);
            var correctedRelativePotencyFactors = substances.ToDictionary(c => c, c => 1d);
            var membershipProbabilities = substances.ToDictionary(c => c, c => 1d);
            var referenceCompound = substances.First();

            var individuals = MockIndividualsGenerator.Create(200, 2, random, useSamplingWeights: true);
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var foodsAsMeasured = MockFoodsGenerator.Create(3);
            var dietaryIndividualDayIntakes = MockDietaryIndividualDayIntakeGenerator.Create(individualDays, foodsAsMeasured, substances, 0, true, random);
            var dietaryExposureUnit = TargetUnit.FromExternalExposureUnit(ExternalExposureUnit.ugPerKgBWPerDay);

            var exposureRoutes = new List<ExposureRouteType>() { ExposureRouteType.Dietary, ExposureRouteType.Dermal, ExposureRouteType.Oral, ExposureRouteType.Inhalation };
            var nonDietaryExposureRoutes = new List<ExposureRouteType>() { ExposureRouteType.Dermal, ExposureRouteType.Oral, ExposureRouteType.Inhalation };
            var routes = new HashSet<ExposureRouteType>() { ExposureRouteType.Dermal, ExposureRouteType.Oral, ExposureRouteType.Inhalation };
            var nonDietaryExposures = MockNonDietaryExposureSetsGenerator.MockNonDietarySurveys(individuals, substances, routes, random, ExternalExposureUnit.ugPerKgBWPerDay, 1, true);

            var absorptionFactors = MockAbsorptionFactorsGenerator.Create(exposureRoutes, substances);

            var kineticModelInstances = new List<KineticModelInstance>();
            var instance = MockKineticModelsGenerator.CreateFakeEuroMixPBTKv5KineticModelInstance(substances.First());
            instance.CodeCompartment = "CLiver";

            var data = new ActionData() {
                ActiveSubstances = substances,
                CorrectedRelativePotencyFactors = correctedRelativePotencyFactors,
                MembershipProbabilities = membershipProbabilities,
                DietaryIndividualDayIntakes = dietaryIndividualDayIntakes,
                DietaryExposureUnit = dietaryExposureUnit,
                ReferenceSubstance = referenceCompound,
                AbsorptionFactors = absorptionFactors,
                NonDietaryExposureRoutes = nonDietaryExposureRoutes,
                NonDietaryExposures = nonDietaryExposures,
                SelectedPopulation = new Population { NominalBodyWeight = 70 },
                KineticModelInstances = new List<KineticModelInstance>() { instance }
            };

            var project = new ProjectDto();
            project.AssessmentSettings.ExposureType = ExposureType.Chronic;
            project.AssessmentSettings.Aggregate = true;
            project.AssessmentSettings.Cumulative = true;
            project.EffectSettings.TargetDoseLevelType = TargetLevelType.Internal;
            project.OutputDetailSettings.IsDetailedOutput = true;
            project.OutputDetailSettings.StoreIndividualDayIntakes = true;
            project.KineticModelSettings.CodeCompartment = "CLiver";
            project.KineticModelSettings.InternalModelType = InternalModelType.PBKModel;
            var calculator = new TargetExposuresActionCalculator(project);
            var (header, _) = TestRunUpdateSummarizeNominal(project, calculator, data, "TestChronicInternalAggregateNomPBK");

            var factorialSet = new UncertaintyFactorialSet(UncertaintySource.Individuals);
            var uncertaintySourceGenerators = new Dictionary<UncertaintySource, IRandom> {
                [UncertaintySource.Individuals] = random
            };

            TestRunUpdateSummarizeUncertainty(calculator, data, header, random, factorialSet, uncertaintySourceGenerators, reportFileName: "TestAcuteInternalAggregate");
        }

        /// <summary>
        /// Runs the TargetExposures action: run, update simulation data, summarize action result, 
        /// run uncertain, update simulation data uncertain, summarize action result uncertain method
        /// Acute, TargetDoseLevelType = TargetDoseLevelType.External, LNN
        /// Including nondietary exposures (aggregate)
        /// </summary>
        [TestMethod]
        public void TargetExposuresActionCalculator_TestChronicInternalAggregateKineticModelSingle() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);

            var substances = MockSubstancesGenerator.Create(1);
            var correctedRelativePotencyFactors = substances.ToDictionary(c => c, c => 1d);
            var membershipProbabilities = substances.ToDictionary(c => c, c => 1d);
            var referenceCompound = substances.First();

            var individuals = MockIndividualsGenerator.Create(200, 2, random, useSamplingWeights: true);
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var foodsAsMeasured = MockFoodsGenerator.Create(3);
            var dietaryIndividualDayIntakes = MockDietaryIndividualDayIntakeGenerator.Create(individualDays, foodsAsMeasured, substances, 0, true, random);
            var dietaryExposureUnit = TargetUnit.FromExternalExposureUnit(ExternalExposureUnit.ugPerKgBWPerDay);

            var exposureRoutes = new List<ExposureRouteType>() { ExposureRouteType.Dietary, ExposureRouteType.Dermal, ExposureRouteType.Oral, ExposureRouteType.Inhalation };
            var nonDietaryExposureRoutes = new List<ExposureRouteType>() { ExposureRouteType.Dermal, ExposureRouteType.Oral, ExposureRouteType.Inhalation };
            var routes = new HashSet<ExposureRouteType>() { ExposureRouteType.Dermal, ExposureRouteType.Oral, ExposureRouteType.Inhalation };
            var nonDietaryExposures = MockNonDietaryExposureSetsGenerator.MockNonDietarySurveys(individuals, substances, routes, random, ExternalExposureUnit.ugPerKgBWPerDay, 1, true);

            var absorptionFactors = MockAbsorptionFactorsGenerator.Create(exposureRoutes, substances);

            var kineticModelInstances = new List<KineticModelInstance>();
            var instance = MockKineticModelsGenerator.CreateFakeEuroMixPBTKv5KineticModelInstance(substances.First());
            instance.CodeCompartment = "CLiver";

            var data = new ActionData() {
                ActiveSubstances = substances,
                CorrectedRelativePotencyFactors = correctedRelativePotencyFactors,
                MembershipProbabilities = membershipProbabilities,
                DietaryIndividualDayIntakes = dietaryIndividualDayIntakes,
                DietaryExposureUnit = dietaryExposureUnit,
                ReferenceSubstance = referenceCompound,
                AbsorptionFactors = absorptionFactors,
                NonDietaryExposureRoutes = nonDietaryExposureRoutes,
                NonDietaryExposures = nonDietaryExposures,
                SelectedPopulation = new Population { NominalBodyWeight = 70 },
                KineticModelInstances = new List<KineticModelInstance>() { instance }
            };

            var project = new ProjectDto();
            project.AssessmentSettings.ExposureType = ExposureType.Chronic;
            project.AssessmentSettings.Aggregate = true;
            project.AssessmentSettings.Cumulative = true;
            project.EffectSettings.TargetDoseLevelType = TargetLevelType.Internal;
            project.OutputDetailSettings.IsDetailedOutput = true;
            project.OutputDetailSettings.StoreIndividualDayIntakes = true;
            project.KineticModelSettings.CodeCompartment = "CLiver";
            project.KineticModelSettings.InternalModelType = InternalModelType.PBKModel;
            var calculator = new TargetExposuresActionCalculator(project);
            var (header, _) = TestRunUpdateSummarizeNominal(project, calculator, data, "TestChronicInternalAggregateNomPBKSingle");

            var factorialSet = new UncertaintyFactorialSet(UncertaintySource.Individuals);
            var uncertaintySourceGenerators = new Dictionary<UncertaintySource, IRandom> {
                [UncertaintySource.Individuals] = random
            };

            TestRunUpdateSummarizeUncertainty(calculator, data, header, random, factorialSet, uncertaintySourceGenerators, reportFileName: "TestAcuteInternalAggregate");
        }

        /// <summary>
        /// Runs the TargetExposures action: run, update simulation data, summarize action result, 
        /// run uncertain, update simulation data uncertain, summarize action result uncertain method
        /// Acute, TargetDoseLevelType = TargetDoseLevelType.External, LNN
        /// Including nondietary exposures (aggregate)
        /// </summary>
        [TestMethod]
        public void TargetExposuresActionCalculator_TestAcuteInternalAggregateKineticModelSingle() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);

            var substances = MockSubstancesGenerator.Create(1);
            var correctedRelativePotencyFactors = substances.ToDictionary(c => c, c => 1d);
            var membershipProbabilities = substances.ToDictionary(c => c, c => 1d);
            var referenceCompound = substances.First();

            var individuals = MockIndividualsGenerator.Create(200, 2, random, useSamplingWeights: true);
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var foodsAsMeasured = MockFoodsGenerator.Create(3);
            var dietaryIndividualDayIntakes = MockDietaryIndividualDayIntakeGenerator.Create(individualDays, foodsAsMeasured, substances, 0, true, random);
            var dietaryExposureUnit = TargetUnit.FromExternalExposureUnit(ExternalExposureUnit.ugPerKgBWPerDay);

            var exposureRoutes = new List<ExposureRouteType>() { ExposureRouteType.Dietary, ExposureRouteType.Dermal, ExposureRouteType.Oral, ExposureRouteType.Inhalation };
            var nonDietaryExposureRoutes = new List<ExposureRouteType>() { ExposureRouteType.Dermal, ExposureRouteType.Oral, ExposureRouteType.Inhalation };
            var routes = new HashSet<ExposureRouteType>() { ExposureRouteType.Dermal, ExposureRouteType.Oral, ExposureRouteType.Inhalation };
            var nonDietaryExposures = MockNonDietaryExposureSetsGenerator.MockNonDietarySurveys(individuals, substances, routes, random, ExternalExposureUnit.ugPerKgBWPerDay, 1, true);

            var absorptionFactors = MockAbsorptionFactorsGenerator.Create(exposureRoutes, substances);

            var kineticModelInstances = new List<KineticModelInstance>();
            var instance = MockKineticModelsGenerator.CreateFakeEuroMixPBTKv5KineticModelInstance(substances.First());
            instance.CodeCompartment = "CLiver";

            var data = new ActionData() {
                ActiveSubstances = substances,
                CorrectedRelativePotencyFactors = correctedRelativePotencyFactors,
                MembershipProbabilities = membershipProbabilities,
                DietaryIndividualDayIntakes = dietaryIndividualDayIntakes,
                DietaryExposureUnit = dietaryExposureUnit,
                ReferenceSubstance = referenceCompound,
                AbsorptionFactors = absorptionFactors,
                NonDietaryExposureRoutes = nonDietaryExposureRoutes,
                NonDietaryExposures = nonDietaryExposures,
                SelectedPopulation = new Population { NominalBodyWeight = 70 },
                KineticModelInstances = new List<KineticModelInstance>() { instance }
            };

            var project = new ProjectDto();
            project.AssessmentSettings.ExposureType = ExposureType.Chronic;
            project.AssessmentSettings.Aggregate = true;
            project.AssessmentSettings.Cumulative = true;
            project.EffectSettings.TargetDoseLevelType = TargetLevelType.Internal;
            project.OutputDetailSettings.IsDetailedOutput = true;
            project.OutputDetailSettings.StoreIndividualDayIntakes = true;
            project.KineticModelSettings.CodeCompartment = "CLiver";
            project.KineticModelSettings.InternalModelType = InternalModelType.PBKModel;
            var calculator = new TargetExposuresActionCalculator(project);
            var (header, _) = TestRunUpdateSummarizeNominal(project, calculator, data, "TestAcuteInternalAggregateNomPBKSingle");

            var factorialSet = new UncertaintyFactorialSet(UncertaintySource.Individuals);
            var uncertaintySourceGenerators = new Dictionary<UncertaintySource, IRandom> {
                [UncertaintySource.Individuals] = random
            };

            TestRunUpdateSummarizeUncertainty(calculator, data, header, random, factorialSet, uncertaintySourceGenerators, reportFileName: "TestAcuteInternalAggregate");
        }
    }
}