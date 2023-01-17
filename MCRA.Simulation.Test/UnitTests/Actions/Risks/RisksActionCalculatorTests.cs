using System.Collections.Generic;
using System.Linq;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Action.Settings.Dto;
using MCRA.Simulation.Action.UncertaintyFactorial;
using MCRA.Simulation.Actions.Risks;
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
        /// Runs the Risks action: run, summarize action result, run uncertain, summarize action result uncertain method, acute 
        /// </summary>
        [TestMethod]
        public void RisksActionCalculator_TestRiskAcuteExternalHI() {
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
                HazardCharacterisations = hazardCharacterisations,
                CorrectedRelativePotencyFactors = correctedRelativePotencyFactors,
                MembershipProbabilities = membershipProbabilities,
                HazardCharacterisationsUnit = hazardCharacterisationsUnit,
                DietaryIndividualDayIntakes = dietaryIndividualDayIntakes,
                ReferenceCompound = referenceCompound,
                ModelledFoods =  modelledFoods
            };
            var project = new ProjectDto() { 
                EffectModelSettings = new EffectModelSettingsDto() {
                    CalculateRisksByFood =  true,
                    RiskMetricType = RiskMetricType.HazardIndex,
                    IsInverseDistribution =  false,
                    ThresholdMarginOfExposure = 0.01
                },
                EffectSettings  = new EffectSettingsDto() {
                    TargetDoseLevelType = TargetLevelType.External,
                },
                AssessmentSettings = new AssessmentSettingsDto() {
                    MultipleSubstances = true,
                    Cumulative = true
                },
            };

            var calculatorNom = new RisksActionCalculator(project);
            _ = TestRunUpdateSummarizeNominal(project, calculatorNom, data, "TestRiskAcuteExternalHI");

            var calculator = new RisksActionCalculator(project);
            var (header, result) = TestRunUpdateSummarizeNominal(project, calculator, data, "TestRiskAcuteExternalHI");

            // Assert cumulative HI at high percentile (e.g., p95) should be lower than
            // the sum of the substance HQs at that same percentile.
            var riskActionResult = result as RisksActionResult;
            var hiCumUpperPercentile = riskActionResult.CumulativeIndividualEffects
                .Select(r => r.HazardIndex(HealthEffectType.Risk))
                .Percentile(95);
            var sumHiSubsUpperPercentile = riskActionResult.IndividualEffectsBySubstance
                .Sum(r => r.Value
                    .Select(ihi => ihi.HazardIndex(HealthEffectType.Risk))
                    .Percentile(95)
                );
            Assert.IsTrue(hiCumUpperPercentile < sumHiSubsUpperPercentile);

            var factorialSet = new UncertaintyFactorialSet(
                UncertaintySource.Concentrations,
                UncertaintySource.Individuals,
                UncertaintySource.Processing
            );
            var uncertaintySourceGenerators = new Dictionary<UncertaintySource, IRandom>();
            uncertaintySourceGenerators[UncertaintySource.Individuals] = random;
            uncertaintySourceGenerators[UncertaintySource.Processing] = random;
            TestRunUpdateSummarizeUncertainty(
                calculator: calculator,
                data: data,
                header: header,
                random: random,
                factorialSet: factorialSet,
                uncertaintySources: uncertaintySourceGenerators,
                reportFileName: "TestRiskAcuteExternalHIUnc");
        }

        /// <summary>
        /// Runs the Risks action: run, summarize action result, run uncertain,
        /// summarize action result uncertain method, acute, inverse distribution.
        /// </summary>
        [TestMethod]
        public void RisksActionCalculator_TestAcuteNoRpfs() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = MockSubstancesGenerator.Create(5);
            var modelledFoods = MockFoodsGenerator.Create(3);
            var effect = new Effect() { Code = "code" };
            var selectedEffect = effect;
            var hazardCharacterisations = MockHazardCharacterisationModelsGenerator.Create(effect, substances.ToList(), seed);
            var referenceCompound = substances.First();
            var correctedRelativePotencyFactors = substances.ToDictionary(r => r, r => 1d);
            var membershipProbabilities = substances.ToDictionary(r => r, r => 1d);
            var hazardCharacterisationsUnit = new TargetUnit(ExposureUnit.ugPerKgBWPerDay);
            var individuals = MockIndividualsGenerator.Create(25, 2, random, useSamplingWeights: true);
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var dietaryIndividualDayIntakes = MockDietaryIndividualDayIntakeGenerator.Create(individualDays, modelledFoods, substances, 0, true, random);

            var data = new ActionData() {
                ActiveSubstances = substances,
                SelectedEffect = selectedEffect,
                HazardCharacterisations = hazardCharacterisations,
                CorrectedRelativePotencyFactors = correctedRelativePotencyFactors,
                MembershipProbabilities = membershipProbabilities,
                HazardCharacterisationsUnit = hazardCharacterisationsUnit,
                DietaryIndividualDayIntakes = dietaryIndividualDayIntakes,
                ReferenceCompound = referenceCompound,
                ModelledFoods = modelledFoods
            };
            var project = new ProjectDto() {
                EffectModelSettings = new EffectModelSettingsDto() {
                    CalculateRisksByFood = true,
                    RiskMetricType = RiskMetricType.MarginOfExposure,
                    IsInverseDistribution = true,
                },
                EffectSettings = new EffectSettingsDto() {
                    TargetDoseLevelType = TargetLevelType.External
                }
            };

            var calculatorNom = new RisksActionCalculator(project);
            _ = TestRunUpdateSummarizeNominal(project, calculatorNom, data, "TestRiskAcuteExternalMoeInverseDistributionNom");

            var calculator = new RisksActionCalculator(project);
            var (header, _) = TestRunUpdateSummarizeNominal(project, calculator, data, null);
            var factorialSet = new UncertaintyFactorialSet(
                UncertaintySource.Concentrations,
                UncertaintySource.Individuals,
                UncertaintySource.Processing
            );
            var uncertaintySourceGenerators = new Dictionary<UncertaintySource, IRandom>();
            uncertaintySourceGenerators[UncertaintySource.Individuals] = random;
            uncertaintySourceGenerators[UncertaintySource.Processing] = random;
            TestRunUpdateSummarizeUncertainty(
                calculator: calculator,
                data: data,
                header: header, 
                random: random,
                factorialSet: factorialSet,
                uncertaintySources: uncertaintySourceGenerators,
                reportFileName: "TestRiskAcuteExternalMoeInverseDistributionUnc");
        }

        /// <summary>
        /// Runs the Risks action: run, summarize action result, run uncertain, summarize
        /// action result uncertain method, chronic. Hazard index
        /// project.AssessmentSettings.ExposureType = ExposureType.Chronic;
        /// </summary>
        [TestMethod]
        public void RisksActionCalculator_TestRiskChronicExternalMOE() {
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
                HazardCharacterisations = hazardCharacterisations,
                CorrectedRelativePotencyFactors = correctedRelativePotencyFactors,
                MembershipProbabilities = membershipProbabilities,
                HazardCharacterisationsUnit = hazardCharacterisationsUnit,
                DietaryIndividualDayIntakes = dietaryIndividualDayIntakes,
                ReferenceCompound = referenceCompound,
                ModelledFoods = modelledFoods
            };
            var project = new ProjectDto() {
                EffectModelSettings = new EffectModelSettingsDto() {
                    CalculateRisksByFood = true,
                    RiskMetricType = RiskMetricType.MarginOfExposure,
                    IsInverseDistribution = false,
                },
                EffectSettings = new EffectSettingsDto() {
                    TargetDoseLevelType = TargetLevelType.External
                },
                AssessmentSettings = new AssessmentSettingsDto() {
                    ExposureType = ExposureType.Chronic
                }
            };
            var calculatorNom = new RisksActionCalculator(project);
            _ = TestRunUpdateSummarizeNominal(project, calculatorNom, data, "TestRiskChronicExternalHiNom");
            var calculator = new RisksActionCalculator(project);
            var (header, _) = TestRunUpdateSummarizeNominal(project, calculator, data, null);
            var factorialSet = new UncertaintyFactorialSet(
                UncertaintySource.Concentrations,
                UncertaintySource.Individuals,
                UncertaintySource.Processing
            );
            var uncertaintySourceGenerators = new Dictionary<UncertaintySource, IRandom>();
            uncertaintySourceGenerators[UncertaintySource.Individuals] = random;
            uncertaintySourceGenerators[UncertaintySource.Processing] = random;
            TestRunUpdateSummarizeUncertainty(
                calculator: calculator,
                data: data,
                header: header,
                random: random,
                factorialSet: factorialSet,
                uncertaintySources: uncertaintySourceGenerators,
                reportFileName: "TestRiskChronicExternalHiUnc");
        }

        /// <summary>
        /// Runs the Risks action: run, summarize action result, run uncertain, summarize
        /// action result uncertain method, chronic. 
        /// project.AssessmentSettings.ExposureType = ExposureType.Chronic;
        /// project.EffectModelSettings.RiskMetricType = RiskMetricType.HazardIndex;
        /// </summary>
        [TestMethod]
        public void RisksActionCalculator_TestRiskChronicExternalHI() {
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
                HazardCharacterisations = hazardCharacterisations,
                CorrectedRelativePotencyFactors = correctedRelativePotencyFactors,
                MembershipProbabilities = membershipProbabilities,
                HazardCharacterisationsUnit = hazardCharacterisationsUnit,
                DietaryIndividualDayIntakes = dietaryIndividualDayIntakes,
                ReferenceCompound = referenceCompound,
                ModelledFoods = modelledFoods
            };
            var project = new ProjectDto() {
                EffectModelSettings = new EffectModelSettingsDto() {
                    CalculateRisksByFood = true,
                    RiskMetricType = RiskMetricType.HazardIndex,
                    IsInverseDistribution = true,
                },
                EffectSettings = new EffectSettingsDto() {
                    TargetDoseLevelType = TargetLevelType.External
                },
                AssessmentSettings = new AssessmentSettingsDto() { 
                    ExposureType = ExposureType.Chronic
                }
            };
            var calculatorNom = new RisksActionCalculator(project);
            _ = TestRunUpdateSummarizeNominal(project, calculatorNom, data, "TestRiskChronicExternalHIInverseDistributionNom");

            var calculator = new RisksActionCalculator(project);
            var (header, _) = TestRunUpdateSummarizeNominal(project, calculator, data, null);
            var factorialSet = new UncertaintyFactorialSet(
                UncertaintySource.Concentrations,
                UncertaintySource.Individuals,
                UncertaintySource.Processing
            );
            var uncertaintySourceGenerators = new Dictionary<UncertaintySource, IRandom>();
            uncertaintySourceGenerators[UncertaintySource.Individuals] = random;
            uncertaintySourceGenerators[UncertaintySource.Processing] = random;
            TestRunUpdateSummarizeUncertainty(
                calculator: calculator,
                data: data,
                header: header,
                random: random,
                factorialSet: factorialSet,
                uncertaintySources: uncertaintySourceGenerators,
                reportFileName: "TestRiskChronicExternalHIInverseDistributionUnc");
        }
        /// <summary>
        /// Runs the Risks action: run, summarize action result, run uncertain, summarize
        /// action result uncertain method,
        /// project.AssessmentSettings.ExposureType = ExposureType.Chronic;
        /// project.EffectModelSettings.RiskMetricType = RiskMetricType.HazardIndex
        /// 
        /// </summary>
        [TestMethod]
        public void RisksActionCalculator_TestRiskChronicExternalHINoRPFs() {
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
                HazardCharacterisations = hazardCharacterisations,
                CorrectedRelativePotencyFactors = correctedRelativePotencyFactors,
                MembershipProbabilities = membershipProbabilities,
                HazardCharacterisationsUnit = hazardCharacterisationsUnit,
                DietaryIndividualDayIntakes = dietaryIndividualDayIntakes,
                ReferenceCompound = referenceCompound,
            };
            var project = new ProjectDto() {
                EffectModelSettings = new EffectModelSettingsDto() {
                    CalculateRisksByFood = false,
                    RiskMetricType = RiskMetricType.HazardIndex,
                    IsInverseDistribution = false,
                },
                EffectSettings = new EffectSettingsDto() {
                    TargetDoseLevelType = TargetLevelType.External
                },
                AssessmentSettings = new AssessmentSettingsDto() {
                    ExposureType = ExposureType.Chronic
                }
            };
            project.EffectModelSettings.RiskMetricType = RiskMetricType.HazardIndex;

            var calculatorNom = new RisksActionCalculator(project);
            _ = TestRunUpdateSummarizeNominal(project, calculatorNom, data, "TestRiskChronicExternalHICalcFoodsNom");

            var calculator = new RisksActionCalculator(project);
            var (header, _) = TestRunUpdateSummarizeNominal(project, calculator, data, null);

            var factorialSet = new UncertaintyFactorialSet(
                UncertaintySource.Concentrations,
                UncertaintySource.Individuals,
                UncertaintySource.Processing
            );
            var uncertaintySourceGenerators = new Dictionary<UncertaintySource, IRandom>();
            uncertaintySourceGenerators[UncertaintySource.Individuals] = random;
            uncertaintySourceGenerators[UncertaintySource.Processing] = random;
            TestRunUpdateSummarizeUncertainty(
                calculator: calculator,
                data: data,
                header: header,
                random: random,
                factorialSet: factorialSet,
                uncertaintySources: uncertaintySourceGenerators,
                reportFileName: "TestRiskChronicExternalHICalcFoodssUnc");
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
                HazardCharacterisations = hazardCharacterisations,
                CorrectedRelativePotencyFactors = correctedRelativePotencyFactors,
                MembershipProbabilities = membershipProbabilities,
                HazardCharacterisationsUnit = hazardCharacterisationsUnit,
                DietaryIndividualDayIntakes = dietaryIndividualDayIntakes,
                ReferenceCompound = referenceCompound,
                ModelledFoods = modelledFoods
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
            var uncertaintySourceGenerators = new Dictionary<UncertaintySource, IRandom>();
            uncertaintySourceGenerators[UncertaintySource.Individuals] = random;
            uncertaintySourceGenerators[UncertaintySource.Processing] = random;
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
                HazardCharacterisations = hazardCharacterisations,
                CorrectedRelativePotencyFactors = correctedRelativePotencyFactors,
                MembershipProbabilities = membershipProbabilities,
                HazardCharacterisationsUnit = hazardCharacterisationsUnit,
                ReferenceCompound = referenceCompound,
                HbmIndividualDayConcentrations = hbmIndividualDayConcentrations,
                HbmIndividualConcentrations = hbmIndividualConcentrations,
                HbmTargetConcentrationUnit = new TargetUnit(ExposureUnit.ugPerKgBWPerDay),
            };
            var project = new ProjectDto() {
                EffectModelSettings = new EffectModelSettingsDto() {
                    RiskMetricType = riskMetricType,
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
            var uncertaintySourceGenerators = new Dictionary<UncertaintySource, IRandom>();
            uncertaintySourceGenerators[UncertaintySource.Individuals] = random;
            uncertaintySourceGenerators[UncertaintySource.Processing] = random;
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
