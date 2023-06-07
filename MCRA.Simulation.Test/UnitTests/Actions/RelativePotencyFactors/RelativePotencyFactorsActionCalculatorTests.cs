using MCRA.Data.Compiled;
using MCRA.Data.Compiled.Objects;
using MCRA.Data.Management;
using MCRA.General;
using MCRA.General.Action.Settings.Dto;
using MCRA.Simulation.Action.UncertaintyFactorial;
using MCRA.Simulation.Actions.RelativePotencyFactors;
using MCRA.Simulation.Test.Mock;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using MCRA.Utils.Statistics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Actions {

    /// <summary>
    /// Runs the RelativePotencyFactors action.
    /// </summary>
    [TestClass]
    public class RelativePotencyFactorsActionCalculatorTests : ActionCalculatorTestsBase {

        /// <summary>
        /// Runs the RelativePotencyFactors action: load data and summarize method
        /// </summary>
        [TestMethod]
        public void RelativePotencyFactorsActionCalculator_TestLoad() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var effect = new Effect() { Code = "code" };
            var substances = MockSubstancesGenerator.Create(3);
            var hazardCharacterisations = MockHazardCharacterisationModelsGenerator.Create(effect, substances, seed);
            var rpfDictionary = new Dictionary<string, List<RelativePotencyFactor>>();
            rpfDictionary[effect.Code] = substances
                .Select(c => new RelativePotencyFactor() { Compound = c, Effect = effect, RPF = 1 })
                .ToList();

            var compiledData = new CompiledData() {
                AllRelativePotencyFactors = rpfDictionary,
                AllEffects = new List<Effect>() { effect }.ToDictionary(c => c.Code, c => c),
                AllSubstances = substances.ToDictionary(c => c.Code)
            };

            var project = new ProjectDto();
            project.EffectSettings.CodeFocalEffect = effect.Code;
            project.EffectSettings.CodeReferenceCompound = substances.First().Code;
            var dataManager = new MockCompiledDataManager(compiledData);
            var subsetManager = new SubsetManager(dataManager, project);

            var data = new ActionData() {
                ActiveSubstances = substances,
                ReferenceSubstance = substances.First(),
                SelectedEffect = effect,
                HazardCharacterisations = hazardCharacterisations
            };

            var calculator = new RelativePotencyFactorsActionCalculator(project);
            var header = TestLoadAndSummarizeNominal(calculator, data, subsetManager, "TestLoad1");
            Assert.IsNotNull(data.CorrectedRelativePotencyFactors);
            var factorialSet = new UncertaintyFactorialSet(UncertaintySource.RPFs);
            var uncertaintySourceGenerators = factorialSet.UncertaintySources.ToDictionary(r => r, r => random as IRandom);
            TestLoadAndSummarizeUncertainty(calculator, data, header, random, factorialSet, uncertaintySourceGenerators, "TestLoad");
        }

        /// <summary>
        /// Runs the RelativePotencyFactors action: run and summarize method
        /// </summary>
        [TestMethod]
        public void RelativePotencyFactorsActionCalculator_TestCompute() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var effect = new Effect() { Code = "code" };
            var substances = MockSubstancesGenerator.Create(3);
            var hazardCharacterisations = MockHazardCharacterisationModelsGenerator.Create(effect, substances, seed);
            var rpfDictionary = new Dictionary<string, List<RelativePotencyFactor>>();
            rpfDictionary[effect.Code] = substances
                .Select(c => new RelativePotencyFactor() { Compound = c, Effect = effect, RPF = 1 })
                .ToList();

            var compiledData = new CompiledData() {
                AllRelativePotencyFactors = rpfDictionary,
                AllEffects = new List<Effect>() { effect }.ToDictionary(c => c.Code, c => c),
            };

            var project = new ProjectDto();
            project.EffectSettings.CodeFocalEffect = effect.Code;
            project.EffectSettings.CodeReferenceCompound = substances.First().Code;
            project.UncertaintyAnalysisSettings.UncertaintyLowerBound = 3.1;
            project.UncertaintyAnalysisSettings.UncertaintyUpperBound = 96.9;

            var dataManager = new MockCompiledDataManager(compiledData);
            var subsetManager = new SubsetManager(dataManager, project);

            var data = new ActionData() {
                ActiveSubstances = substances,
                ReferenceSubstance = substances.First(),
                SelectedEffect = effect,
                HazardCharacterisations = hazardCharacterisations
            };

            var calculator = new RelativePotencyFactorsActionCalculator(project);
            var (header, _) = TestRunUpdateSummarizeNominal(project, calculator, data, "TestLoad2");
            Assert.IsNotNull(data.CorrectedRelativePotencyFactors);
            var factorialSet = new UncertaintyFactorialSet(UncertaintySource.RPFs);
            var uncertaintySourceGenerators = factorialSet.UncertaintySources.ToDictionary(r => r, r => random as IRandom);
            TestRunUpdateSummarizeUncertainty(calculator, data, header, random, factorialSet, uncertaintySourceGenerators);
        }
    }
}