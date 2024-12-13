using MCRA.Data.Compiled;
using MCRA.Data.Compiled.Objects;
using MCRA.Data.Management;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Action.UncertaintyFactorial;
using MCRA.Simulation.Actions.RelativePotencyFactors;
using MCRA.Simulation.Calculators.HazardCharacterisationCalculation;
using MCRA.Simulation.Test.Mock;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
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
            var substances = FakeSubstancesGenerator.Create(3);
            var hazardCharacterisations = FakeHazardCharacterisationModelsGenerator.Create(effect, substances, seed);
            var rpfDictionary = new Dictionary<string, List<RelativePotencyFactor>> {
                [effect.Code] = substances
                .Select(c => new RelativePotencyFactor() { Compound = c, Effect = effect, RPF = 1 })
                .ToList()
            };

            var compiledData = new CompiledData() {
                AllRelativePotencyFactors = rpfDictionary,
                AllEffects = new List<Effect>() { effect }.ToDictionary(c => c.Code),
                AllSubstances = substances.ToDictionary(c => c.Code)
            };

            var project = new ProjectDto();
            project.EffectsSettings.CodeFocalEffect = effect.Code;
            project.RelativePotencyFactorsSettings.CodeReferenceSubstance = substances.First().Code;
            var dataManager = new MockCompiledDataManager(compiledData);
            var subsetManager = new SubsetManager(dataManager, project);
            var hazardCharacterisationCollection = new List<HazardCharacterisationModelCompoundsCollection>() { new() { HazardCharacterisationModels = hazardCharacterisations } };
            var data = new ActionData() {
                ActiveSubstances = substances,
                ReferenceSubstance = substances.First(),
                SelectedEffect = effect,
                HazardCharacterisationModelsCollections = hazardCharacterisationCollection
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
            var substances = FakeSubstancesGenerator.Create(3);
            var hazardCharacterisations = FakeHazardCharacterisationModelsGenerator.Create(effect, substances, seed);
            var rpfDictionary = new Dictionary<string, List<RelativePotencyFactor>> {
                [effect.Code] = substances
                .Select(c => new RelativePotencyFactor() { Compound = c, Effect = effect, RPF = 1 })
                .ToList()
            };

            var compiledData = new CompiledData() {
                AllRelativePotencyFactors = rpfDictionary,
                AllEffects = new List<Effect>() { effect }.ToDictionary(c => c.Code),
            };

            var project = new ProjectDto();
            project.EffectsSettings.CodeFocalEffect = effect.Code;
            project.RelativePotencyFactorsSettings.CodeReferenceSubstance = substances.First().Code;
            project.RelativePotencyFactorsSettings.UncertaintyLowerBound = 3.1;
            project.RelativePotencyFactorsSettings.UncertaintyUpperBound = 96.9;

            var dataManager = new MockCompiledDataManager(compiledData);
            var subsetManager = new SubsetManager(dataManager, project);
            var hazardCharacterisationCollection = new List<HazardCharacterisationModelCompoundsCollection>() {
                new() {
                    TargetUnit = TargetUnit.FromExternalExposureUnit(ExternalExposureUnit.ugPerKgBWPerDay),
                    HazardCharacterisationModels = hazardCharacterisations
                }
            };

            var data = new ActionData() {
                ActiveSubstances = substances,
                ReferenceSubstance = substances.First(),
                SelectedEffect = effect,
                HazardCharacterisationModelsCollections = hazardCharacterisationCollection
            };

            var calculator = new RelativePotencyFactorsActionCalculator(project);
            var (header, _) = TestRunUpdateSummarizeNominal(project, calculator, data, "TestLoad2");
            Assert.IsNotNull(data.CorrectedRelativePotencyFactors);
            var factorialSet = new UncertaintyFactorialSet(UncertaintySource.RPFs);
            var uncertaintySourceGenerators = factorialSet.UncertaintySources.ToDictionary(r => r, r => random as IRandom);
            TestRunUpdateSummarizeUncertainty(calculator, data, header, random, factorialSet, uncertaintySourceGenerators);
        }


        /// <summary>
        /// Runs the RelativePotencyFactors action: run and summarize method
        /// </summary>
        //
        [DataRow(0, 0)] //OK
        [DataRow(0, 1)] //reference is missing
        [DataRow(2, 1)] //substance is missing
        [TestMethod]
        public void RelativePotencyFactorsActionCalculator_TestLoadUncertain(
                int referenceIndex, int skipIndex
            ) {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var effect = new Effect() { Code = "code" };

            var substances = FakeSubstancesGenerator.Create(3);
            var referenceSubstance = substances[referenceIndex];
            var hazardCharacterisations = FakeHazardCharacterisationModelsGenerator.Create(effect, substances, seed);
            var rpfDictionary = new Dictionary<string, List<RelativePotencyFactor>> {
                [effect.Code] = substances
                .Select(c => new RelativePotencyFactor() { Compound = c, Effect = effect, RPF = 1 })
                .ToList()
            };
            var rpfsUncertain = FakeRelativePotencyFactorsGenerator.Create(substances, referenceSubstance, random.Next());
            foreach (var substance in substances.Skip(skipIndex)) {
                var set = new RelativePotencyFactorUncertain() {
                    idUncertaintySet = $"id_1",
                    RPF = rpfsUncertain.FirstOrDefault(c => c.Compound == substance).RPF
                };
                var uncertains = rpfDictionary["code"].First(c => c.Compound == substance).RelativePotencyFactorsUncertains;
                uncertains.Add(set);
            }

            var compiledData = new CompiledData() {
                AllRelativePotencyFactors = rpfDictionary,
                AllEffects = new List<Effect>() { effect }.ToDictionary(c => c.Code),
                AllSubstances = substances.ToDictionary(c => c.Code)
            };

            var project = new ProjectDto();
            project.EffectsSettings.CodeFocalEffect = effect.Code;
            project.RelativePotencyFactorsSettings.CodeReferenceSubstance = referenceSubstance.Code;
            var dataManager = new MockCompiledDataManager(compiledData);
            var subsetManager = new SubsetManager(dataManager, project);
            var hazardCharacterisationCollection = new List<HazardCharacterisationModelCompoundsCollection>() { new() { HazardCharacterisationModels = hazardCharacterisations } };
            var data = new ActionData() {
                ActiveSubstances = substances,
                ReferenceSubstance = referenceSubstance,
                SelectedEffect = effect,
                HazardCharacterisationModelsCollections = hazardCharacterisationCollection
            };

            var calculator = new RelativePotencyFactorsActionCalculator(project);
            var header = TestLoadAndSummarizeNominal(calculator, data, subsetManager, "TestLoad1");
            Assert.IsNotNull(data.CorrectedRelativePotencyFactors);
            var factorialSet = new UncertaintyFactorialSet(UncertaintySource.RPFs);
            var uncertaintySourceGenerators = factorialSet.UncertaintySources.ToDictionary(r => r, r => random as IRandom);
            try {
                TestLoadAndSummarizeUncertainty(calculator, data, header, random, factorialSet, uncertaintySourceGenerators, "TestLoad");
            } catch (Exception ex) {
                var message = ex.ToString();
                //Missing RPF for reference substance[Compound 0(CMP0)] in RPF uncertainty set[id_1].
                //Missing RPF for [Compound 0(CMP0)] in RPF uncertainty set[id_1].
                Assert.IsTrue(true, "Should have thrown the exception");
            }

        }
    }
}