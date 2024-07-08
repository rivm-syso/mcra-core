using MCRA.Utils.Statistics;
using MCRA.Data.Compiled;
using MCRA.Data.Management;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Actions.PointsOfDeparture;
using MCRA.Simulation.Test.Mock;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MCRA.Simulation.Action.UncertaintyFactorial;
using MCRA.General.ModuleDefinitions.Settings;

namespace MCRA.Simulation.Test.UnitTests.Actions {
    /// <summary>
    /// Runs the HazardDoses action
    /// </summary>
    [TestClass]
    public class HazardDosesActionCalculatorTests : ActionCalculatorTestsBase {

        /// <summary>
        /// Runs the HazardDoses action: load data, summarize action result: without uncertainty
        /// project.EffectSettings.CodeFocalEffect = effects.First().Code;
        /// </summary>
        [TestMethod]
        public void HazardDosesActionCalculator_TestNoUncertainty() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var effects = MockEffectsGenerator.Create(3);
            var substances = MockSubstancesGenerator.Create(3);
            var hazardDoses = MockPointsOfDepartureGenerator.Create(substances, PointOfDepartureType.Bmd, effects.First(), "Rat", random);
            var compiledData = new CompiledData() {
                AllPointsOfDeparture = hazardDoses.Select(c => c.Value).ToList(),
                AllEffects = effects.ToDictionary(c => c.Code),
            };
            var data = new ActionData {
                ActiveSubstances = substances,
                RelevantEffects = effects,
            };

            var project = new ProjectDto();
            project.EffectsSettings.CodeFocalEffect = effects.First().Code;
            var dataManager = new MockCompiledDataManager(compiledData);
            var subsetManager = new SubsetManager(dataManager, project);
            var calculator = new PointsOfDepartureActionCalculator(project);
            var header = TestLoadAndSummarizeNominal(calculator, data, subsetManager, "TestLoad1");
            Assert.IsNotNull(data.PointsOfDeparture);
            var factorialSet = new UncertaintyFactorialSet(UncertaintySource.RPFs);
            var uncertaintySourceGenerators = factorialSet.UncertaintySources.ToDictionary(r => r, r => random as IRandom);
            TestLoadAndSummarizeUncertainty(calculator, data, header, random, factorialSet, uncertaintySourceGenerators, "TestLoad");
        }
    }
}