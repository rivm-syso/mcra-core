using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Action.UncertaintyFactorial;
using MCRA.Simulation.Actions.Risks;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Test.UnitTests.Actions {
    /// <summary>
    /// Runs the Risks action
    /// </summary>
    [TestClass]
    public class RisksActionCalculatorTests : ActionCalculatorTestsBase {

        /// <summary>
        /// Runs the risks action external dietary exposures of a single substance
        /// for various setting configurations.
        /// </summary>
        [DataRow(ExposureType.Acute, RiskMetricType.HazardExposureRatio, false)]
        [DataRow(ExposureType.Acute, RiskMetricType.ExposureHazardRatio, false)]
        [DataRow(ExposureType.Chronic, RiskMetricType.HazardExposureRatio, false)]
        [DataRow(ExposureType.Chronic, RiskMetricType.ExposureHazardRatio, false)]
        [DataRow(ExposureType.Acute, RiskMetricType.HazardExposureRatio, true)]
        [DataRow(ExposureType.Acute, RiskMetricType.ExposureHazardRatio, true)]
        [DataRow(ExposureType.Chronic, RiskMetricType.HazardExposureRatio, true)]
        [DataRow(ExposureType.Chronic, RiskMetricType.ExposureHazardRatio, true)]
        [TestMethod]
        public void RisksActionCalculator_FromDietarySingleSubstance_ShouldGenerateReports(
            ExposureType exposureType,
            RiskMetricType riskMetricType,
            bool isInverseDistribution
        ) {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = FakeSubstancesGenerator.Create(1);
            var modelledFoods = FakeFoodsGenerator.Create(3);
            var effect = FakeEffectsGenerator.Create(1).First();
            var dietaryExposureUnit = TargetUnit.FromExternalExposureUnit(ExternalExposureUnit.ugPerKgBWPerDay);
            var hazardCharacterisationModelsCollections = FakeHazardCharacterisationModelsGenerator
                .CreateSingle(effect, substances.ToList(), dietaryExposureUnit, seed);
            var individuals = FakeIndividualsGenerator.Create(25, 2, random, useSamplingWeights: true);
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var dietaryIndividualDayIntakes = FakeDietaryIndividualDayIntakeGenerator.Create(individualDays, modelledFoods, substances, 0, true, random);

            var data = new ActionData() {
                ActiveSubstances = substances,
                SelectedEffect = effect,
                HazardCharacterisationModelsCollections = hazardCharacterisationModelsCollections,
                DietaryIndividualDayIntakes = dietaryIndividualDayIntakes,
                ModelledFoods = modelledFoods,
                DietaryExposureUnit = dietaryExposureUnit
            };
            var project = new ProjectDto();
            var config = new RisksModuleConfig {
                CalculateRisksByFood = true,
                RiskMetricType = riskMetricType,
                IsInverseDistribution = true,
                ThresholdMarginOfExposure = riskMetricType == RiskMetricType.HazardExposureRatio ? 100 : 0.01,
                TargetDoseLevelType = TargetLevelType.External,
                MultipleSubstances = false,
                ExposureType = exposureType
            };
            project.SaveModuleConfiguration(config);

            var isInverseString = isInverseDistribution ? "_inverse" : string.Empty;
            var testId = $"FromDietarySingleSubstance_{exposureType}_{riskMetricType}{isInverseString}";
            var calculatorNom = new RisksActionCalculator(project);
            _ = TestRunUpdateSummarizeNominal(project, calculatorNom, data, testId);

            var calculator = new RisksActionCalculator(project);
            var (header, _) = TestRunUpdateSummarizeNominal(project, calculator, data, testId);
            var factorialSet = new UncertaintyFactorialSet(
                UncertaintySource.Concentrations,
                UncertaintySource.Individuals,
                UncertaintySource.Processing
            );
            var uncertaintySourceGenerators = new Dictionary<UncertaintySource, IRandom> {
                [UncertaintySource.Individuals] = random,
                [UncertaintySource.Processing] = random
            };
            TestRunUpdateSummarizeUncertainty(
                calculator: calculator,
                data: data,
                header: header,
                random: random,
                factorialSet: factorialSet,
                uncertaintySources: uncertaintySourceGenerators,
                reportFileName: testId
            );
        }

        /// <summary>
        /// Runs the risks action to compute cumulative risks from external dietary exposures
        /// using the sum of ratios approach for various setting configurations.
        /// </summary>
        [DataRow(ExposureType.Acute, RiskMetricType.HazardExposureRatio, false)]
        [DataRow(ExposureType.Acute, RiskMetricType.ExposureHazardRatio, false)]
        [DataRow(ExposureType.Chronic, RiskMetricType.HazardExposureRatio, false)]
        [DataRow(ExposureType.Chronic, RiskMetricType.ExposureHazardRatio, false)]
        [DataRow(ExposureType.Acute, RiskMetricType.HazardExposureRatio, true)]
        [DataRow(ExposureType.Acute, RiskMetricType.ExposureHazardRatio, true)]
        [DataRow(ExposureType.Chronic, RiskMetricType.HazardExposureRatio, true)]
        [DataRow(ExposureType.Chronic, RiskMetricType.ExposureHazardRatio, true)]
        [TestMethod]
        public void RisksActionCalculator_FromDietaryCumulativeSumOfRatios_ShouldGenerateReports(
            ExposureType exposureType,
            RiskMetricType riskMetricType,
            bool isInverseDistribution
        ) {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = FakeSubstancesGenerator.Create(5);
            var modelledFoods = FakeFoodsGenerator.Create(3);
            var effect = FakeEffectsGenerator.Create(1).First();
            var dietaryExposureUnit = TargetUnit.FromExternalExposureUnit(ExternalExposureUnit.ugPerKgBWPerDay);
            var hazardCharacterisationModelsCollections = FakeHazardCharacterisationModelsGenerator
                .CreateSingle(effect, substances.ToList(), dietaryExposureUnit, seed);
            var membershipProbabilities = substances.ToDictionary(r => r, r => 1d);
            var individuals = FakeIndividualsGenerator.Create(25, 2, random, useSamplingWeights: true);
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var dietaryIndividualDayIntakes = FakeDietaryIndividualDayIntakeGenerator
                .Create(individualDays, modelledFoods, substances, 0, true, random);

            var data = new ActionData() {
                ActiveSubstances = substances,
                SelectedEffect = effect,
                HazardCharacterisationModelsCollections = hazardCharacterisationModelsCollections,
                MembershipProbabilities = membershipProbabilities,
                DietaryIndividualDayIntakes = dietaryIndividualDayIntakes,
                ModelledFoods = modelledFoods,
                DietaryExposureUnit = dietaryExposureUnit
            };
            var project = new ProjectDto();
            var config = new RisksModuleConfig {
                RiskMetricType = riskMetricType,
                RiskMetricCalculationType = RiskMetricCalculationType.SumRatios,
                CumulativeRisk = true,
                CalculateRisksByFood = true,
                IsInverseDistribution = isInverseDistribution,
                ThresholdMarginOfExposure = riskMetricType == RiskMetricType.HazardExposureRatio ? 100 : 0.01,
                TargetDoseLevelType = TargetLevelType.External,
                ExposureType = exposureType,
                MultipleSubstances = true,
            };
            project.SaveModuleConfiguration(config);

            var isInverseString = isInverseDistribution ? "_inverse" : string.Empty;
            var testId = $"FromDietaryCumulativeSumOfRatios_{exposureType}_{riskMetricType}{isInverseString}";
            var calculatorNom = new RisksActionCalculator(project);
            _ = TestRunUpdateSummarizeNominal(project, calculatorNom, data, testId);

            var calculator = new RisksActionCalculator(project);
            var (header, result) = TestRunUpdateSummarizeNominal(project, calculator, data, testId);

            // Assert cumulative HI at high percentile (e.g., p95) should be lower than
            // the sum of the substance HQs at that same percentile.
            var riskActionResult = result as RisksActionResult;
            var hiCumUpperPercentile = riskActionResult.IndividualRisks
                .Select(r => r.ExposureHazardRatio)
                .Percentile(95);
            var sumHiSubsUpperPercentile = riskActionResult.IndividualEffectsBySubstanceCollections
                .FirstOrDefault()
                .IndividualEffects
                .Sum(r => r.Value
                    .Select(ihi => ihi.ExposureHazardRatio)
                    .Percentile(95)
                );
            Assert.IsLessThan(sumHiSubsUpperPercentile, hiCumUpperPercentile);

            var factorialSet = new UncertaintyFactorialSet(
                UncertaintySource.Concentrations,
                UncertaintySource.Individuals,
                UncertaintySource.Processing
            );
            var uncertaintySourceGenerators = new Dictionary<UncertaintySource, IRandom> {
                [UncertaintySource.Individuals] = random,
                [UncertaintySource.Processing] = random
            };
            TestRunUpdateSummarizeUncertainty(
                calculator: calculator,
                data: data,
                header: header,
                random: random,
                factorialSet: factorialSet,
                uncertaintySources: uncertaintySourceGenerators,
                reportFileName: testId
            );
        }

        /// <summary>
        /// Runs the risks action to compute cumulative risks from external dietary exposures
        /// using RPF weighing for various setting configurations.
        /// </summary>
        [DataRow(ExposureType.Acute, RiskMetricType.HazardExposureRatio, false, false)]
        [DataRow(ExposureType.Acute, RiskMetricType.ExposureHazardRatio, false, false)]
        [DataRow(ExposureType.Chronic, RiskMetricType.HazardExposureRatio, false, false)]
        [DataRow(ExposureType.Chronic, RiskMetricType.ExposureHazardRatio, false, false)]
        [DataRow(ExposureType.Acute, RiskMetricType.HazardExposureRatio, true, false)]
        [DataRow(ExposureType.Acute, RiskMetricType.ExposureHazardRatio, true, false)]
        [DataRow(ExposureType.Chronic, RiskMetricType.HazardExposureRatio, true, false)]
        [DataRow(ExposureType.Chronic, RiskMetricType.ExposureHazardRatio, true, false)]
        [DataRow(ExposureType.Acute, RiskMetricType.HazardExposureRatio, false, true)]
        [DataRow(ExposureType.Acute, RiskMetricType.ExposureHazardRatio, false, true)]
        [DataRow(ExposureType.Chronic, RiskMetricType.HazardExposureRatio, false, true)]
        [DataRow(ExposureType.Chronic, RiskMetricType.ExposureHazardRatio, false, true)]
        [DataRow(ExposureType.Acute, RiskMetricType.HazardExposureRatio, true, true)]
        [DataRow(ExposureType.Acute, RiskMetricType.ExposureHazardRatio, true, true)]
        [DataRow(ExposureType.Chronic, RiskMetricType.HazardExposureRatio, true, true)]
        [DataRow(ExposureType.Chronic, RiskMetricType.ExposureHazardRatio, true, true)]
        [TestMethod]
        public void RisksActionCalculator_FromDietaryCumulativeRpfWeighted_ShouldGenerateReports(
            ExposureType exposureType,
            RiskMetricType riskMetricType,
            bool onlyReference,
            bool isInverseDistribution
        ) {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = FakeSubstancesGenerator.Create(5);
            var referenceSubstance = substances.First();
            var effect = FakeEffectsGenerator.Create(1).First();
            var dietaryExposureUnit = TargetUnit.FromExternalExposureUnit(ExternalExposureUnit.ugPerKgBWPerDay);
            var hcSubstances = onlyReference ? [referenceSubstance] : substances;
            var hazardCharacterisationModelsCollections = FakeHazardCharacterisationModelsGenerator
                .CreateSingle(effect, hcSubstances, dietaryExposureUnit, seed);
            var referenceDose = hazardCharacterisationModelsCollections.First()
                .HazardCharacterisationModels[referenceSubstance];
            var correctedRelativePotencyFactors = onlyReference
                ? FakeRelativePotencyFactorsGenerator.Create(substances, referenceSubstance)
                    .ToDictionary(r => r.Compound, r => r.RPF)
                : hazardCharacterisationModelsCollections.First().HazardCharacterisationModels
                    .ToDictionary(r => r.Key, r => r.Value.Value / referenceDose.Value);
            var membershipProbabilities = substances
                .ToDictionary(r => r, r => 1d);
            var individuals = FakeIndividualsGenerator.CreateSimulated(25, 2, random, useSamplingWeights: true);
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var modelledFoods = FakeFoodsGenerator.Create(3);
            var dietaryIndividualDayIntakes = FakeDietaryIndividualDayIntakeGenerator
                .Create(individualDays, modelledFoods, substances, 0, true, random);

            var data = new ActionData() {
                ActiveSubstances = substances,
                SelectedEffect = effect,
                HazardCharacterisationModelsCollections = hazardCharacterisationModelsCollections,
                CorrectedRelativePotencyFactors = correctedRelativePotencyFactors,
                MembershipProbabilities = membershipProbabilities,
                DietaryIndividualDayIntakes = dietaryIndividualDayIntakes,
                ReferenceSubstance = referenceSubstance,
                ModelledFoods = modelledFoods,
                DietaryExposureUnit = dietaryExposureUnit
            };

            var project = new ProjectDto();
            var config = new RisksModuleConfig {
                RiskMetricType = riskMetricType,
                RiskMetricCalculationType = RiskMetricCalculationType.RPFWeighted,
                CumulativeRisk = true,
                CalculateRisksByFood = true,
                IsInverseDistribution = false,
                ThresholdMarginOfExposure = riskMetricType == RiskMetricType.HazardExposureRatio ? 100 : 0.01,
                TargetDoseLevelType = TargetLevelType.External,
                ExposureType = exposureType,
                MultipleSubstances = true,
            };
            project.SaveModuleConfiguration(config);

            var isInverseString = isInverseDistribution ? "_inverse" : string.Empty;
            var onlyReferenceString = onlyReference ? "_onlyRef" : "_allSubs";
            var testId = $"FromDietaryCumulativeRpfWeighted_{exposureType}_{riskMetricType}{onlyReferenceString}{isInverseString}";
            var calculatorNom = new RisksActionCalculator(project);
            _ = TestRunUpdateSummarizeNominal(project, calculatorNom, data, testId);

            var calculator = new RisksActionCalculator(project);
            var (header, result) = TestRunUpdateSummarizeNominal(project, calculator, data, testId);

            var factorialSet = new UncertaintyFactorialSet(
                UncertaintySource.Concentrations,
                UncertaintySource.Individuals,
                UncertaintySource.Processing
            );
            var uncertaintySourceGenerators = new Dictionary<UncertaintySource, IRandom> {
                [UncertaintySource.Individuals] = random,
                [UncertaintySource.Processing] = random
            };
            TestRunUpdateSummarizeUncertainty(
                calculator: calculator,
                data: data,
                header: header,
                random: random,
                factorialSet: factorialSet,
                uncertaintySources: uncertaintySourceGenerators,
                reportFileName: testId
            );
        }

        /// <summary>
        /// Runs the Risks action with multiple substances, but not cumulative for several
        /// settings configurations.
        /// </summary>
        [DataRow(ExposureType.Acute, RiskMetricType.HazardExposureRatio, false)]
        [DataRow(ExposureType.Acute, RiskMetricType.ExposureHazardRatio, false)]
        [DataRow(ExposureType.Chronic, RiskMetricType.HazardExposureRatio, false)]
        [DataRow(ExposureType.Chronic, RiskMetricType.ExposureHazardRatio, false)]
        [DataRow(ExposureType.Acute, RiskMetricType.HazardExposureRatio, true)]
        [DataRow(ExposureType.Acute, RiskMetricType.ExposureHazardRatio, true)]
        [DataRow(ExposureType.Chronic, RiskMetricType.HazardExposureRatio, true)]
        [DataRow(ExposureType.Chronic, RiskMetricType.ExposureHazardRatio, true)]
        [TestMethod]
        public void RisksActionCalculator_FromDietaryMultipleSubstanceNotCumulative_ShouldGenerateReports(
            ExposureType exposureType,
            RiskMetricType riskMetricType,
            bool isInverseDistribution
        ) {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = FakeSubstancesGenerator.Create(5);
            var modelledFoods = FakeFoodsGenerator.Create(3);
            var effect = FakeEffectsGenerator.Create(1).First();
            var dietaryExposureUnit = TargetUnit.FromExternalExposureUnit(ExternalExposureUnit.ugPerKgBWPerDay);
            var hazardCharacterisationModelsCollections = FakeHazardCharacterisationModelsGenerator
                .CreateSingle(effect, substances.ToList(), dietaryExposureUnit, seed);
            var membershipProbabilities = substances.ToDictionary(r => r, r => 1d);
            var individuals = FakeIndividualsGenerator.Create(25, 2, random, useSamplingWeights: true);
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var dietaryIndividualDayIntakes = FakeDietaryIndividualDayIntakeGenerator.Create(individualDays, modelledFoods, substances, 0, true, random);

            var data = new ActionData() {
                ActiveSubstances = substances,
                SelectedEffect = effect,
                HazardCharacterisationModelsCollections = hazardCharacterisationModelsCollections,
                MembershipProbabilities = membershipProbabilities,
                DietaryIndividualDayIntakes = dietaryIndividualDayIntakes,
                ModelledFoods = modelledFoods,
                DietaryExposureUnit = dietaryExposureUnit
            };
            var project = new ProjectDto();
            var config = new RisksModuleConfig {
                CalculateRisksByFood = true,
                RiskMetricType = riskMetricType,
                IsInverseDistribution = true,
                ThresholdMarginOfExposure = riskMetricType == RiskMetricType.HazardExposureRatio ? 100 : 0.01,
                TargetDoseLevelType = TargetLevelType.External,
                MultipleSubstances = true,
                ExposureType = exposureType
            };
            project.SaveModuleConfiguration(config);

            var isInverseString = isInverseDistribution ? "_inverse" : string.Empty;
            var testId = $"FromDietaryMultipleSubstanceNotCumulative{exposureType}_{riskMetricType}{isInverseString}";
            var calculatorNom = new RisksActionCalculator(project);
            _ = TestRunUpdateSummarizeNominal(project, calculatorNom, data, testId);

            var calculator = new RisksActionCalculator(project);
            var (header, _) = TestRunUpdateSummarizeNominal(project, calculator, data, null);
            var factorialSet = new UncertaintyFactorialSet(
                UncertaintySource.Concentrations,
                UncertaintySource.Individuals,
                UncertaintySource.Processing
            );
            var uncertaintySourceGenerators = new Dictionary<UncertaintySource, IRandom> {
                [UncertaintySource.Individuals] = random,
                [UncertaintySource.Processing] = random
            };
            TestRunUpdateSummarizeUncertainty(
                calculator: calculator,
                data: data,
                header: header,
                random: random,
                factorialSet: factorialSet,
                uncertaintySources: uncertaintySourceGenerators,
                reportFileName: testId
            );
        }

        /// <summary>
        /// Runs the Risks action: run, summarize action result, run uncertain, ummarize action result uncertain method, acute
        /// </summary>
        [TestMethod]
        public void RisksActionCalculator_TestRiskAcuteExternalIsEad() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = FakeSubstancesGenerator.Create(5);
            var modelledFoods = FakeFoodsGenerator.Create(3);
            var effect = FakeEffectsGenerator.Create(1).First();
            var dietaryExposureUnit = TargetUnit.FromExternalExposureUnit(ExternalExposureUnit.ugPerKgBWPerDay);
            var hazardCharacterisationModelsCollections = FakeHazardCharacterisationModelsGenerator
                .CreateSingle(effect, substances.ToList(), dietaryExposureUnit, seed);
            var correctedRelativePotencyFactors = substances.ToDictionary(r => r, r => 1d);
            var membershipProbabilities = substances.ToDictionary(r => r, r => 1d);
            var referenceCompound = substances.First();
            var individuals = FakeIndividualsGenerator.Create(25, 2, random, useSamplingWeights: true);
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var dietaryIndividualDayIntakes = FakeDietaryIndividualDayIntakeGenerator.Create(individualDays, modelledFoods, substances, 0, true, random);

            var data = new ActionData() {
                ActiveSubstances = substances,
                SelectedEffect = effect,
                ConsumerIndividuals = individuals,
                HazardCharacterisationModelsCollections = hazardCharacterisationModelsCollections,
                CorrectedRelativePotencyFactors = correctedRelativePotencyFactors,
                MembershipProbabilities = membershipProbabilities,
                DietaryIndividualDayIntakes = dietaryIndividualDayIntakes,
                ReferenceSubstance = referenceCompound,
                DietaryExposureUnit = dietaryExposureUnit
            };
            var project = new ProjectDto();
            var config = new RisksModuleConfig {
                CalculateRisksByFood = true,
                RiskMetricType = RiskMetricType.HazardExposureRatio,
                IsInverseDistribution = false,
                IsEAD = true,
                TargetDoseLevelType = TargetLevelType.External,
                MultipleSubstances = true
            };
            project.SaveModuleConfiguration(config);

            var calculatorNom = new RisksActionCalculator(project);
            _ = TestRunUpdateSummarizeNominal(project, calculatorNom, data, "TestRiskAcuteExternalIsEadNom");

            var calculator = new RisksActionCalculator(project);
            var (header, _) = TestRunUpdateSummarizeNominal(project, calculator, data, null);

            var factorialSet = new UncertaintyFactorialSet(
                UncertaintySource.Concentrations,
                UncertaintySource.Individuals,
                UncertaintySource.Processing
            );
            var uncertaintySourceGenerators = new Dictionary<UncertaintySource, IRandom> {
                [UncertaintySource.Individuals] = random,
                [UncertaintySource.Processing] = random
            };
            TestRunUpdateSummarizeUncertainty(
                calculator: calculator,
                data: data,
                header: header,
                random: random,
                factorialSet: factorialSet,
                uncertaintySources: uncertaintySourceGenerators,
                reportFileName: "TestRiskAcuteExternalIsEadUnc");
        }

        /// <summary>
        /// Runs the Risks action:
        ///  - internal
        ///  - HBM
        ///  - Acute and chronic
        /// </summary>
        [DataRow(ExposureType.Acute, RiskMetricType.HazardExposureRatio)]
        [DataRow(ExposureType.Acute, RiskMetricType.ExposureHazardRatio)]
        [DataRow(ExposureType.Chronic, RiskMetricType.HazardExposureRatio)]
        [DataRow(ExposureType.Chronic, RiskMetricType.ExposureHazardRatio)]
        [TestMethod]
        public void RisksActionCalculator_FromHbm_ShouldGenerateAcuteAndChronicReports(
            ExposureType exposureType,
            RiskMetricType riskMetricType
        ) {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = FakeSubstancesGenerator.Create(5);
            var effect = FakeEffectsGenerator.Create(1).First();
            var hazardCharacterisationsUnit = TargetUnit.FromExternalExposureUnit(ExternalExposureUnit.ugPerKgBWPerDay);
            var hazardCharacterisationModelsCollections = FakeHazardCharacterisationModelsGenerator
                .CreateSingle(effect, substances.ToList(), hazardCharacterisationsUnit, seed);
            var correctedRelativePotencyFactors = substances.ToDictionary(r => r, r => 1d);
            var membershipProbabilities = substances.ToDictionary(r => r, r => 1d);
            var referenceCompound = substances.First();
            var individuals = FakeIndividualsGenerator.Create(25, 2, random, useSamplingWeights: true);
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);

            // HBM
            var samplingMethod = FakeHbmDataGenerator.FakeHumanMonitoringSamplingMethod();
            var hbmIndividualConcentrations = FakeHbmDataGenerator.MockHumanMonitoringIndividualConcentrations(individuals, substances);
            var hbmIndividualDayConcentrations = FakeHbmDataGenerator.MockHumanMonitoringIndividualDayConcentrations(individualDays, substances, samplingMethod);

            var data = new ActionData() {
                ActiveSubstances = substances,
                SelectedEffect = effect,
                ConsumerIndividuals = individuals,
                HazardCharacterisationModelsCollections = hazardCharacterisationModelsCollections,
                MembershipProbabilities = membershipProbabilities,
                HbmIndividualDayCollections = [ new() {
                        TargetUnit = TargetUnit.FromExternalExposureUnit(ExternalExposureUnit.ugPerKgBWPerDay),
                        HbmIndividualDayConcentrations = hbmIndividualDayConcentrations
                    }
                ],
                HbmIndividualCollections = [ new() {
                        TargetUnit = TargetUnit.FromExternalExposureUnit(ExternalExposureUnit.ugPerKgBWPerDay),
                        HbmIndividualConcentrations = hbmIndividualConcentrations
                    }
                ],
            };

            var project = new ProjectDto();
            var config = new RisksModuleConfig {
                RiskMetricType = riskMetricType,
                RiskMetricCalculationType = RiskMetricCalculationType.SumRatios,
                CumulativeRisk = true,
                IsInverseDistribution = false,
                TargetDoseLevelType = TargetLevelType.Internal,
                ExposureType = exposureType,
                ExposureCalculationMethod = ExposureCalculationMethod.MonitoringConcentration,
                MultipleSubstances = true,
            };
            project.SaveModuleConfiguration(config);

            var calculatorNom = new RisksActionCalculator(project);
            (_, var resultNom) = TestRunUpdateSummarizeNominal(project, calculatorNom, data, $"FromHbm_SumOfRatios_{exposureType}_{riskMetricType}");

            var calculator = new RisksActionCalculator(project);
            var (header, _) = TestRunUpdateSummarizeNominal(project, calculator, data, null);
            var factorialSet = new UncertaintyFactorialSet(
                UncertaintySource.Concentrations,
                UncertaintySource.Individuals,
                UncertaintySource.Processing
            );
            var uncertaintySourceGenerators = new Dictionary<UncertaintySource, IRandom> {
                [UncertaintySource.Individuals] = random,
                [UncertaintySource.Processing] = random
            };
            TestRunUpdateSummarizeUncertainty(
                calculator: calculator,
                data: data,
                header: header,
                random: random,
                factorialSet: factorialSet,
                uncertaintySources: uncertaintySourceGenerators,
                reportFileName: $"FromHbm_SumOfRatios_{exposureType}_{riskMetricType}"
            );

            var risksActionResultNom = resultNom as RisksActionResult;
            Assert.IsNotNull(risksActionResultNom);
            Assert.IsNotNull(risksActionResultNom.IndividualEffectsBySubstanceCollections.Count == 1);
            Assert.HasCount(substances.Count, risksActionResultNom.IndividualEffectsBySubstanceCollections.First().IndividualEffects);
        }

        /// <summary>
        /// Runs the Risks action, internal HBM, for cumulative, RPF-weighted, without complete hazard data
        /// </summary>
        [DataRow(ExposureType.Acute, RiskMetricType.HazardExposureRatio)]
        [DataRow(ExposureType.Acute, RiskMetricType.ExposureHazardRatio)]
        [DataRow(ExposureType.Chronic, RiskMetricType.HazardExposureRatio)]
        [DataRow(ExposureType.Chronic, RiskMetricType.ExposureHazardRatio)]
        [TestMethod]
        public void RisksActionCalculator_FromlHbm_CumulativeRpfWeightedNoFullHazardData_ShouldGenerateIndividualeffectsBySubstance(
            ExposureType exposureType,
            RiskMetricType riskMetricType
        ) {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = FakeSubstancesGenerator.Create(5);
            var referenceCompound = substances.First();
            var effect = FakeEffectsGenerator.Create(1).First();
            var targetUnit = TargetUnit.FromInternalDoseUnit(DoseUnit.ugPerL, BiologicalMatrix.Blood);
            var hazardCharacterisationModelsCollections = FakeHazardCharacterisationModelsGenerator
                .CreateSingle(effect, [referenceCompound], targetUnit, seed: seed, ageDependent: true);
            var correctedRelativePotencyFactors = substances.ToDictionary(r => r, r => 1d);
            var membershipProbabilities = substances.ToDictionary(r => r, r => 1d);
            var individuals = FakeIndividualsGenerator.Create(25, 2, random, useSamplingWeights: true);
            FakeIndividualsGenerator.AddFakeAgeProperty(individuals, random);
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);

            // HBM
            var samplingMethod = FakeHbmDataGenerator.FakeHumanMonitoringSamplingMethod();
            var hbmIndividualConcentrations = FakeHbmDataGenerator.MockHumanMonitoringIndividualConcentrations(individuals, substances);
            var hbmIndividualDayConcentrations = FakeHbmDataGenerator.MockHumanMonitoringIndividualDayConcentrations(individualDays, substances, samplingMethod);

            var data = new ActionData() {
                ActiveSubstances = substances,
                SelectedEffect = effect,
                ConsumerIndividuals = individuals,
                HazardCharacterisationModelsCollections = hazardCharacterisationModelsCollections,
                CorrectedRelativePotencyFactors = correctedRelativePotencyFactors,
                MembershipProbabilities = membershipProbabilities,
                ReferenceSubstance = referenceCompound,
                HbmIndividualDayCollections = [ new() {
                        TargetUnit = targetUnit,
                        HbmIndividualDayConcentrations = hbmIndividualDayConcentrations
                    }
                ],
                HbmIndividualCollections = [ new() {
                        TargetUnit = targetUnit,
                        HbmIndividualConcentrations = hbmIndividualConcentrations
                    }
                ],
            };

            var project = new ProjectDto();
            var config = new RisksModuleConfig {
                RiskMetricType = riskMetricType,
                RiskMetricCalculationType = RiskMetricCalculationType.RPFWeighted,
                CumulativeRisk = true,
                TargetDoseLevelType = TargetLevelType.Internal,
                ExposureType = exposureType,
                ExposureCalculationMethod = ExposureCalculationMethod.MonitoringConcentration,
                MultipleSubstances = true,
            };
            project.SaveModuleConfiguration(config);

            var calculatorNom = new RisksActionCalculator(project);
            var (headerNom, resultNom) = TestRunUpdateSummarizeNominal(project, calculatorNom, data, $"FromHbm_RpfWeighted_{exposureType}_{riskMetricType}");

            var calculator = new RisksActionCalculator(project);
            var (header, _) = TestRunUpdateSummarizeNominal(project, calculator, data, null);
            var factorialSet = new UncertaintyFactorialSet(
                UncertaintySource.Concentrations,
                UncertaintySource.Individuals,
                UncertaintySource.Processing
            );
            var uncertaintySourceGenerators = new Dictionary<UncertaintySource, IRandom> {
                [UncertaintySource.Individuals] = random,
                [UncertaintySource.Processing] = random
            };
            TestRunUpdateSummarizeUncertainty(
                calculator: calculator,
                data: data,
                header: header,
                random: random,
                factorialSet: factorialSet,
                uncertaintySources: uncertaintySourceGenerators,
                reportFileName: $"FromHbm_RpfWeighted_{exposureType}_{riskMetricType}"
            );

            var risksActionResultNom = resultNom as RisksActionResult;
            Assert.IsNotNull(risksActionResultNom);
            Assert.IsNotNull(risksActionResultNom.IndividualEffectsBySubstanceCollections.Count == 1);
            Assert.HasCount(substances.Count, risksActionResultNom.IndividualEffectsBySubstanceCollections.First().IndividualEffects);
        }
    }
}
