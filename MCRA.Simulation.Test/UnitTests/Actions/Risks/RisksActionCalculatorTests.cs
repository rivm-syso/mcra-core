using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Action.UncertaintyFactorial;
using MCRA.Simulation.Actions.Risks;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualConcentrationCalculation;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualDayConcentrationCalculation;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using MCRA.Utils.Statistics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
        [DataRow(ExposureType.Acute, RiskMetricType.MarginOfExposure, false)]
        [DataRow(ExposureType.Acute, RiskMetricType.HazardIndex, false)]
        [DataRow(ExposureType.Chronic, RiskMetricType.MarginOfExposure, false)]
        [DataRow(ExposureType.Chronic, RiskMetricType.HazardIndex, false)]
        [DataRow(ExposureType.Acute, RiskMetricType.MarginOfExposure, true)]
        [DataRow(ExposureType.Acute, RiskMetricType.HazardIndex, true)]
        [DataRow(ExposureType.Chronic, RiskMetricType.MarginOfExposure, true)]
        [DataRow(ExposureType.Chronic, RiskMetricType.HazardIndex, true)]
        [TestMethod]
        public void RisksActionCalculator_FromDietarySingleSubstance_ShouldGenerateReports(
            ExposureType exposureType,
            RiskMetricType riskMetricType,
            bool isInverseDistribution
        ) {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = MockSubstancesGenerator.Create(1);
            var modelledFoods = MockFoodsGenerator.Create(3);
            var effect = MockEffectsGenerator.Create(1).First();
            var hazardCharacterisationsUnit = TargetUnit.FromExternalExposureUnit(ExternalExposureUnit.ugPerKgBWPerDay);
            var hazardCharacterisationModelsCollections = MockHazardCharacterisationModelsGenerator
                .CreateSingle(effect, substances.ToList(), hazardCharacterisationsUnit, seed);
            var individuals = MockIndividualsGenerator.Create(25, 2, random, useSamplingWeights: true);
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var dietaryIndividualDayIntakes = MockDietaryIndividualDayIntakeGenerator.Create(individualDays, modelledFoods, substances, 0, true, random);

            var data = new ActionData() {
                ActiveSubstances = substances,
                SelectedEffect = effect,
                HazardCharacterisationModelsCollections = hazardCharacterisationModelsCollections,
                DietaryIndividualDayIntakes = dietaryIndividualDayIntakes,
                ModelledFoods = modelledFoods
            };
            var project = new ProjectDto() {
                EffectModelSettings = new EffectModelSettingsDto() {
                    CalculateRisksByFood = true,
                    RiskMetricType = riskMetricType,
                    IsInverseDistribution = true,
                    ThresholdMarginOfExposure = riskMetricType == RiskMetricType.MarginOfExposure ? 100 : 0.01
                },
                EffectSettings = new EffectSettingsDto() {
                    TargetDoseLevelType = TargetLevelType.External
                },
                AssessmentSettings = new AssessmentSettingsDto() {
                    MultipleSubstances = false,
                    ExposureType = exposureType
                }
            };

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
        [DataRow(ExposureType.Acute, RiskMetricType.MarginOfExposure, false)]
        [DataRow(ExposureType.Acute, RiskMetricType.HazardIndex, false)]
        [DataRow(ExposureType.Chronic, RiskMetricType.MarginOfExposure, false)]
        [DataRow(ExposureType.Chronic, RiskMetricType.HazardIndex, false)]
        [DataRow(ExposureType.Acute, RiskMetricType.MarginOfExposure, true)]
        [DataRow(ExposureType.Acute, RiskMetricType.HazardIndex, true)]
        [DataRow(ExposureType.Chronic, RiskMetricType.MarginOfExposure, true)]
        [DataRow(ExposureType.Chronic, RiskMetricType.HazardIndex, true)]
        [TestMethod]
        public void RisksActionCalculator_FromDietaryCumulativeSumOfRatios_ShouldGenerateReports(
            ExposureType exposureType,
            RiskMetricType riskMetricType,
            bool isInverseDistribution
        ) {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = MockSubstancesGenerator.Create(5);
            var modelledFoods = MockFoodsGenerator.Create(3);
            var effect = MockEffectsGenerator.Create(1).First();
            var targetUnit = TargetUnit.FromExternalExposureUnit(ExternalExposureUnit.ugPerKgBWPerDay);
            var hazardCharacterisationModelsCollections = MockHazardCharacterisationModelsGenerator
                .CreateSingle(effect, substances.ToList(), targetUnit, seed);
            var membershipProbabilities = substances.ToDictionary(r => r, r => 1d);
            var individuals = MockIndividualsGenerator.Create(25, 2, random, useSamplingWeights: true);
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var dietaryIndividualDayIntakes = MockDietaryIndividualDayIntakeGenerator
                .Create(individualDays, modelledFoods, substances, 0, true, random);

            var data = new ActionData() {
                ActiveSubstances = substances,
                SelectedEffect = effect,
                HazardCharacterisationModelsCollections = hazardCharacterisationModelsCollections,
                MembershipProbabilities = membershipProbabilities,
                DietaryIndividualDayIntakes = dietaryIndividualDayIntakes,
                ModelledFoods = modelledFoods
            };
            var project = new ProjectDto() {
                EffectModelSettings = new EffectModelSettingsDto() {
                    RiskMetricType = riskMetricType,
                    RiskMetricCalculationType = RiskMetricCalculationType.SumRatios,
                    CumulativeRisk = true,
                    CalculateRisksByFood = true,
                    IsInverseDistribution = isInverseDistribution,
                    ThresholdMarginOfExposure = riskMetricType == RiskMetricType.MarginOfExposure ? 100 : 0.01
                },
                EffectSettings = new EffectSettingsDto() {
                    TargetDoseLevelType = TargetLevelType.External,
                },
                AssessmentSettings = new AssessmentSettingsDto() {
                    ExposureType = exposureType,
                    MultipleSubstances = true,
                },
            };

            var isInverseString = isInverseDistribution ? "_inverse" : string.Empty;
            var testId = $"FromDietaryCumulativeSumOfRatios_{exposureType}_{riskMetricType}{isInverseString}";
            var calculatorNom = new RisksActionCalculator(project);
            _ = TestRunUpdateSummarizeNominal(project, calculatorNom, data, testId);

            var calculator = new RisksActionCalculator(project);
            var (header, result) = TestRunUpdateSummarizeNominal(project, calculator, data, testId);

            // Assert cumulative HI at high percentile (e.g., p95) should be lower than
            // the sum of the substance HQs at that same percentile.
            var riskActionResult = result as RisksActionResult;
            var hiCumUpperPercentile = riskActionResult.IndividualEffects
                .Select(r => r.ExposureHazardRatio)
                .Percentile(95);
            var sumHiSubsUpperPercentile = riskActionResult.IndividualEffectsBySubstanceCollections
                .FirstOrDefault()
                .IndividualEffects
                .Sum(r => r.Value
                    .Select(ihi => ihi.ExposureHazardRatio)
                    .Percentile(95)
                );
            Assert.IsTrue(hiCumUpperPercentile < sumHiSubsUpperPercentile);

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
        [DataRow(ExposureType.Acute, RiskMetricType.MarginOfExposure, false, false)]
        [DataRow(ExposureType.Acute, RiskMetricType.HazardIndex, false, false)]
        [DataRow(ExposureType.Chronic, RiskMetricType.MarginOfExposure, false, false)]
        [DataRow(ExposureType.Chronic, RiskMetricType.HazardIndex, false, false)]
        [DataRow(ExposureType.Acute, RiskMetricType.MarginOfExposure, true, false)]
        [DataRow(ExposureType.Acute, RiskMetricType.HazardIndex, true, false)]
        [DataRow(ExposureType.Chronic, RiskMetricType.MarginOfExposure, true, false)]
        [DataRow(ExposureType.Chronic, RiskMetricType.HazardIndex, true, false)]
        [DataRow(ExposureType.Acute, RiskMetricType.MarginOfExposure, false, true)]
        [DataRow(ExposureType.Acute, RiskMetricType.HazardIndex, false, true)]
        [DataRow(ExposureType.Chronic, RiskMetricType.MarginOfExposure, false, true)]
        [DataRow(ExposureType.Chronic, RiskMetricType.HazardIndex, false, true)]
        [DataRow(ExposureType.Acute, RiskMetricType.MarginOfExposure, true, true)]
        [DataRow(ExposureType.Acute, RiskMetricType.HazardIndex, true, true)]
        [DataRow(ExposureType.Chronic, RiskMetricType.MarginOfExposure, true, true)]
        [DataRow(ExposureType.Chronic, RiskMetricType.HazardIndex, true, true)]
        [TestMethod]
        public void RisksActionCalculator_FromDietaryCumulativeRpfWeighted_ShouldGenerateReports(
            ExposureType exposureType,
            RiskMetricType riskMetricType,
            bool onlyReference,
            bool isInverseDistribution
        ) {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = MockSubstancesGenerator.Create(5);
            var referenceSubstance = substances.First();
            var effect = MockEffectsGenerator.Create(1).First();
            var targetUnit = TargetUnit.FromExternalExposureUnit(ExternalExposureUnit.ugPerKgBWPerDay);
            var hcSubstances = onlyReference ? new List<Compound>() { referenceSubstance } : substances;
            var hazardCharacterisationModelsCollections = MockHazardCharacterisationModelsGenerator
                .CreateSingle(effect, hcSubstances, targetUnit, seed);
            var referenceDose = hazardCharacterisationModelsCollections.First()
                .HazardCharacterisationModels[referenceSubstance];
            var correctedRelativePotencyFactors = onlyReference
                ? MockRelativePotencyFactorsGenerator.Create(substances, referenceSubstance)
                    .ToDictionary(r => r.Compound, r => r.RPF.Value)
                : hazardCharacterisationModelsCollections.First().HazardCharacterisationModels
                    .ToDictionary(r => r.Key, r => r.Value.Value / referenceDose.Value);
            var membershipProbabilities = substances
                .ToDictionary(r => r, r => 1d);
            var individuals = MockIndividualsGenerator.Create(25, 2, random, useSamplingWeights: true);
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var modelledFoods = MockFoodsGenerator.Create(3);
            var dietaryIndividualDayIntakes = MockDietaryIndividualDayIntakeGenerator
                .Create(individualDays, modelledFoods, substances, 0, true, random);

            var data = new ActionData() {
                ActiveSubstances = substances,
                SelectedEffect = effect,
                HazardCharacterisationModelsCollections = hazardCharacterisationModelsCollections,
                CorrectedRelativePotencyFactors = correctedRelativePotencyFactors,
                MembershipProbabilities = membershipProbabilities,
                DietaryIndividualDayIntakes = dietaryIndividualDayIntakes,
                ReferenceSubstance = referenceSubstance,
                ModelledFoods = modelledFoods
            };
            var project = new ProjectDto() {
                EffectModelSettings = new EffectModelSettingsDto() {
                    RiskMetricType = riskMetricType,
                    RiskMetricCalculationType = RiskMetricCalculationType.RPFWeighted,
                    CumulativeRisk = true,
                    CalculateRisksByFood = true,
                    IsInverseDistribution = false,
                    ThresholdMarginOfExposure = riskMetricType == RiskMetricType.MarginOfExposure ? 100 : 0.01
                },
                EffectSettings = new EffectSettingsDto() {
                    TargetDoseLevelType = TargetLevelType.External,
                },
                AssessmentSettings = new AssessmentSettingsDto() {
                    ExposureType = exposureType,
                    MultipleSubstances = true,
                },
            };

            var isInverseString = isInverseDistribution ? "_inverse" : string.Empty;
            var onlyReferenceString = onlyReference ? "_onlyRef" : "_allSubs";
            var testId = $"FromDietaryCumulativeRpfWeighted_{exposureType}_{riskMetricType}{onlyReferenceString}{isInverseString}";
            var calculatorNom = new RisksActionCalculator(project);
            _ = TestRunUpdateSummarizeNominal(project,calculatorNom, data, testId);

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
        [DataRow(ExposureType.Acute, RiskMetricType.MarginOfExposure, false)]
        [DataRow(ExposureType.Acute, RiskMetricType.HazardIndex, false)]
        [DataRow(ExposureType.Chronic, RiskMetricType.MarginOfExposure, false)]
        [DataRow(ExposureType.Chronic, RiskMetricType.HazardIndex, false)]
        [DataRow(ExposureType.Acute, RiskMetricType.MarginOfExposure, true)]
        [DataRow(ExposureType.Acute, RiskMetricType.HazardIndex, true)]
        [DataRow(ExposureType.Chronic, RiskMetricType.MarginOfExposure, true)]
        [DataRow(ExposureType.Chronic, RiskMetricType.HazardIndex, true)]
        [TestMethod]
        public void RisksActionCalculator_FromDietaryMultipleSubstanceNotCumulative_ShouldGenerateReports(
            ExposureType exposureType,
            RiskMetricType riskMetricType,
            bool isInverseDistribution
        ) {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = MockSubstancesGenerator.Create(5);
            var modelledFoods = MockFoodsGenerator.Create(3);
            var effect = MockEffectsGenerator.Create(1).First();
            var hazardCharacterisationsUnit = TargetUnit.FromExternalExposureUnit(ExternalExposureUnit.ugPerKgBWPerDay);
            var hazardCharacterisationModelsCollections = MockHazardCharacterisationModelsGenerator
                .CreateSingle(effect, substances.ToList(), hazardCharacterisationsUnit, seed);
            var membershipProbabilities = substances.ToDictionary(r => r, r => 1d);
            var individuals = MockIndividualsGenerator.Create(25, 2, random, useSamplingWeights: true);
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var dietaryIndividualDayIntakes = MockDietaryIndividualDayIntakeGenerator.Create(individualDays, modelledFoods, substances, 0, true, random);

            var data = new ActionData() {
                ActiveSubstances = substances,
                SelectedEffect = effect,
                HazardCharacterisationModelsCollections = hazardCharacterisationModelsCollections,
                MembershipProbabilities = membershipProbabilities,
                DietaryIndividualDayIntakes = dietaryIndividualDayIntakes,
                ModelledFoods = modelledFoods
            };
            var project = new ProjectDto() {
                EffectModelSettings = new EffectModelSettingsDto() {
                    CalculateRisksByFood = true,
                    RiskMetricType = riskMetricType,
                    IsInverseDistribution = true,
                    ThresholdMarginOfExposure = riskMetricType == RiskMetricType.MarginOfExposure ? 100 : 0.01
                },
                EffectSettings = new EffectSettingsDto() {
                    TargetDoseLevelType = TargetLevelType.External
                },
                AssessmentSettings = new AssessmentSettingsDto() {
                    MultipleSubstances = true,
                    ExposureType = exposureType
                }
            };

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
            var substances = MockSubstancesGenerator.Create(5);
            var modelledFoods = MockFoodsGenerator.Create(3);
            var effect = MockEffectsGenerator.Create(1).First();
            var hazardCharacterisationsUnit = TargetUnit.FromExternalExposureUnit(ExternalExposureUnit.ugPerKgBWPerDay);
            var hazardCharacterisationModelsCollections = MockHazardCharacterisationModelsGenerator
                .CreateSingle(effect, substances.ToList(), hazardCharacterisationsUnit, seed);
            var correctedRelativePotencyFactors = substances.ToDictionary(r => r, r => 1d);
            var membershipProbabilities = substances.ToDictionary(r => r, r => 1d);
            var referenceCompound = substances.First();
            var individuals = MockIndividualsGenerator.Create(25, 2, random, useSamplingWeights: true);
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var dietaryIndividualDayIntakes = MockDietaryIndividualDayIntakeGenerator.Create(individualDays, modelledFoods, substances, 0, true, random);

            var data = new ActionData() {
                ActiveSubstances = substances,
                SelectedEffect = effect,
                ConsumerIndividuals = individuals,
                SimulatedIndividualDays = individualDays,
                HazardCharacterisationModelsCollections = hazardCharacterisationModelsCollections,
                CorrectedRelativePotencyFactors = correctedRelativePotencyFactors,
                MembershipProbabilities = membershipProbabilities,
                DietaryIndividualDayIntakes = dietaryIndividualDayIntakes,
                ReferenceSubstance = referenceCompound
            };
            var project = new ProjectDto() {
                EffectModelSettings = new EffectModelSettingsDto() {
                    CalculateRisksByFood = true,
                    RiskMetricType = RiskMetricType.MarginOfExposure,
                    IsInverseDistribution = false,
                },
                EffectSettings = new EffectSettingsDto() {
                    TargetDoseLevelType = TargetLevelType.External
                }
            };
            project.EffectModelSettings.IsEAD = true;

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
        [DataRow(ExposureType.Acute, RiskMetricType.MarginOfExposure)]
        [DataRow(ExposureType.Acute, RiskMetricType.HazardIndex)]
        [DataRow(ExposureType.Chronic, RiskMetricType.MarginOfExposure)]
        [DataRow(ExposureType.Chronic, RiskMetricType.HazardIndex)]
        [TestMethod]
        public void RisksActionCalculator_InternalHbm_ShouldGenerateAcuteAndChronicReports(
            ExposureType exposureType,
            RiskMetricType riskMetricType
        ) {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = MockSubstancesGenerator.Create(5);
            var effect = MockEffectsGenerator.Create(1).First();
            var hazardCharacterisationsUnit = TargetUnit.FromExternalExposureUnit(ExternalExposureUnit.ugPerKgBWPerDay);
            var hazardCharacterisationModelsCollections = MockHazardCharacterisationModelsGenerator
                .CreateSingle(effect, substances.ToList(), hazardCharacterisationsUnit, seed);
            var correctedRelativePotencyFactors = substances.ToDictionary(r => r, r => 1d);
            var membershipProbabilities = substances.ToDictionary(r => r, r => 1d);
            var referenceCompound = substances.First();
            var individuals = MockIndividualsGenerator.Create(25, 2, random, useSamplingWeights: true);
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);

            // HBM
            var samplingMethod = FakeHbmDataGenerator.FakeHumanMonitoringSamplingMethod();
            var hbmIndividualConcentrations = FakeHbmDataGenerator.MockHumanMonitoringIndividualConcentrations(individuals, substances);
            var hbmIndividualDayConcentrations = FakeHbmDataGenerator.MockHumanMonitoringIndividualDayConcentrations(individualDays, substances, samplingMethod);

            var data = new ActionData() {
                ActiveSubstances = substances,
                SelectedEffect = effect,
                ConsumerIndividuals = individuals,
                SimulatedIndividualDays = individualDays,
                HazardCharacterisationModelsCollections = hazardCharacterisationModelsCollections,
                CorrectedRelativePotencyFactors = correctedRelativePotencyFactors,
                MembershipProbabilities = membershipProbabilities,
                ReferenceSubstance = referenceCompound,
                HbmIndividualDayCollections = new List<HbmIndividualDayCollection>() { new HbmIndividualDayCollection() {
                        TargetUnit = TargetUnit.FromExternalExposureUnit(ExternalExposureUnit.ugPerKgBWPerDay),
                        HbmIndividualDayConcentrations = hbmIndividualDayConcentrations
                    }
                },
                HbmIndividualCollections = new List<HbmIndividualCollection>() { new HbmIndividualCollection() {
                        TargetUnit = TargetUnit.FromExternalExposureUnit(ExternalExposureUnit.ugPerKgBWPerDay),
                        HbmIndividualConcentrations = hbmIndividualConcentrations
                    }
                },
            };

            var project = new ProjectDto() {
                EffectModelSettings = new EffectModelSettingsDto() {
                    RiskMetricType = riskMetricType,
                    CumulativeRisk = true,
                    IsInverseDistribution = false,
                },
                EffectSettings = new EffectSettingsDto() {
                    TargetDoseLevelType = TargetLevelType.Internal
                },
                AssessmentSettings = new AssessmentSettingsDto() {
                    ExposureType = exposureType,
                    InternalConcentrationType = InternalConcentrationType.MonitoringConcentration
                }
            };

            var calculatorNom = new RisksActionCalculator(project);
            _ = TestRunUpdateSummarizeNominal(project, calculatorNom, data, "TestRiskInternalHbmNom");

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
                reportFileName: $"TestRiskInternalHbm_{exposureType}_{riskMetricType}"
            );
        }
    }
}
