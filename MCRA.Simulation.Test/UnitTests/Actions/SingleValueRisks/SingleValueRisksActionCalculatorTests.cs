using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Action.UncertaintyFactorial;
using MCRA.Simulation.Actions.SingleValueRisks;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using MCRA.Utils.Statistics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Actions {

    /// <summary>
    /// Tests the single value risks module.
    /// </summary>
    [TestClass]
    public class SingleValueRisksActionCalculatorTests : ActionCalculatorTestsBase {

        /// <summary>
        /// Runs the single value risks action as compute.
        /// config.SingleValueRiskCalculationMethod = SingleValueRiskCalculationMethod.FromSingleValues;
        /// </summary>
        [TestMethod]
        public void SingleValueRisksActionCalculator_SingleValue() {
            int seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = MockFoodsGenerator.CreateFoodsWithUnitWeights(5, random, fractionMissing: .1);
            var effects = MockEffectsGenerator.Create(1);
            var substances = MockSubstancesGenerator.Create(3);
            var exposures = MockSingleValueDietaryExposuresGenerator.Create(foods, substances, random);
            var hazardCharacterisationsUnit = TargetUnit.FromExternalExposureUnit(ExternalExposureUnit.ugPerKgBWPerDay);
            var hazardCharacterisationModelsCollections = MockHazardCharacterisationModelsGenerator
                .CreateSingle(effects.First(), substances, hazardCharacterisationsUnit, seed: seed);

            var data = new ActionData() {
                SingleValueDietaryExposureResults = exposures,
                SingleValueDietaryExposureUnit = hazardCharacterisationsUnit,
                HazardCharacterisationModelsCollections = hazardCharacterisationModelsCollections,
            };
            var project = new ProjectDto();
            var config = project.SingleValueRisksSettings;
            config.SingleValueRiskCalculationMethod = SingleValueRiskCalculationMethod.FromSingleValues;
            var calculator = new SingleValueRisksActionCalculator(project);
            _ = TestRunUpdateSummarizeNominal(project, calculator, data, $"SingleValue");

            Assert.AreEqual(data.SingleValueDietaryExposureResults.Count, data.SingleValueRiskCalculationResults.Count);
        }

        /// <summary>
        /// Runs the single value risks action as compute.
        /// config.SingleValueRiskCalculationMethod = SingleValueRiskCalculationMethod.FromIndividualRisks;
        /// config.RiskMetricType = RiskMetricType.HazardExposureRatio;
        /// config.IsInverseDistribution = false;
        /// </summary>
        [TestMethod]
        public void SingleValueRisksActionCalculator_TestAcuteMOENom() {
            int seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = MockSubstancesGenerator.Create(1);
            var individuals = MockIndividualsGenerator.Create(100, 1, random);
            var individualEffects = MockIndividualEffectsGenerator.Create(individuals, 0.1, random);

            var data = new ActionData() {
                ReferenceSubstance = substances[0],
                CumulativeIndividualEffects = individualEffects
            };
            var project = new ProjectDto();
            var config = project.SingleValueRisksSettings;
            config.SingleValueRiskCalculationMethod = SingleValueRiskCalculationMethod.FromIndividualRisks;
            config.RiskMetricType = RiskMetricType.HazardExposureRatio;
            config.IsInverseDistribution = false;

            var calculator = new SingleValueRisksActionCalculator(project);
            _ = TestRunUpdateSummarizeNominal(project, calculator, data, $"TestAcuteMOENom");
            Assert.AreEqual(1, data.SingleValueRiskCalculationResults.Count);
        }

        /// <summary>
        /// Runs the single value risks action as compute.
        /// config.SingleValueRiskCalculationMethod = SingleValueRiskCalculationMethod.FromIndividualRisks;
        /// config.RiskMetricType = RiskMetricType.HazardExposureRatio;
        /// config.IsInverseDistribution = true;
        /// </summary>
        [TestMethod]
        public void SingleValueRisksActionCalculator_TestAcuteMOEInvNom() {
            int seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = MockSubstancesGenerator.Create(1);
            var individuals = MockIndividualsGenerator.Create(100, 1, random);
            var individualEffects = MockIndividualEffectsGenerator.Create(individuals, 0.1, random);

            var data = new ActionData() {
                ReferenceSubstance = substances[0],
                CumulativeIndividualEffects = individualEffects
            };
            var project = new ProjectDto();
            var config = project.SingleValueRisksSettings;
            config.SingleValueRiskCalculationMethod = SingleValueRiskCalculationMethod.FromIndividualRisks;
            config.RiskMetricType = RiskMetricType.HazardExposureRatio;
            config.IsInverseDistribution = true;

            var calculator = new SingleValueRisksActionCalculator(project);
            _ = TestRunUpdateSummarizeNominal(project, calculator, data, $"TestAcuteMOEInvNom");
            Assert.AreEqual(1, data.SingleValueRiskCalculationResults.Count);
        }

        /// <summary>
        /// Runs the single value risks action as compute.
        /// config.SingleValueRiskCalculationMethod = SingleValueRiskCalculationMethod.FromIndividualRisks;
        /// config.RiskMetricType = RiskMetricType.ExposureHazardRatio;
        /// config.IsInverseDistribution = false;
        /// </summary>
        [TestMethod]
        public void SingleValueRisksActionCalculator_TestAcuteHINom() {
            int seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = MockSubstancesGenerator.Create(1);
            var individuals = MockIndividualsGenerator.Create(100, 1, random);
            var individualEffects = MockIndividualEffectsGenerator.Create(individuals, 0.1, random);

            var data = new ActionData() {
                ReferenceSubstance = substances[0],
                CumulativeIndividualEffects = individualEffects
            };
            var project = new ProjectDto();
            var config = project.SingleValueRisksSettings;
            config.SingleValueRiskCalculationMethod = SingleValueRiskCalculationMethod.FromIndividualRisks;
            config.RiskMetricType = RiskMetricType.ExposureHazardRatio;
            config.IsInverseDistribution = false;

            var calculator = new SingleValueRisksActionCalculator(project);
            _ = TestRunUpdateSummarizeNominal(project, calculator, data, $"TestAcuteHINom");
            Assert.AreEqual(1, data.SingleValueRiskCalculationResults.Count);
        }

        /// <summary>
        /// Runs the single value risks action as compute.
        /// config.SingleValueRiskCalculationMethod = SingleValueRiskCalculationMethod.FromIndividualRisks;
        /// config.RiskMetricType = RiskMetricType.ExposureHazardRatio;
        /// config.IsInverseDistribution = true;
        /// </summary>
        [TestMethod]
        public void SingleValueRisksActionCalculator_TestAcuteHIInvNom() {
            int seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = MockSubstancesGenerator.Create(1);
            var individuals = MockIndividualsGenerator.Create(100, 1, random);
            var individualEffects = MockIndividualEffectsGenerator.Create(individuals, 0.1, random);

            var data = new ActionData() {
                ReferenceSubstance = substances[0],
                CumulativeIndividualEffects = individualEffects
            };
            var project = new ProjectDto();
            var config = project.SingleValueRisksSettings;
            config.SingleValueRiskCalculationMethod = SingleValueRiskCalculationMethod.FromIndividualRisks;
            config.RiskMetricType = RiskMetricType.ExposureHazardRatio;
            config.IsInverseDistribution = true;

            var calculator = new SingleValueRisksActionCalculator(project);
            _ = TestRunUpdateSummarizeNominal(project, calculator, data, "TestAcuteHIInvNom");
            Assert.AreEqual(1, data.SingleValueRiskCalculationResults.Count);
        }

        /// <summary>
        /// Runs the single value risks action as compute.
        /// config.SingleValueRiskCalculationMethod = SingleValueRiskCalculationMethod.FromIndividualRisks;
        /// config.RiskMetricType = RiskMetricType.ExposureHazardRatio;
        /// config.IsInverseDistribution = true;
        /// config.UseAdjustmentFactors = true;
        /// </summary>
        [TestMethod]
        public void SingleValueRisksActionCalculator_TestAdjustmentFactorAcuteH() {
            int seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = MockSubstancesGenerator.Create(1);
            var individuals = MockIndividualsGenerator.Create(100, 1, random);
            var individualEffects = MockIndividualEffectsGenerator.Create(individuals, 0.1, random);

            var data = new ActionData() {
                ReferenceSubstance = substances[0],
                CumulativeIndividualEffects = individualEffects
            };
            var project = new ProjectDto();
            var config = project.SingleValueRisksSettings;
            config.SingleValueRiskCalculationMethod = SingleValueRiskCalculationMethod.FromIndividualRisks;
            config.RiskMetricType = RiskMetricType.ExposureHazardRatio;
            config.IsInverseDistribution = true;
            config.UseAdjustmentFactors = true;
            config.ExposureAdjustmentFactorDistributionMethod = AdjustmentFactorDistributionMethod.Beta;
            config.ExposureParameterA = 2;
            config.ExposureParameterB = 4;
            config.ExposureParameterC = .5;
            config.ExposureParameterD = 6;
            config.HazardAdjustmentFactorDistributionMethod = AdjustmentFactorDistributionMethod.Beta;
            config.HazardParameterA = 1.5;
            config.HazardParameterB = 3.5;
            config.HazardParameterC = .5;
            config.HazardParameterD = 3;

            var calculatorNom = new SingleValueRisksActionCalculator(project);
            _ = TestRunUpdateSummarizeNominal(project, calculatorNom, data, "TestAdjustmentFactorAcuteHINom");

            var calculator = new SingleValueRisksActionCalculator(project);
            var (header, _) = TestRunUpdateSummarizeNominal(project, calculator, data, null);
            Assert.AreEqual(1, data.SingleValueRiskCalculationResults.Count);

            var factorialSet = new UncertaintyFactorialSet(UncertaintySource.Concentrations, UncertaintySource.Individuals, UncertaintySource.SingleValueRiskAdjustmentFactors);
            var uncertaintySourceGenerators = factorialSet.UncertaintySources.ToDictionary(r => r, r => random as IRandom);
            TestRunUpdateSummarizeUncertainty(calculator, data, header, random, factorialSet, uncertaintySourceGenerators, reportFileName: "TestAdjustmentFactorAcuteHIUnc");
        }

        /// <summary>
        /// Runs the single value risks action as compute.
        /// config.SingleValueRiskCalculationMethod = SingleValueRiskCalculationMethod.FromIndividualRisks;
        /// config.RiskMetricType = RiskMetricType.HazardExposureRatio;
        /// config.IsInverseDistribution = true;
        /// config.UseAdjustmentFactors = true;
        /// </summary>
        [TestMethod]
        public void SingleValueRisksActionCalculator_TestAdjustmentFactorAcuteMOE() {
            int seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = MockSubstancesGenerator.Create(1);
            var individuals = MockIndividualsGenerator.Create(100, 1, random);
            var individualEffects = MockIndividualEffectsGenerator.Create(individuals, 0.1, random);

            var data = new ActionData() {
                ReferenceSubstance = substances[0],
                CumulativeIndividualEffects = individualEffects
            };
            var project = new ProjectDto();
            var config = project.SingleValueRisksSettings;
            config.SingleValueRiskCalculationMethod = SingleValueRiskCalculationMethod.FromIndividualRisks;
            config.RiskMetricType = RiskMetricType.HazardExposureRatio;
            config.IsInverseDistribution = true;
            config.UseAdjustmentFactors = true;
            config.ExposureAdjustmentFactorDistributionMethod = AdjustmentFactorDistributionMethod.Beta;
            config.ExposureParameterA = 2;
            config.ExposureParameterB = 4;
            config.ExposureParameterC = .5;
            config.ExposureParameterD = 6;
            config.HazardAdjustmentFactorDistributionMethod = AdjustmentFactorDistributionMethod.Beta;
            config.HazardParameterA = 1.5;
            config.HazardParameterB = 3.5;
            config.HazardParameterC = .5;
            config.HazardParameterD = 3;

            var calculatorNom = new SingleValueRisksActionCalculator(project);
            _ = TestRunUpdateSummarizeNominal(project, calculatorNom, data, "TestAdjustmentFactorAcuteMOENom");

            var calculator = new SingleValueRisksActionCalculator(project);
            var (header, _) = TestRunUpdateSummarizeNominal(project, calculator, data, null);
            Assert.AreEqual(1, data.SingleValueRiskCalculationResults.Count);

            var factorialSet = new UncertaintyFactorialSet(UncertaintySource.Concentrations, UncertaintySource.Individuals, UncertaintySource.SingleValueRiskAdjustmentFactors);
            var uncertaintySourceGenerators = factorialSet.UncertaintySources.ToDictionary(r => r, r => random as IRandom);
            TestRunUpdateSummarizeUncertainty(calculator, data, header, random, factorialSet, uncertaintySourceGenerators, reportFileName: "TestAdjustmentFactorAcuteMOEUnc");
        }

        /// <summary>
        /// Runs the single value risks action as compute.
        /// config.SingleValueRiskCalculationMethod = SingleValueRiskCalculationMethod.FromIndividualRisks;
        /// config.RiskMetricType = RiskMetricType.HazardExposureRatio;
        /// config.IsInverseDistribution = true;
        /// config.UseAdjustmentFactors = true;
        /// config.UseBackgroundAdjustmentFactor = true;
        /// config.ExposureType = ExposureType.Acute;
        /// </summary>
        [TestMethod]
        public void SingleValueRisksActionCalculator_TestAdjustmentFactorBackGroundAcuteMOE() {
            int seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = MockSubstancesGenerator.Create(5);
            var individuals = MockIndividualsGenerator.Create(100, 1, random);
            var individualEffects = MockIndividualEffectsGenerator.Create(individuals, 0.1, random);

            var foods = MockFoodsGenerator.Create(3);
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var dietaryIndividualDayIntakes = MockDietaryIndividualDayIntakeGenerator
                .Create(individualDays, foods, substances, 0.5, false, random, false);
            var correctedRelativePotencyFactors = substances.ToDictionary(c => c, c => 1d);
            var membershipProbabilities = substances.ToDictionary(c => c, c => 1d);
            var focalCommodityCombinations = new HashSet<(Food, Compound)> {
                (foods.First(), substances.First())
            };
            var data = new ActionData() {
                ReferenceSubstance = substances[0],
                CumulativeIndividualEffects = individualEffects,
                DietaryIndividualDayIntakes = dietaryIndividualDayIntakes,
                CorrectedRelativePotencyFactors = correctedRelativePotencyFactors,
                MembershipProbabilities = membershipProbabilities,
                FocalCommodityCombinations = focalCommodityCombinations
            };
            var project = new ProjectDto();
            var config = project.SingleValueRisksSettings;
            config.SingleValueRiskCalculationMethod = SingleValueRiskCalculationMethod.FromIndividualRisks;
            config.RiskMetricType = RiskMetricType.HazardExposureRatio;
            config.IsInverseDistribution = true;
            config.UseAdjustmentFactors = true;
            config.ExposureAdjustmentFactorDistributionMethod = AdjustmentFactorDistributionMethod.Beta;
            config.ExposureParameterA = 2;
            config.ExposureParameterB = 4;
            config.ExposureParameterC = .5;
            config.ExposureParameterD = 6;
            config.HazardAdjustmentFactorDistributionMethod = AdjustmentFactorDistributionMethod.Beta;
            config.HazardParameterA = 1.5;
            config.HazardParameterB = 3.5;
            config.HazardParameterC = .5;
            config.HazardParameterD = 3;
            config.UseBackgroundAdjustmentFactor = true;
            config.Percentage = 10;
            config.FocalCommodity = true;
            config.FocalCommodityReplacementMethod = FocalCommodityReplacementMethod.ReplaceSubstanceConcentrationsByLimitValue;
            config.ExposureType = ExposureType.Acute;

            var calculatorNom = new SingleValueRisksActionCalculator(project);
            _ = TestRunUpdateSummarizeNominal(project, calculatorNom, data, "TestAdjustmentFactorBackGroundAcuteMOENom");

            var calculator = new SingleValueRisksActionCalculator(project);
            var (header, _) = TestRunUpdateSummarizeNominal(project, calculator, data, null);
            Assert.AreEqual(1, data.SingleValueRiskCalculationResults.Count);

            var factorialSet = new UncertaintyFactorialSet(UncertaintySource.Concentrations, UncertaintySource.Individuals, UncertaintySource.SingleValueRiskAdjustmentFactors);
            var uncertaintySourceGenerators = factorialSet.UncertaintySources.ToDictionary(r => r, r => random as IRandom);
            TestRunUpdateSummarizeUncertainty(calculator, data, header, random, factorialSet, uncertaintySourceGenerators, reportFileName: "TestAdjustmentFactorBackGroundAcuteMOEUnc");
        }

        /// <summary>
        /// Runs the single value risks action as compute.
        /// config.SingleValueRiskCalculationMethod = SingleValueRiskCalculationMethod.FromIndividualRisks;
        /// config.RiskMetricType = RiskMetricType.HazardExposureRatio;
        /// config.IsInverseDistribution = true;
        /// config.UseAdjustmentFactors = true;
        /// config.UseBackgroundAdjustmentFactor = true;
        /// config.ExposureType = ExposureType.Chronic;
        /// </summary>
        [TestMethod]
        public void SingleValueRisksActionCalculator_TestAdjustmentFactorBackGroundChronicMOE() {
            int seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = MockSubstancesGenerator.Create(5);
            var individuals = MockIndividualsGenerator.Create(100, 1, random);
            var individualEffects = MockIndividualEffectsGenerator.Create(individuals, 0.1, random);

            var foods = MockFoodsGenerator.Create(3);
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var dietaryIndividualDayIntakes = MockDietaryIndividualDayIntakeGenerator
                .Create(individualDays, foods, substances, 0.5, false, random, false);
            var correctedRelativePotencyFactors = substances.ToDictionary(c => c, c => 1d);
            var membershipProbabilities = substances.ToDictionary(c => c, c => 1d);
            var focalCommodityCombinations = new HashSet<(Food, Compound)> {
                (foods.First(), substances.First())
            };
            var data = new ActionData() {
                ReferenceSubstance = substances[0],
                CumulativeIndividualEffects = individualEffects,
                DietaryIndividualDayIntakes = dietaryIndividualDayIntakes,
                CorrectedRelativePotencyFactors = correctedRelativePotencyFactors,
                MembershipProbabilities = membershipProbabilities,
                FocalCommodityCombinations = focalCommodityCombinations
            };
            var project = new ProjectDto();
            var config = project.SingleValueRisksSettings;
            config.SingleValueRiskCalculationMethod = SingleValueRiskCalculationMethod.FromIndividualRisks;
            config.RiskMetricType = RiskMetricType.HazardExposureRatio;
            config.IsInverseDistribution = true;
            config.UseAdjustmentFactors = true;
            config.ExposureAdjustmentFactorDistributionMethod = AdjustmentFactorDistributionMethod.Beta;
            config.ExposureParameterA = 2;
            config.ExposureParameterB = 4;
            config.ExposureParameterC = .5;
            config.ExposureParameterD = 6;
            config.HazardAdjustmentFactorDistributionMethod = AdjustmentFactorDistributionMethod.Beta;
            config.HazardParameterA = 1.5;
            config.HazardParameterB = 3.5;
            config.HazardParameterC = .5;
            config.HazardParameterD = 3;
            config.UseBackgroundAdjustmentFactor = true;
            config.Percentage = 10;
            config.FocalCommodity = true;
            config.FocalCommodityReplacementMethod = FocalCommodityReplacementMethod.ReplaceSubstanceConcentrationsByLimitValue;
            config.ExposureType = ExposureType.Chronic;

            var calculatorNom = new SingleValueRisksActionCalculator(project);
            _ = TestRunUpdateSummarizeNominal(project, calculatorNom, data, "TestAdjustmentFactorBackGroundChronicMOENom");

            var calculator = new SingleValueRisksActionCalculator(project);
            var (header, _) = TestRunUpdateSummarizeNominal(project, calculator, data, null);
            Assert.AreEqual(1, data.SingleValueRiskCalculationResults.Count);

            var factorialSet = new UncertaintyFactorialSet(UncertaintySource.Concentrations, UncertaintySource.Individuals, UncertaintySource.SingleValueRiskAdjustmentFactors);
            var uncertaintySourceGenerators = factorialSet.UncertaintySources.ToDictionary(r => r, r => random as IRandom);
            TestRunUpdateSummarizeUncertainty(calculator, data, header, random, factorialSet, uncertaintySourceGenerators, reportFileName: "TestAdjustmentFactorBackGroundChronicMOEUnc");
        }

        /// <summary>
        /// Runs the single value risks action as compute.
        /// config.SingleValueRiskCalculationMethod = SingleValueRiskCalculationMethod.FromIndividualRisks;
        /// config.RiskMetricType = RiskMetricType.ExposureHazardRatio;
        /// config.IsInverseDistribution = true;
        /// config.UseAdjustmentFactors = true;
        /// config.UseBackgroundAdjustmentFactor = true;
        /// config.ExposureType = ExposureType.Acute;
        /// </summary>
        [TestMethod]
        public void SingleValueRisksActionCalculator_TestAdjustmentFactorBackGroundAcuteHI() {
            int seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = MockSubstancesGenerator.Create(5);
            var individuals = MockIndividualsGenerator.Create(100, 1, random);
            var individualEffects = MockIndividualEffectsGenerator.Create(individuals, 0.1, random);

            var foods = MockFoodsGenerator.Create(3);
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var dietaryIndividualDayIntakes = MockDietaryIndividualDayIntakeGenerator
                .Create(individualDays, foods, substances, 0.5, false, random, false);
            var correctedRelativePotencyFactors = substances.ToDictionary(c => c, c => 1d);
            var membershipProbabilities = substances.ToDictionary(c => c, c => 1d);
            var focalCommodityCombinations = new HashSet<(Food, Compound)> {
                (foods.First(), substances.First())
            };
            var data = new ActionData() {
                ReferenceSubstance = substances[0],
                CumulativeIndividualEffects = individualEffects,
                DietaryIndividualDayIntakes = dietaryIndividualDayIntakes,
                CorrectedRelativePotencyFactors = correctedRelativePotencyFactors,
                MembershipProbabilities = membershipProbabilities,
                FocalCommodityCombinations = focalCommodityCombinations
            };
            var project = new ProjectDto();
            var config = project.SingleValueRisksSettings;
            config.SingleValueRiskCalculationMethod = SingleValueRiskCalculationMethod.FromIndividualRisks;
            config.RiskMetricType = RiskMetricType.ExposureHazardRatio;
            config.IsInverseDistribution = true;
            config.UseAdjustmentFactors = true;
            config.ExposureAdjustmentFactorDistributionMethod = AdjustmentFactorDistributionMethod.Beta;
            config.ExposureParameterA = 2;
            config.ExposureParameterB = 4;
            config.ExposureParameterC = .5;
            config.ExposureParameterD = 6;
            config.HazardAdjustmentFactorDistributionMethod = AdjustmentFactorDistributionMethod.Beta;
            config.HazardParameterA = 1.5;
            config.HazardParameterB = 3.5;
            config.HazardParameterC = .5;
            config.HazardParameterD = 3;
            config.UseBackgroundAdjustmentFactor = true;
            config.Percentage = 90;
            config.FocalCommodity = true;
            config.FocalCommodityReplacementMethod = FocalCommodityReplacementMethod.ReplaceSubstanceConcentrationsByLimitValue;
            config.ExposureType = ExposureType.Acute;

            var calculatorNom = new SingleValueRisksActionCalculator(project);
            _ = TestRunUpdateSummarizeNominal(project, calculatorNom, data, "TestAdjustmentFactorBackGroundAcuteHINom");

            var calculator = new SingleValueRisksActionCalculator(project);
            var (header, _) = TestRunUpdateSummarizeNominal(project, calculator, data, null);
            Assert.AreEqual(1, data.SingleValueRiskCalculationResults.Count);

            var factorialSet = new UncertaintyFactorialSet(UncertaintySource.Concentrations, UncertaintySource.Individuals, UncertaintySource.SingleValueRiskAdjustmentFactors);
            var uncertaintySourceGenerators = factorialSet.UncertaintySources.ToDictionary(r => r, r => random as IRandom);
            TestRunUpdateSummarizeUncertainty(calculator, data, header, random, factorialSet, uncertaintySourceGenerators, reportFileName: "TestAdjustmentFactorBackGroundAcuteHIUnc");
        }

        /// <summary>
        /// Runs the single value risks action as compute.
        /// config.SingleValueRiskCalculationMethod = SingleValueRiskCalculationMethod.FromIndividualRisks;
        /// config.RiskMetricType = RiskMetricType.ExposureHazardRatio;
        /// config.IsInverseDistribution = true;
        /// config.UseAdjustmentFactors = true;
        /// config.UseBackgroundAdjustmentFactor = true;
        /// config.ExposureType = ExposureType.Chronic;
        /// </summary>
        [TestMethod]
        public void SingleValueRisksActionCalculator_TestAdjustmentFactorBackGroundChronicHI() {
            int seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = MockSubstancesGenerator.Create(5);
            var individuals = MockIndividualsGenerator.Create(100, 1, random);
            var individualEffects = MockIndividualEffectsGenerator.Create(individuals, 0.1, random);

            var foods = MockFoodsGenerator.Create(3);
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var dietaryIndividualDayIntakes = MockDietaryIndividualDayIntakeGenerator
                .Create(individualDays, foods, substances, 0.5, false, random, false);
            var correctedRelativePotencyFactors = substances.ToDictionary(c => c, c => 1d);
            var membershipProbabilities = substances.ToDictionary(c => c, c => 1d);
            var focalCommodityCombinations = new HashSet<(Food, Compound)> {
                (foods.First(), substances.First())
            };
            var data = new ActionData() {
                ReferenceSubstance = substances[0],
                CumulativeIndividualEffects = individualEffects,
                DietaryIndividualDayIntakes = dietaryIndividualDayIntakes,
                CorrectedRelativePotencyFactors = correctedRelativePotencyFactors,
                MembershipProbabilities = membershipProbabilities,
                FocalCommodityCombinations = focalCommodityCombinations
            };

            var project = new ProjectDto();
            var config = project.SingleValueRisksSettings;
            config.SingleValueRiskCalculationMethod = SingleValueRiskCalculationMethod.FromIndividualRisks;
            config.RiskMetricType = RiskMetricType.ExposureHazardRatio;
            config.IsInverseDistribution = true;
            config.UseAdjustmentFactors = true;
            config.ExposureAdjustmentFactorDistributionMethod = AdjustmentFactorDistributionMethod.Beta;
            config.ExposureParameterA = 2;
            config.ExposureParameterB = 4;
            config.ExposureParameterC = .5;
            config.ExposureParameterD = 6;
            config.HazardAdjustmentFactorDistributionMethod = AdjustmentFactorDistributionMethod.Beta;
            config.HazardParameterA = 1.5;
            config.HazardParameterB = 3.5;
            config.HazardParameterC = .5;
            config.HazardParameterD = 3;
            config.UseBackgroundAdjustmentFactor = true;
            config.Percentage = 90;
            config.FocalCommodity = true;
            config.FocalCommodityReplacementMethod = FocalCommodityReplacementMethod.ReplaceSubstanceConcentrationsByLimitValue;
            config.ExposureType = ExposureType.Chronic;

            var calculatorNom = new SingleValueRisksActionCalculator(project);
            _ = TestRunUpdateSummarizeNominal(project, calculatorNom, data, "TestAdjustmentFactorBackGroundChronicHINom");

            var calculator = new SingleValueRisksActionCalculator(project);
            var (header, _) = TestRunUpdateSummarizeNominal(project, calculator, data, null);
            Assert.AreEqual(1, data.SingleValueRiskCalculationResults.Count);
            var factorialSet = new UncertaintyFactorialSet(UncertaintySource.Concentrations, UncertaintySource.Individuals, UncertaintySource.SingleValueRiskAdjustmentFactors);
            var uncertaintySourceGenerators = factorialSet.UncertaintySources.ToDictionary(r => r, r => random as IRandom);
            TestRunUpdateSummarizeUncertainty(calculator, data, header, random, factorialSet, uncertaintySourceGenerators, reportFileName: "TestAdjustmentFactorBackGroundChronicHIUnc");
        }
    }
}
