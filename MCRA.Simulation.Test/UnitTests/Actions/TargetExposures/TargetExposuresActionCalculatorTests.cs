using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Action.UncertaintyFactorial;
using MCRA.Simulation.Actions.TargetExposures;
using MCRA.Simulation.Calculators.IntakeModelling;
using MCRA.Simulation.Calculators.KineticConversionFactorModels;
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
            var exposureRoutes = new[] { ExposurePathType.Oral };
            var kineticConversionFactors = MockKineticModelsGenerator.CreateKineticConversionFactors(
                substances,
                exposureRoutes,
                TargetUnit.FromInternalDoseUnit(DoseUnit.ugPerKg, BiologicalMatrix.Liver)
            );
            var kineticConversionFactorModels = kineticConversionFactors?
                .Select(c => KineticConversionFactorCalculatorFactory
                    .Create(c, false)
                ).ToList();
            var data = new ActionData() {
                ActiveSubstances = substances,
                DietaryIndividualDayIntakes = dietaryIndividualDayIntakes,
                ExternalExposureUnit = ExposureUnitTriple.FromExposureUnit(ExternalExposureUnit.ugPerKgBWPerDay),
                DietaryExposureUnit = TargetUnit.FromExternalExposureUnit(ExternalExposureUnit.ugPerKgBWPerDay),
                SelectedPopulation = new Population { NominalBodyWeight = 70 },
                KineticConversionFactorModels = kineticConversionFactorModels
            };

            var project = new ProjectDto();
            var config = project.TargetExposuresSettings;
            config.TargetDoseLevelType = TargetLevelType.Internal;
            config.CodeCompartment = "Liver";
            config.InternalModelType = InternalModelType.PBKModel;
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
            var exposureRoutes = new[] { ExposurePathType.Dermal, ExposurePathType.Oral, ExposurePathType.Inhalation };
            var referenceCompound = substances.First();
            var kineticConversionFactors = MockKineticModelsGenerator.CreateKineticConversionFactors(
                substances,
                exposureRoutes,
                TargetUnit.FromInternalDoseUnit(DoseUnit.ugPerKg, BiologicalMatrix.Liver)
            );
            var kineticConversionFactorModels = kineticConversionFactors?
                .Select(c => KineticConversionFactorCalculatorFactory
                    .Create(c, false)
                ).ToList();
            var data = new ActionData() {
                ActiveSubstances = substances,
                CorrectedRelativePotencyFactors = correctedRelativePotencyFactors,
                MembershipProbabilities = membershipProbabilities,
                DietaryIndividualDayIntakes = dietaryIndividualDayIntakes,
                DietaryExposureUnit = dietaryExposureUnit,
                ReferenceSubstance = referenceCompound,
                SelectedPopulation = new Population { NominalBodyWeight = 70 },
                KineticConversionFactorModels = kineticConversionFactorModels
            };

            var project = new ProjectDto();
            var config = project.TargetExposuresSettings;
            config.Cumulative = true;
            config.ExposureType = ExposureType.Chronic;
            config.TargetDoseLevelType = TargetLevelType.Internal;
            config.CodeCompartment = "Liver";
            config.InternalModelType = InternalModelType.PBKModel;
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
            var exposureRoutes = new[] { ExposurePathType.Dermal, ExposurePathType.Oral, ExposurePathType.Inhalation };
            var nonDietaryExposureRoutes = new[] { ExposurePathType.Dermal, ExposurePathType.Oral, ExposurePathType.Inhalation };
            var routes = new HashSet<ExposurePathType>() { ExposurePathType.Dermal, ExposurePathType.Oral, ExposurePathType.Inhalation };
            var nonDietaryExposures = MockNonDietaryExposureSetsGenerator.MockNonDietarySurveys(individuals, substances, routes, random, ExternalExposureUnit.ugPerKgBWPerDay, 1, true);
            var kineticConversionFactors = MockKineticModelsGenerator.CreateKineticConversionFactors(
                substances,
                exposureRoutes,
                TargetUnit.FromInternalDoseUnit(DoseUnit.ugPerKg, BiologicalMatrix.Liver)
            );
            var kineticConversionFactorModels = kineticConversionFactors?
                .Select(c => KineticConversionFactorCalculatorFactory
                    .Create(c, false)
                ).ToList();

            var data = new ActionData() {
                ActiveSubstances = substances,
                CorrectedRelativePotencyFactors = correctedRelativePotencyFactors,
                MembershipProbabilities = membershipProbabilities,
                DietaryIndividualDayIntakes = dietaryIndividualDayIntakes,
                DietaryExposureUnit = dietaryExposureUnit,
                ReferenceSubstance = referenceCompound,
                DietaryModelBasedIntakeResults = dietaryModelBasedIntakeResults,
                KineticConversionFactorModels = kineticConversionFactorModels,
                NonDietaryExposureRoutes = nonDietaryExposureRoutes,
                NonDietaryExposures = nonDietaryExposures,
                SelectedPopulation = new Population { NominalBodyWeight = 70 }
            };

            var project = new ProjectDto();
            var config = project.TargetExposuresSettings;
            config.Cumulative = true;
            config.ExposureType = ExposureType.Chronic;
            config.TargetDoseLevelType = TargetLevelType.Internal;
            config.Aggregate = true;
            config.IsDetailedOutput = true;
            config.StoreIndividualDayIntakes = true;
            config.CodeCompartment = "Liver";
            config.InternalModelType = InternalModelType.PBKModel;
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
            var dietaryIndividualDayIntakes = MockDietaryIndividualDayIntakeGenerator
                .Create(individualDays, foodsAsMeasured, substances, 0, true, random);
            var dietaryExposureUnit = TargetUnit.FromExternalExposureUnit(ExternalExposureUnit.ugPerKgBWPerDay);

            var exposureRoutes = new[] { ExposurePathType.Dermal, ExposurePathType.Oral, ExposurePathType.Inhalation };
            var nonDietaryExposureRoutes = new HashSet<ExposurePathType>() { ExposurePathType.Dermal, ExposurePathType.Oral, ExposurePathType.Inhalation };
            var nonDietaryExposures = MockNonDietaryExposureSetsGenerator.MockNonDietarySurveys(individuals, substances, nonDietaryExposureRoutes, random, ExternalExposureUnit.ugPerKgBWPerDay, 1, true);
            var kineticConversionFactors = MockKineticModelsGenerator.CreateKineticConversionFactors(
                substances,
                exposureRoutes,
                TargetUnit.FromInternalDoseUnit(DoseUnit.ugPerKg, BiologicalMatrix.Liver)
            );

            var kineticConversionFactorModels = kineticConversionFactors?
                .Select(c => KineticConversionFactorCalculatorFactory
                    .Create(c, false)
                ).ToList();

            var data = new ActionData() {
                ActiveSubstances = substances,
                CorrectedRelativePotencyFactors = correctedRelativePotencyFactors,
                MembershipProbabilities = membershipProbabilities,
                DietaryIndividualDayIntakes = dietaryIndividualDayIntakes,
                DietaryExposureUnit = dietaryExposureUnit,
                ReferenceSubstance = referenceCompound,
                NonDietaryExposureRoutes = nonDietaryExposureRoutes,
                NonDietaryExposures = nonDietaryExposures,
                SelectedPopulation = new Population { NominalBodyWeight = 70 },
                KineticConversionFactorModels = kineticConversionFactorModels
            };

            var project = new ProjectDto();
            var config = project.TargetExposuresSettings;
            config.ExposureType = ExposureType.Acute;
            config.Aggregate = true;
            config.Cumulative = true;
            config.TargetDoseLevelType = TargetLevelType.Internal;
            config.IsDetailedOutput = true;
            config.StoreIndividualDayIntakes = true;
            config.CodeCompartment = "Liver";
            config.InternalModelType = InternalModelType.PBKModel;

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
        /// Combination of PBK and Linear model
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

            var nonDietaryExposureRoutes = new[] { ExposurePathType.Dermal, ExposurePathType.Oral, ExposurePathType.Inhalation };
            var nonDietaryExposures = MockNonDietaryExposureSetsGenerator.MockNonDietarySurveys(individuals, substances, nonDietaryExposureRoutes, random, ExternalExposureUnit.ugPerKgBWPerDay, 1, true);

            var exposureRoutes = new[] { ExposurePathType.Dermal, ExposurePathType.Oral, ExposurePathType.Inhalation };
            var kineticConversionFactors = MockKineticModelsGenerator.CreateKineticConversionFactors(
                substances,
                exposureRoutes,
                TargetUnit.FromInternalDoseUnit(DoseUnit.ugPerKg, BiologicalMatrix.Liver)
            );

            var kineticConversionFactorModels = kineticConversionFactors?
                .Select(c => KineticConversionFactorCalculatorFactory
                    .Create(c, false)
                ).ToList();
            var kineticModelInstances = new List<KineticModelInstance>();
            var instance = MockKineticModelsGenerator.CreatePbkModelInstance(substances.First());

            var data = new ActionData() {
                ActiveSubstances = substances,
                CorrectedRelativePotencyFactors = correctedRelativePotencyFactors,
                MembershipProbabilities = membershipProbabilities,
                DietaryIndividualDayIntakes = dietaryIndividualDayIntakes,
                DietaryExposureUnit = dietaryExposureUnit,
                ReferenceSubstance = referenceCompound,
                KineticConversionFactorModels = kineticConversionFactorModels,
                NonDietaryExposureRoutes = nonDietaryExposureRoutes,
                NonDietaryExposures = nonDietaryExposures,
                SelectedPopulation = new Population { NominalBodyWeight = 70 },
                KineticModelInstances = new List<KineticModelInstance>() { instance }
            };

            var project = new ProjectDto();
            var config = project.TargetExposuresSettings;
            config.ExposureType = ExposureType.Acute;
            config.Aggregate = true;
            config.Cumulative = true;
            config.TargetDoseLevelType = TargetLevelType.Internal;
            config.IsDetailedOutput = true;
            config.StoreIndividualDayIntakes = true;
            config.CodeCompartment = "Liver";
            config.InternalModelType = InternalModelType.PBKModel;

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
        /// Combination of PBK and Linear model
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

            var exposureRoutes = new[] { ExposurePathType.Dermal, ExposurePathType.Oral, ExposurePathType.Inhalation };
            var nonDietaryExposureRoutes = new[] { ExposurePathType.Dermal, ExposurePathType.Oral, ExposurePathType.Inhalation };
            var routes = new HashSet<ExposurePathType>() { ExposurePathType.Dermal, ExposurePathType.Oral, ExposurePathType.Inhalation };
            var nonDietaryExposures = MockNonDietaryExposureSetsGenerator.MockNonDietarySurveys(individuals, substances, routes, random, ExternalExposureUnit.ugPerKgBWPerDay, 1, true);

            var kineticConversionFactors = MockKineticModelsGenerator.CreateKineticConversionFactors(
                substances,
                exposureRoutes,
                TargetUnit.FromInternalDoseUnit(DoseUnit.ugPerKg, BiologicalMatrix.Liver)
            );

            var kineticConversionFactorModels = kineticConversionFactors?
                .Select(c => KineticConversionFactorCalculatorFactory
                    .Create(c, false)
                ).ToList();
            var kineticModelInstances = new List<KineticModelInstance>();
            var instance = MockKineticModelsGenerator.CreatePbkModelInstance(substances.First());

            var data = new ActionData() {
                ActiveSubstances = substances,
                CorrectedRelativePotencyFactors = correctedRelativePotencyFactors,
                MembershipProbabilities = membershipProbabilities,
                DietaryIndividualDayIntakes = dietaryIndividualDayIntakes,
                DietaryExposureUnit = dietaryExposureUnit,
                ReferenceSubstance = referenceCompound,
                KineticConversionFactorModels = kineticConversionFactorModels,
                NonDietaryExposureRoutes = nonDietaryExposureRoutes,
                NonDietaryExposures = nonDietaryExposures,
                SelectedPopulation = new Population { NominalBodyWeight = 70 },
                KineticModelInstances = new List<KineticModelInstance>() { instance }
            };

            var project = new ProjectDto();
            var config = project.TargetExposuresSettings;
            config.ExposureType = ExposureType.Chronic;
            config.Aggregate = true;
            config.Cumulative = true;
            config.TargetDoseLevelType = TargetLevelType.Internal;
            config.IsDetailedOutput = true;
            config.StoreIndividualDayIntakes = true;
            config.CodeCompartment = "Liver";
            config.InternalModelType = InternalModelType.PBKModel;
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
            var relativePotencyFactors = substances.ToDictionary(c => c, c => 1d);
            var membershipProbabilities = substances.ToDictionary(c => c, c => 1d);
            var referenceCompound = substances.First();

            var individuals = MockIndividualsGenerator.Create(200, 2, random, useSamplingWeights: true);
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var foodsAsMeasured = MockFoodsGenerator.Create(3);
            var dietaryIndividualDayIntakes = MockDietaryIndividualDayIntakeGenerator.Create(individualDays, foodsAsMeasured, substances, 0, true, random);
            var dietaryExposureUnit = TargetUnit.FromExternalExposureUnit(ExternalExposureUnit.ugPerKgBWPerDay);

            var exposureRoutes = new[]   { ExposurePathType.Dermal, ExposurePathType.Oral, ExposurePathType.Inhalation };
            var nonDietaryExposureRoutes = new[] { ExposurePathType.Dermal, ExposurePathType.Oral, ExposurePathType.Inhalation };
            var routes = new HashSet<ExposurePathType>() { ExposurePathType.Dermal, ExposurePathType.Oral, ExposurePathType.Inhalation };
            var nonDietaryExposures = MockNonDietaryExposureSetsGenerator.MockNonDietarySurveys(individuals, substances, routes, random, ExternalExposureUnit.ugPerKgBWPerDay, 1, true);

            var kineticConversionFactors = MockKineticModelsGenerator.CreateKineticConversionFactors(
                substances,
                exposureRoutes,
                dietaryExposureUnit
            );

            var kineticConversionFactorModels = kineticConversionFactors?
                .Select(c => KineticConversionFactorCalculatorFactory
                    .Create(c, false)
                ).ToList();
            var kineticModelInstances = new List<KineticModelInstance>();
            var instance = MockKineticModelsGenerator.CreatePbkModelInstance(substances.First());

            var data = new ActionData() {
                ActiveSubstances = substances,
                CorrectedRelativePotencyFactors = relativePotencyFactors,
                MembershipProbabilities = membershipProbabilities,
                DietaryIndividualDayIntakes = dietaryIndividualDayIntakes,
                DietaryExposureUnit = dietaryExposureUnit,
                ReferenceSubstance = referenceCompound,
                KineticConversionFactorModels = kineticConversionFactorModels,
                NonDietaryExposureRoutes = nonDietaryExposureRoutes,
                NonDietaryExposures = nonDietaryExposures,
                SelectedPopulation = new Population { NominalBodyWeight = 70 },
                KineticModelInstances = new List<KineticModelInstance>() { instance }
            };

            var project = new ProjectDto();
            var config = project.TargetExposuresSettings;
            config.ExposureType = ExposureType.Chronic;
            config.Aggregate = true;
            config.Cumulative = true;
            config.TargetDoseLevelType = TargetLevelType.Internal;
            config.IsDetailedOutput = true;
            config.StoreIndividualDayIntakes = true;
            config.CodeCompartment = "Liver";
            config.InternalModelType = InternalModelType.PBKModel;
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

            var exposureRoutes = new[] { ExposurePathType.Dermal, ExposurePathType.Oral, ExposurePathType.Inhalation };
            var nonDietaryExposureRoutes = new[] { ExposurePathType.Dermal, ExposurePathType.Oral, ExposurePathType.Inhalation };
            var routes = new HashSet<ExposurePathType>() { ExposurePathType.Dermal, ExposurePathType.Oral, ExposurePathType.Inhalation };
            var nonDietaryExposures = MockNonDietaryExposureSetsGenerator.MockNonDietarySurveys(individuals, substances, routes, random, ExternalExposureUnit.ugPerKgBWPerDay, 1, true);

            var kineticConversionFactors = MockKineticModelsGenerator.CreateKineticConversionFactors(
                substances,
                exposureRoutes,
                dietaryExposureUnit
            );

            var kineticConversionFactorModels = kineticConversionFactors?
                .Select(c => KineticConversionFactorCalculatorFactory
                    .Create(c, false)
                ).ToList();
            var kineticModelInstances = new List<KineticModelInstance>();
            var instance = MockKineticModelsGenerator.CreatePbkModelInstance(substances.First());

            var data = new ActionData() {
                ActiveSubstances = substances,
                CorrectedRelativePotencyFactors = correctedRelativePotencyFactors,
                MembershipProbabilities = membershipProbabilities,
                DietaryIndividualDayIntakes = dietaryIndividualDayIntakes,
                DietaryExposureUnit = dietaryExposureUnit,
                ReferenceSubstance = referenceCompound,
                KineticConversionFactorModels = kineticConversionFactorModels,
                NonDietaryExposureRoutes = nonDietaryExposureRoutes,
                NonDietaryExposures = nonDietaryExposures,
                SelectedPopulation = new Population { NominalBodyWeight = 70 },
                KineticModelInstances = new List<KineticModelInstance>() { instance }
            };

            var project = new ProjectDto();
            var config = project.TargetExposuresSettings;
            config.ExposureType = ExposureType.Chronic;
            config.Aggregate = true;
            config.Cumulative = true;
            config.TargetDoseLevelType = TargetLevelType.Internal;
            config.IsDetailedOutput = true;
            config.StoreIndividualDayIntakes = true;
            config.CodeCompartment = "Liver";
            config.InternalModelType = InternalModelType.PBKModel;
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
