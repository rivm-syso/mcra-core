using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Action.UncertaintyFactorial;
using MCRA.Simulation.Actions.ConcentrationModels;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Test.UnitTests.Actions {
    /// <summary>
    /// Runs the ConcentrationModels action
    /// </summary>
    [TestClass]
    public class ConcentrationModelsActionCalculatorTests : ActionCalculatorTestsBase {

        /// <summary>
        /// Runs the ConcentrationModels action:
        /// project.ConcentrationModelSettings.IsSampleBased = true;
        /// project.AssessmentSettings.Cumulative = true;
        /// </summary>
        [TestMethod]
        public void ConcentrationModelsActionCalculator_TestCompute() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var modelledFoods = FakeFoodsGenerator.Create(3);
            var substances = FakeSubstancesGenerator.Create(5);
            var referenceCompound = substances.First();
            var concentrationModels = FakeConcentrationsModelsGenerator.Create(modelledFoods, substances, nonDetectsHandlingMethod: NonDetectsHandlingMethod.ReplaceByLOR);
            var activeSubstanceSampleCollections = FakeSampleCompoundCollectionsGenerator
                .Create(
                    modelledFoods,
                    substances,
                    concentrationModels
                );
            var correctedRelativePotencyFactors = substances.ToDictionary(c => c, c => 1d);

            var project = new ProjectDto();
            var settings = new ConcentrationModelsModuleConfig {
                IsSampleBased = true,
                NonDetectsHandlingMethod = NonDetectsHandlingMethod.ReplaceByLOR,
                FractionOfLor = .5,
                Cumulative = true
            };
            project.SaveModuleConfiguration(settings);

            var data = new ActionData() {
                ModelledFoods = modelledFoods,
                ModelledSubstances = substances,
                ReferenceSubstance = referenceCompound,
                ActiveSubstanceSampleCollections = activeSubstanceSampleCollections,
                CorrectedRelativePotencyFactors = correctedRelativePotencyFactors
            };
            var calculator = new ConcentrationModelsActionCalculator(project);

            var (header, _) = TestRunUpdateSummarizeNominal(project, calculator, data, $"ConcentrationModels1");
            var factorialSet = new UncertaintyFactorialSet(UncertaintySource.Concentrations, UncertaintySource.ConcentrationNonDetectImputation);
            var uncertaintySourceGenerators = factorialSet.UncertaintySources.ToDictionary(r => r, r => random as IRandom);
            TestRunUpdateSummarizeUncertainty(calculator, data, header, random, factorialSet, uncertaintySourceGenerators);
        }

        /// <summary>
        /// Runs the ConcentrationModels action: IsSampleBased = true
        /// project.ConcentrationModelSettings.IsSampleBased = true;
        /// project.ConcentrationModelSettings.ImputeMissingValues = true;
        /// project.AssessmentSettings.Cumulative = true;
        /// project.ScenarioAnalysisSettings.IsMrlSettingScenario = true;
        /// project.ScenarioAnalysisSettings.UseFrequency = .8;
        /// project.ScenarioAnalysisSettings.ConcentrationModelType = ConcentrationModelType.MaximumResidueLimit;
        /// </summary>
        [TestMethod]
        public void ConcentrationModelsActionCalculator_TestComputeFocalScenario() {

            var modelledFoods = FakeFoodsGenerator.Create(3);
            var substances = FakeSubstancesGenerator.Create(5);
            var focalFood = modelledFoods.First();
            var focalSubstance = substances.First();
            var concentrationModels = FakeConcentrationsModelsGenerator.Create(modelledFoods, substances, sampleSize: 3000);
            concentrationModels.Remove((focalFood, focalSubstance));
            var activeSubstanceSampleCollections = FakeSampleCompoundCollectionsGenerator.Create(
                modelledFoods,
                substances,
                concentrationModels
            );

            var correctedRelativePotencyFactors = substances.ToDictionary(c => c, c => 1d);
            var maximumResidueLimits = new Dictionary<(Food, Compound), ConcentrationLimit> {
                [(focalFood, focalSubstance)] = new ConcentrationLimit() {
                    Compound = focalSubstance,
                    Food = focalFood,
                    Limit = 12,
                }
            };

            var concentrationModelsConfig = new ConcentrationModelsModuleConfig {
                DefaultConcentrationModel = ConcentrationModelType.MaximumResidueLimit,
                IsSampleBased = true,
                ImputeMissingValues = true,
                Cumulative = true,
            };
            var concentrationsConfig = new ConcentrationsModuleConfig {
                FocalFoods = [ new () {
                    CodeFood = focalFood.Code,
                    CodeSubstance =  focalSubstance.Code,
                } ]
            };
            var project = new ProjectDto(concentrationModelsConfig, concentrationsConfig);

            var data = new ActionData() {
                ModelledFoods = modelledFoods,
                ModelledSubstances = substances,
                CorrectedRelativePotencyFactors = correctedRelativePotencyFactors,
                ActiveSubstanceSampleCollections = activeSubstanceSampleCollections,
                MaximumConcentrationLimits = maximumResidueLimits
            };

            var calculator = new ConcentrationModelsActionCalculator(project);
            TestRunUpdateSummarizeNominal(project, calculator, data, "FocalScenario");

        }
    }
}