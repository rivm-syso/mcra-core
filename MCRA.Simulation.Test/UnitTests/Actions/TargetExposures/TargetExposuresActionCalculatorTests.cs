using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Action.UncertaintyFactorial;
using MCRA.Simulation.Actions.TargetExposures;
using MCRA.Simulation.Calculators.IntakeModelling;
using MCRA.Simulation.Calculators.KineticConversionFactorModels;
using MCRA.Simulation.Objects;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using MCRA.Utils.Statistics;

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
            var foods = FakeFoodsGenerator.Create(3);
            var substances = FakeSubstancesGenerator.Create(1);
            var individuals = FakeIndividualsGenerator.Create(25, 2, random, useSamplingWeights: true);
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var dietaryIndividualDayIntakes = FakeDietaryIndividualDayIntakeGenerator.Create(individualDays, foods, substances, 0.5, false, random, false);
            var routes = new[] { ExposureRoute.Oral };
            var kineticConversionFactors = FakeKineticConversionFactorModelsGenerator.CreateKineticConversionFactors(
                substances,
                routes,
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
                KineticConversionFactorModels = kineticConversionFactorModels,
                MembershipProbabilities = substances.ToDictionary(c => c, c => 1d)
            };

            var project = new ProjectDto();
            var config = project.TargetExposuresSettings;
            config.TargetDoseLevelType = TargetLevelType.Internal;
            config.CodeCompartment = "Liver";
            config.InternalModelType = InternalModelType.PBKModel;
            config.ExposureSources = [ExposureSource.Diet];
            config.ExposureRoutes = [.. routes];
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
            var substances = FakeSubstancesGenerator.Create(5);
            var correctedRelativePotencyFactors = substances.ToDictionary(c => c, c => 1d);
            var membershipProbabilities = substances.ToDictionary(c => c, c => 1d);
            var individuals = FakeIndividualsGenerator.Create(25, 2, random, useSamplingWeights: true);
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var foodsAsMeasured = FakeFoodsGenerator.Create(3);
            var dietaryIndividualDayIntakes = FakeDietaryIndividualDayIntakeGenerator.Create(individualDays, foodsAsMeasured, substances, 0, true, random);
            var dietaryExposureUnit = TargetUnit.FromExternalExposureUnit(ExternalExposureUnit.ugPerKgBWPerDay);
            var routes = new[] { ExposureRoute.Dermal, ExposureRoute.Oral, ExposureRoute.Inhalation };
            var referenceCompound = substances.First();
            var kineticConversionFactors = FakeKineticConversionFactorModelsGenerator.CreateKineticConversionFactors(
                substances,
                routes,
                TargetUnit.FromInternalDoseUnit(DoseUnit.ugPerKg, BiologicalMatrix.Liver)
            );
            var kineticConversionFactorModels = kineticConversionFactors?
                .Select(c => KineticConversionFactorCalculatorFactory.Create(c, false))
                .ToList();
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
            config.ExposureSources = [ExposureSource.Diet];
            config.ExposureRoutes = [.. routes];
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
            var substances = FakeSubstancesGenerator.Create(5);
            var correctedRelativePotencyFactors = substances.ToDictionary(c => c, c => 1d);
            var membershipProbabilities = substances.ToDictionary(c => c, c => 1d);
            var individuals = FakeIndividualsGenerator.Create(200, 2, random, useSamplingWeights: true);
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var foodsAsMeasured = FakeFoodsGenerator.Create(3);
            var dietaryIndividualDayIntakes = FakeDietaryIndividualDayIntakeGenerator.Create(individualDays, foodsAsMeasured, substances, 0, true, random);
            var dietaryExposureUnit = TargetUnit.FromExternalExposureUnit(ExternalExposureUnit.ugPerKgBWPerDay);
            var intakes = dietaryIndividualDayIntakes.Select(c => c.TotalExposurePerMassUnit(correctedRelativePotencyFactors, membershipProbabilities, false)).ToList();
            var dietaryModelBasedIntakeResults = new List<ModelBasedIntakeResult> { new() { ModelBasedIntakes = intakes, CovariateGroup = new CovariateGroup() } };
            var referenceCompound = substances.First();
            var exposureRoutes = new[] { ExposureRoute.Dermal, ExposureRoute.Oral, ExposureRoute.Inhalation };
            var nonDietaryExposureRoutes = new[] { ExposureRoute.Dermal, ExposureRoute.Oral, ExposureRoute.Inhalation };
            var routes = new HashSet<ExposureRoute>() { ExposureRoute.Dermal, ExposureRoute.Oral, ExposureRoute.Inhalation };
            var nonDietaryExposures = FakeNonDietaryExposureSetsGenerator.Create(individuals, substances, routes, random, ExternalExposureUnit.ugPerKgBWPerDay);
            var kineticConversionFactors = FakeKineticConversionFactorModelsGenerator.CreateKineticConversionFactors(
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
            config.ExposureRoutes = [.. exposureRoutes];
            config.ExposureSources = [ExposureSource.Diet, ExposureSource.OtherNonDiet];
            config.IsDetailedOutput = true;
            config.StoreIndividualDayIntakes = true;
            config.CodeCompartment = "Liver";
            config.InternalModelType = InternalModelType.PBKModel;
            config.NonDietaryPopulationAlignmentMethod = PopulationAlignmentMethod.MatchIndividualID;
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

            var substances = FakeSubstancesGenerator.Create(5);
            var correctedRelativePotencyFactors = substances.ToDictionary(c => c, c => 1d);
            var membershipProbabilities = substances.ToDictionary(c => c, c => 1d);
            var referenceCompound = substances.First();

            var individuals = FakeIndividualsGenerator.Create(200, 2, random, useSamplingWeights: true);
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var foodsAsMeasured = FakeFoodsGenerator.Create(3);
            var dietaryIndividualDayIntakes = FakeDietaryIndividualDayIntakeGenerator
                .Create(individualDays, foodsAsMeasured, substances, 0, true, random);
            var dietaryExposureUnit = TargetUnit.FromExternalExposureUnit(ExternalExposureUnit.ugPerKgBWPerDay);

            var routes = new[] { ExposureRoute.Dermal, ExposureRoute.Oral, ExposureRoute.Inhalation };
            var nonDietaryExposureRoutes = new HashSet<ExposureRoute>() { ExposureRoute.Dermal, ExposureRoute.Oral, ExposureRoute.Inhalation };
            var nonDietaryExposures = FakeNonDietaryExposureSetsGenerator.Create(individuals, substances, nonDietaryExposureRoutes, random, ExternalExposureUnit.ugPerKgBWPerDay);
            var kineticConversionFactors = FakeKineticConversionFactorModelsGenerator.CreateKineticConversionFactors(
                substances,
                routes,
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
            config.ExposureRoutes = [.. routes];
            config.ExposureSources = [ExposureSource.Diet, ExposureSource.OtherNonDiet];
            config.Cumulative = true;
            config.TargetDoseLevelType = TargetLevelType.Internal;
            config.IsDetailedOutput = true;
            config.StoreIndividualDayIntakes = true;
            config.CodeCompartment = "Liver";
            config.InternalModelType = InternalModelType.PBKModel;
            config.NonDietaryPopulationAlignmentMethod = PopulationAlignmentMethod.MatchIndividualID;

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

            var substances = FakeSubstancesGenerator.Create(5);
            var correctedRelativePotencyFactors = substances.ToDictionary(c => c, c => 1d);
            var membershipProbabilities = substances.ToDictionary(c => c, c => 1d);
            var referenceCompound = substances.First();

            var individuals = FakeIndividualsGenerator.Create(200, 2, random, useSamplingWeights: true);
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var foodsAsMeasured = FakeFoodsGenerator.Create(3);
            var dietaryIndividualDayIntakes = FakeDietaryIndividualDayIntakeGenerator.Create(individualDays, foodsAsMeasured, substances, 0, true, random);
            var dietaryExposureUnit = TargetUnit.FromExternalExposureUnit(ExternalExposureUnit.ugPerKgBWPerDay);

            var nonDietaryExposureRoutes = new[] { ExposureRoute.Dermal, ExposureRoute.Oral, ExposureRoute.Inhalation };
            var nonDietaryExposures = FakeNonDietaryExposureSetsGenerator.Create(individuals, substances, nonDietaryExposureRoutes, random, ExternalExposureUnit.ugPerKgBWPerDay);

            var exposureRoutes = new[] { ExposureRoute.Dermal, ExposureRoute.Oral, ExposureRoute.Inhalation };
            var kineticConversionFactors = FakeKineticConversionFactorModelsGenerator.CreateKineticConversionFactors(
                substances,
                exposureRoutes,
                TargetUnit.FromInternalDoseUnit(DoseUnit.ugPerKg, BiologicalMatrix.Liver)
            );

            var kineticConversionFactorModels = kineticConversionFactors?
                .Select(c => KineticConversionFactorCalculatorFactory
                    .Create(c, false)
                ).ToList();
            var kineticModelInstances = new List<KineticModelInstance>();
            var instance = FakeKineticModelsGenerator.CreatePbkModelInstance(substances.First());

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
                KineticModelInstances = [instance]
            };

            var project = new ProjectDto();
            var config = project.TargetExposuresSettings;
            config.ExposureType = ExposureType.Acute;
            config.ExposureRoutes = [.. exposureRoutes];
            config.ExposureSources = [ExposureSource.Diet, ExposureSource.OtherNonDiet];
            config.Cumulative = true;
            config.TargetDoseLevelType = TargetLevelType.Internal;
            config.IsDetailedOutput = true;
            config.StoreIndividualDayIntakes = true;
            config.CodeCompartment = "Liver";
            config.InternalModelType = InternalModelType.PBKModel;
            config.NonDietaryPopulationAlignmentMethod = PopulationAlignmentMethod.MatchIndividualID;
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

            var substances = FakeSubstancesGenerator.Create(5);
            var correctedRelativePotencyFactors = substances.ToDictionary(c => c, c => 1d);
            var membershipProbabilities = substances.ToDictionary(c => c, c => 1d);
            var referenceCompound = substances.First();

            var individuals = FakeIndividualsGenerator.Create(200, 2, random, useSamplingWeights: true);
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var foodsAsMeasured = FakeFoodsGenerator.Create(3);
            var dietaryIndividualDayIntakes = FakeDietaryIndividualDayIntakeGenerator.Create(individualDays, foodsAsMeasured, substances, 0, true, random);
            var dietaryExposureUnit = TargetUnit.FromExternalExposureUnit(ExternalExposureUnit.ugPerKgBWPerDay);

            var exposureRoutes = new[] { ExposureRoute.Dermal, ExposureRoute.Oral, ExposureRoute.Inhalation };
            var nonDietaryExposureRoutes = new[] { ExposureRoute.Dermal, ExposureRoute.Oral, ExposureRoute.Inhalation };
            var routes = new HashSet<ExposureRoute>() { ExposureRoute.Dermal, ExposureRoute.Oral, ExposureRoute.Inhalation };
            var nonDietaryExposures = FakeNonDietaryExposureSetsGenerator.Create(individuals, substances, routes, random, ExternalExposureUnit.ugPerKgBWPerDay);

            var kineticConversionFactors = FakeKineticConversionFactorModelsGenerator.CreateKineticConversionFactors(
                substances,
                exposureRoutes,
                TargetUnit.FromInternalDoseUnit(DoseUnit.ugPerKg, BiologicalMatrix.Liver)
            );

            var kineticConversionFactorModels = kineticConversionFactors?
                .Select(c => KineticConversionFactorCalculatorFactory
                    .Create(c, false)
                ).ToList();
            var kineticModelInstances = new List<KineticModelInstance>();
            var instance = FakeKineticModelsGenerator.CreatePbkModelInstance(substances.First());

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
                KineticModelInstances = [instance]
            };

            var project = new ProjectDto();
            var config = project.TargetExposuresSettings;
            config.ExposureType = ExposureType.Chronic;
            config.ExposureRoutes = [.. exposureRoutes];
            config.ExposureSources = [ExposureSource.Diet, ExposureSource.OtherNonDiet];
            config.Cumulative = true;
            config.TargetDoseLevelType = TargetLevelType.Internal;
            config.IsDetailedOutput = true;
            config.StoreIndividualDayIntakes = true;
            config.CodeCompartment = "Liver";
            config.InternalModelType = InternalModelType.PBKModel;
            config.NonDietaryPopulationAlignmentMethod = PopulationAlignmentMethod.MatchIndividualID;
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

            var substances = FakeSubstancesGenerator.Create(1);
            var relativePotencyFactors = substances.ToDictionary(c => c, c => 1d);
            var membershipProbabilities = substances.ToDictionary(c => c, c => 1d);
            var referenceCompound = substances.First();

            var individuals = FakeIndividualsGenerator.Create(200, 2, random, useSamplingWeights: true);
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var foodsAsMeasured = FakeFoodsGenerator.Create(3);
            var dietaryIndividualDayIntakes = FakeDietaryIndividualDayIntakeGenerator.Create(individualDays, foodsAsMeasured, substances, 0, true, random);
            var dietaryExposureUnit = TargetUnit.FromExternalExposureUnit(ExternalExposureUnit.ugPerKgBWPerDay);

            var exposureRoutes = new[]   { ExposureRoute.Dermal, ExposureRoute.Oral, ExposureRoute.Inhalation };
            var nonDietaryExposureRoutes = new[] { ExposureRoute.Dermal, ExposureRoute.Oral, ExposureRoute.Inhalation };
            var routes = new HashSet<ExposureRoute>() { ExposureRoute.Dermal, ExposureRoute.Oral, ExposureRoute.Inhalation };
            var nonDietaryExposures = FakeNonDietaryExposureSetsGenerator.Create(individuals, substances, routes, random, ExternalExposureUnit.ugPerKgBWPerDay);
            var biologicalMatrix = BiologicalMatrix.Liver;
            var kineticConversionFactors = FakeKineticConversionFactorModelsGenerator.CreateKineticConversionFactors(
                substances,
                exposureRoutes,
                dietaryExposureUnit
            );
            foreach (var kcf in kineticConversionFactors) {
                kcf.BiologicalMatrixTo = biologicalMatrix;
            }
            var kineticConversionFactorModels = kineticConversionFactors?
                .Select(c => KineticConversionFactorCalculatorFactory
                    .Create(c, false)
                ).ToList();
            var kineticModelInstances = new List<KineticModelInstance>();
            var instance = FakeKineticModelsGenerator.CreatePbkModelInstance(substances.First());

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
                KineticModelInstances = [instance]
            };

            var project = new ProjectDto();
            var config = project.TargetExposuresSettings;
            config.ExposureType = ExposureType.Chronic;
            config.ExposureRoutes = [.. exposureRoutes];
            config.ExposureSources = [ExposureSource.Diet, ExposureSource.OtherNonDiet];
            config.Cumulative = true;
            config.TargetDoseLevelType = TargetLevelType.Internal;
            config.IsDetailedOutput = true;
            config.StoreIndividualDayIntakes = true;
            config.CodeCompartment = biologicalMatrix.ToString();
            config.InternalModelType = InternalModelType.PBKModel;
            config.NonDietaryPopulationAlignmentMethod = PopulationAlignmentMethod.MatchIndividualID;
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

            var substances = FakeSubstancesGenerator.Create(1);
            var correctedRelativePotencyFactors = substances.ToDictionary(c => c, c => 1d);
            var membershipProbabilities = substances.ToDictionary(c => c, c => 1d);
            var referenceCompound = substances.First();

            var individuals = FakeIndividualsGenerator.Create(200, 2, random, useSamplingWeights: true);
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var foodsAsMeasured = FakeFoodsGenerator.Create(3);
            var dietaryIndividualDayIntakes = FakeDietaryIndividualDayIntakeGenerator.Create(individualDays, foodsAsMeasured, substances, 0, true, random);
            var dietaryExposureUnit = TargetUnit.FromExternalExposureUnit(ExternalExposureUnit.ugPerKgBWPerDay);

            var exposureRoutes = new[] { ExposureRoute.Dermal, ExposureRoute.Oral, ExposureRoute.Inhalation };
            var nonDietaryExposureRoutes = new[] { ExposureRoute.Dermal, ExposureRoute.Oral, ExposureRoute.Inhalation };
            var routes = new HashSet<ExposureRoute>() { ExposureRoute.Dermal, ExposureRoute.Oral, ExposureRoute.Inhalation };
            var nonDietaryExposures = FakeNonDietaryExposureSetsGenerator.Create(individuals, substances, routes, random, ExternalExposureUnit.ugPerKgBWPerDay);
            var biologicalMatrix = BiologicalMatrix.Liver;
            var kineticConversionFactors = FakeKineticConversionFactorModelsGenerator.CreateKineticConversionFactors(
                substances,
                exposureRoutes,
                dietaryExposureUnit
            );
            foreach (var kcf in kineticConversionFactors) {
                kcf.BiologicalMatrixTo = biologicalMatrix;
            }
            var kineticConversionFactorModels = kineticConversionFactors?
                .Select(c => KineticConversionFactorCalculatorFactory
                    .Create(c, false)
                ).ToList();
            var kineticModelInstances = new List<KineticModelInstance>();
            var instance = FakeKineticModelsGenerator.CreatePbkModelInstance(substances.First());

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
                KineticModelInstances = [instance]
            };

            var project = new ProjectDto();
            var config = project.TargetExposuresSettings;
            config.ExposureType = ExposureType.Chronic;
            config.ExposureRoutes = [.. exposureRoutes];
            config.ExposureSources = [ExposureSource.Diet, ExposureSource.OtherNonDiet];
            config.Cumulative = true;
            config.TargetDoseLevelType = TargetLevelType.Internal;
            config.IsDetailedOutput = true;
            config.StoreIndividualDayIntakes = true;
            config.CodeCompartment = biologicalMatrix.ToString();
            config.InternalModelType = InternalModelType.PBKModel;
            config.NonDietaryPopulationAlignmentMethod = PopulationAlignmentMethod.MatchIndividualID;
            var calculator = new TargetExposuresActionCalculator(project);
            var (header, _) = TestRunUpdateSummarizeNominal(project, calculator, data, "TestAcuteInternalAggregateNomPBKSingle");

            var factorialSet = new UncertaintyFactorialSet(UncertaintySource.Individuals);
            var uncertaintySourceGenerators = new Dictionary<UncertaintySource, IRandom> {
                [UncertaintySource.Individuals] = random
            };

            TestRunUpdateSummarizeUncertainty(calculator, data, header, random, factorialSet, uncertaintySourceGenerators, reportFileName: "TestAcuteInternalAggregate");
        }

        [TestMethod]
        public void TargetExposuresActionCalculator_TestChronicOccupational() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = FakeSubstancesGenerator.Create(1);
            var individuals = FakeIndividualsGenerator.Create(
                200,
                2,
                randomSamplingWeight: random,
                useSamplingWeights: true,
                randomBodyWeight: random
            );
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);

            var routes = new[] { ExposureRoute.Dermal, ExposureRoute.Inhalation };

            var scenarios = FakeOccupationalExposuresGenerator.CreateScenarios([1, 2, 2], random);
            var occupationalScenarioExposures = FakeOccupationalExposuresGenerator.CreateOccupationalScenarioExposures(
                scenarios,
                routes,
                substances,
                SubstanceAmountUnit.Micrograms,
                isSystemic: true,
                random
            );
            var absorptionFactors = FakeAbsorptionFactorsGenerator.Create(
                routes,
                substances
            );

            var data = new ActionData() {
                ActiveSubstances = substances,
                Individuals = [.. individualDays.Cast<IIndividualDay>()],
                OccupationalScenarios = scenarios.ToDictionary(r => r.Code),
                OccupationalScenarioExposures = occupationalScenarioExposures,
                AbsorptionFactors = absorptionFactors
            };

            var project = new ProjectDto();
            var config = project.TargetExposuresSettings;
            config.ExposureType = ExposureType.Chronic;
            config.ExposureRoutes = [.. routes];
            config.ExposureSources = [ExposureSource.Occupational];
            config.TargetDoseLevelType = TargetLevelType.Systemic;
            config.IndividualReferenceSet = ReferenceIndividualSet.Individuals;
            config.Cumulative = false;
            var calculator = new TargetExposuresActionCalculator(project);
            var (header, _) = TestRunUpdateSummarizeNominal(project, calculator, data, "TestChronicOccupational");

            var factorialSet = new UncertaintyFactorialSet(UncertaintySource.Individuals);
            var uncertaintySourceGenerators = new Dictionary<UncertaintySource, IRandom> {
                [UncertaintySource.Individuals] = random
            };

            TestRunUpdateSummarizeUncertainty(calculator, data, header, random, factorialSet, uncertaintySourceGenerators, reportFileName: "TestAcuteInternalAggregate");
        }
    }
}
