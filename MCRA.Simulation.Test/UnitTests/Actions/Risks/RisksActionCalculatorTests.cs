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
        /// Runs the Risks action: 
        ///  - external
        ///  - dietary
        ///  - single substance
        ///  - Acute and chronic
        ///  - MOE and HI
        /// </summary>
        [DataRow(ExposureType.Acute, RiskMetricType.MarginOfExposure)]
        [DataRow(ExposureType.Acute, RiskMetricType.HazardIndex)]
        [DataRow(ExposureType.Chronic, RiskMetricType.MarginOfExposure)]
        [DataRow(ExposureType.Chronic, RiskMetricType.HazardIndex)]
        [TestMethod]
        public void RisksActionCalculator_ExternalDietarySingleSubstance_ShouldGenerateReports(
            ExposureType exposureType,
            RiskMetricType riskMetricType
        ) {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = MockSubstancesGenerator.Create(1);
            var modelledFoods = MockFoodsGenerator.Create(3);
            var effects = MockEffectsGenerator.Create(1);
            var selectedEffect = effects.First();
            var hazardCharacterisations = MockHazardCharacterisationModelsGenerator
                .Create(selectedEffect, substances.ToList(), seed);
            var membershipProbabilities = substances.ToDictionary(r => r, r => 1d);
            var hazardCharacterisationsUnit = new TargetUnit(ExposureUnit.ugPerKgBWPerDay);
            var individuals = MockIndividualsGenerator.Create(25, 2, random, useSamplingWeights: true);
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var dietaryIndividualDayIntakes = MockDietaryIndividualDayIntakeGenerator.Create(individualDays, modelledFoods, substances, 0, true, random);

            var data = new ActionData() {
                ActiveSubstances = substances,
                SelectedEffect = selectedEffect,
                ReferenceSubstance = substances.First(),
                HazardCharacterisationModels = hazardCharacterisations,
                MembershipProbabilities = membershipProbabilities,
                HazardCharacterisationsUnit = hazardCharacterisationsUnit,
                DietaryIndividualDayIntakes = dietaryIndividualDayIntakes,
                ModelledFoods = modelledFoods
            };
            var project = new ProjectDto() {
                EffectModelSettings = new EffectModelSettingsDto() {
                    CalculateRisksByFood = true,
                    RiskMetricType = riskMetricType,
                    IsInverseDistribution = true,
                },
                EffectSettings = new EffectSettingsDto() {
                    TargetDoseLevelType = TargetLevelType.External
                },
                AssessmentSettings = new AssessmentSettingsDto() {
                    MultipleSubstances = false,
                    ExposureType = exposureType
                }
            };

            var calculatorNom = new RisksActionCalculator(project);
            _ = TestRunUpdateSummarizeNominal(
                project,
                calculatorNom,
                data,
                $"ExternalDietarySingleSubstance_{exposureType}_{riskMetricType}"
            );

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
                reportFileName: $"ExternalDietarySingleSubstance_{exposureType}_{riskMetricType}"
            );
        }

        /// <summary>
        /// Runs the Risks action: 
        ///  - external
        ///  - dietary
        ///  - cumulative
        ///  - Acute and chronic
        ///  - MOE and HI
        /// </summary>
        [DataRow(ExposureType.Acute, RiskMetricType.MarginOfExposure, RiskMetricCalculationType.RPFWeighted)]
        [DataRow(ExposureType.Acute, RiskMetricType.HazardIndex, RiskMetricCalculationType.RPFWeighted)]
        [DataRow(ExposureType.Chronic, RiskMetricType.MarginOfExposure, RiskMetricCalculationType.RPFWeighted)]
        [DataRow(ExposureType.Chronic, RiskMetricType.HazardIndex, RiskMetricCalculationType.RPFWeighted)]
        [DataRow(ExposureType.Acute, RiskMetricType.MarginOfExposure, RiskMetricCalculationType.SumRatios)]
        [DataRow(ExposureType.Acute, RiskMetricType.HazardIndex, RiskMetricCalculationType.SumRatios)]
        [DataRow(ExposureType.Chronic, RiskMetricType.MarginOfExposure, RiskMetricCalculationType.SumRatios)]
        [DataRow(ExposureType.Chronic, RiskMetricType.HazardIndex, RiskMetricCalculationType.SumRatios)]
        [TestMethod]
        public void RisksActionCalculator_ExternalDietaryCumulative_ShouldGenerateReports(
            ExposureType exposureType,
            RiskMetricType riskMetricType,
            RiskMetricCalculationType riskMetricCalculationType
        ) {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = MockSubstancesGenerator.Create(5);
            var modelledFoods = MockFoodsGenerator.Create(3);
            var effect = new Effect() { Code = "code" };
            var selectedEffect = effect;
            var hazardCharacterisations = MockHazardCharacterisationModelsGenerator.Create(effect, substances.ToList(), seed);
            var hcref = hazardCharacterisations.First().Value.Value;
            var correctedRelativePotencyFactors = hazardCharacterisations.ToDictionary(r => r.Key, r => hcref / r.Value.Value);
            var membershipProbabilities = substances.ToDictionary(r => r, r => 1d);
            var referenceCompound = substances.First();
            var hazardCharacterisationsUnit = new TargetUnit(ExposureUnit.ugPerKgBWPerDay);
            var individuals = MockIndividualsGenerator.Create(25, 2, random, useSamplingWeights: true);
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var dietaryIndividualDayIntakes = MockDietaryIndividualDayIntakeGenerator.Create(individualDays, modelledFoods, substances, 0, true, random);

            var data = new ActionData() {
                ActiveSubstances = substances,
                SelectedEffect = selectedEffect,
                HazardCharacterisationModels = hazardCharacterisations,
                CorrectedRelativePotencyFactors = correctedRelativePotencyFactors,
                MembershipProbabilities = membershipProbabilities,
                HazardCharacterisationsUnit = hazardCharacterisationsUnit,
                DietaryIndividualDayIntakes = dietaryIndividualDayIntakes,
                ReferenceSubstance = referenceCompound,
                ModelledFoods = modelledFoods
            };
            var project = new ProjectDto() {
                EffectModelSettings = new EffectModelSettingsDto() {
                    RiskMetricType = riskMetricType,
                    RiskMetricCalculationType = riskMetricCalculationType,
                    CumulativeRisk = true,
                    CalculateRisksByFood = true,
                    IsInverseDistribution = false,
                    ThresholdMarginOfExposure = 0.01
                },
                EffectSettings = new EffectSettingsDto() {
                    TargetDoseLevelType = TargetLevelType.External,
                },
                AssessmentSettings = new AssessmentSettingsDto() {
                    ExposureType = exposureType,
                    MultipleSubstances = true,
                },
            };

            var calculatorNom = new RisksActionCalculator(project);
            _ = TestRunUpdateSummarizeNominal(project, calculatorNom, data, "TestRiskAcuteExternalHI");

            var calculator = new RisksActionCalculator(project);
            var (header, result) = TestRunUpdateSummarizeNominal(
                project,
                calculator,
                data,
                $"ExternalDietaryCumulative_{exposureType}_{riskMetricType}"
            );

            // Assert cumulative HI at high percentile (e.g., p95) should be lower than
            // the sum of the substance HQs at that same percentile.
            var riskActionResult = result as RisksActionResult;
            var hiCumUpperPercentile = riskActionResult.IndividualEffects
                .Select(r => r.ExposureHazardRatio)
                .Percentile(95);
            var sumHiSubsUpperPercentile = riskActionResult.IndividualEffectsBySubstance
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
                reportFileName: $"ExternalDietaryCumulative_{exposureType}_{riskMetricType}"
            );
        }

        /// <summary>
        /// Runs the Risks action: 
        ///  - external
        ///  - dietary
        ///  - multiple substances
        ///  - no RPFs
        ///  - Acute and chronic
        ///  - MOE and HI
        /// </summary>
        [DataRow(ExposureType.Acute, RiskMetricType.MarginOfExposure)]
        [DataRow(ExposureType.Acute, RiskMetricType.HazardIndex)]
        [DataRow(ExposureType.Chronic, RiskMetricType.MarginOfExposure)]
        [DataRow(ExposureType.Chronic, RiskMetricType.HazardIndex)]
        [TestMethod]
        public void RisksActionCalculator_ExternalDietaryMultipleSubstanceNoRpfs_ShouldGenerateReports(
            ExposureType exposureType,
            RiskMetricType riskMetricType
        ) {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = MockSubstancesGenerator.Create(5);
            var modelledFoods = MockFoodsGenerator.Create(3);
            var effects = MockEffectsGenerator.Create(1);
            var selectedEffect = effects.First();
            var hazardCharacterisations = MockHazardCharacterisationModelsGenerator
                .Create(selectedEffect, substances.ToList(), seed);
            var membershipProbabilities = substances.ToDictionary(r => r, r => 1d);
            var hazardCharacterisationsUnit = new TargetUnit(ExposureUnit.ugPerKgBWPerDay);
            var individuals = MockIndividualsGenerator.Create(25, 2, random, useSamplingWeights: true);
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var dietaryIndividualDayIntakes = MockDietaryIndividualDayIntakeGenerator.Create(individualDays, modelledFoods, substances, 0, true, random);

            var data = new ActionData() {
                ActiveSubstances = substances,
                SelectedEffect = selectedEffect,
                HazardCharacterisationModels = hazardCharacterisations,
                MembershipProbabilities = membershipProbabilities,
                HazardCharacterisationsUnit = hazardCharacterisationsUnit,
                DietaryIndividualDayIntakes = dietaryIndividualDayIntakes,
                ModelledFoods = modelledFoods
            };
            var project = new ProjectDto() {
                EffectModelSettings = new EffectModelSettingsDto() {
                    CalculateRisksByFood = true,
                    RiskMetricType = riskMetricType,
                    IsInverseDistribution = true,
                },
                EffectSettings = new EffectSettingsDto() {
                    TargetDoseLevelType = TargetLevelType.External
                },
                AssessmentSettings = new AssessmentSettingsDto() {
                    MultipleSubstances = true,
                    ExposureType = exposureType
                }
            };

            var calculatorNom = new RisksActionCalculator(project);
            _ = TestRunUpdateSummarizeNominal(
                project,
                calculatorNom,
                data,
                $"ExternalDietaryMultipleSubstanceNoRpfs_{exposureType}_{riskMetricType}"
            );

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
                reportFileName: $"ExternalDietaryMultipleSubstanceNoRpfs_{exposureType}_{riskMetricType}"
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
            var effect = new Effect() { Code = "code" };
            var selectedEffect = effect;
            var hazardCharacterisations = MockHazardCharacterisationModelsGenerator.Create(effect, substances.ToList(), seed);
            var correctedRelativePotencyFactors = substances.ToDictionary(r => r, r => 1d);
            var membershipProbabilities = substances.ToDictionary(r => r, r => 1d);
            var referenceCompound = substances.First();
            var hazardCharacterisationsUnit = new TargetUnit(ExposureUnit.ugPerKgBWPerDay);
            var individuals = MockIndividualsGenerator.Create(25, 2, random, useSamplingWeights: true);
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var dietaryIndividualDayIntakes = MockDietaryIndividualDayIntakeGenerator.Create(individualDays, modelledFoods, substances, 0, true, random);

            var data = new ActionData() {
                ActiveSubstances = substances,
                SelectedEffect = selectedEffect,
                ConsumerIndividuals = individuals,
                SimulatedIndividualDays = individualDays,
                HazardCharacterisationModels = hazardCharacterisations,
                CorrectedRelativePotencyFactors = correctedRelativePotencyFactors,
                MembershipProbabilities = membershipProbabilities,
                HazardCharacterisationsUnit = hazardCharacterisationsUnit,
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
            var effect = new Effect() { Code = "code" };
            var selectedEffect = effect;
            var hazardCharacterisations = MockHazardCharacterisationModelsGenerator
                .Create(effect, substances.ToList(), seed);
            var correctedRelativePotencyFactors = substances.ToDictionary(r => r, r => 1d);
            var membershipProbabilities = substances.ToDictionary(r => r, r => 1d);
            var referenceCompound = substances.First();
            var hazardCharacterisationsUnit = new TargetUnit(ExposureUnit.ugPerKgBWPerDay);
            var individuals = MockIndividualsGenerator.Create(25, 2, random, useSamplingWeights: true);
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);

            // HBM
            var samplingMethod = FakeHbmDataGenerator.FakeHumanMonitoringSamplingMethod();
            var hbmIndividualConcentrations = FakeHbmDataGenerator.MockHumanMonitoringIndividualConcentrations(individuals, substances);
            var hbmIndividualDayConcentrations = FakeHbmDataGenerator.MockHumanMonitoringIndividualDayConcentrations(individualDays, substances, samplingMethod);

            var data = new ActionData() {
                ActiveSubstances = substances,
                SelectedEffect = selectedEffect,
                ConsumerIndividuals = individuals,
                SimulatedIndividualDays = individualDays,
                HazardCharacterisationModels = hazardCharacterisations,
                CorrectedRelativePotencyFactors = correctedRelativePotencyFactors,
                MembershipProbabilities = membershipProbabilities,
                HazardCharacterisationsUnit = hazardCharacterisationsUnit,
                ReferenceSubstance = referenceCompound,
                HbmIndividualDayCollections = new List<HbmIndividualDayCollection>() { new HbmIndividualDayCollection() { HbmIndividualDayConcentrations = hbmIndividualDayConcentrations } },
                HbmIndividualCollections = new List<HbmIndividualCollection>() { new HbmIndividualCollection() { HbmIndividualConcentrations = hbmIndividualConcentrations } },
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
                reportFileName: $"TestRiskInternalHbm_{exposureType}_{riskMetricType}");
        }
    }
}
