using MCRA.Data.Compiled;
using MCRA.Data.Management;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Actions.Effects;
using MCRA.Simulation.Test.Mock;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Actions {
    /// <summary>
    /// Runs the Effects action
    /// </summary>
    [TestClass]
    public class EffectsActionCalculatorTests : ActionCalculatorTestsBase {

        /// <summary>
        /// Runs the Effects action: load data and summarize method
        /// for a multiple effects analysis.
        /// </summary>
        [TestMethod]
        public void EffectsActionCalculator_TestLoadMultiple() {
            var effects = FakeEffectsGenerator.Create(3);
            var compiledData = new CompiledData() {
                AllEffects = effects.ToDictionary(c => c.Code),
            };

            var project = new ProjectDto();
            project.EffectsSettings.CodeFocalEffect = effects.First().Code;

            var dataManager = new MockCompiledDataManager(compiledData);
            var subsetManager = new SubsetManager(dataManager, project);
            var data = new ActionData();
            var calculator = new EffectsActionCalculator(project);
            TestLoadAndSummarizeNominal(calculator, data, subsetManager, "TestLoad1");

            CollectionAssert.AreEquivalent(effects, data.AllEffects.ToList());

            // Selected effect should be null, even
            Assert.IsNull(data.SelectedEffect);
        }

        /// <summary>
        /// Runs the Effects action: load data and summarize method
        /// given a single effects analysis.
        /// </summary>
        [TestMethod]
        public void EffectsActionCalculator_TestLoadSingle() {
            var effects = FakeEffectsGenerator.Create(1);
            var compiledData = new CompiledData() {
                AllEffects = effects.ToDictionary(c => c.Code),
            };

            var project = new ProjectDto();
            project.EffectsSettings.CodeFocalEffect = effects.First().Code;

            var dataManager = new MockCompiledDataManager(compiledData);
            var subsetManager = new SubsetManager(dataManager, project);
            var data = new ActionData();
            var calculator = new EffectsActionCalculator(project);
            TestLoadAndSummarizeNominal(calculator, data, subsetManager, "TestLoad1");

            CollectionAssert.AreEquivalent(effects, data.AllEffects.ToList());
            Assert.AreEqual(effects.First(), data.SelectedEffect);
        }

        /// <summary>
        /// Runs the Effects action: load data and summarize method
        /// given a single effects analysis. Selected effect should
        /// be null when multiple effects are in the scope. Single
        /// effect selection requires only one effect in all effects.
        /// </summary>
        [TestMethod]
        public void EffectsActionCalculator_TestLoadSingleFail() {
            var effects = FakeEffectsGenerator.Create(2);
            var compiledData = new CompiledData() {
                AllEffects = effects.ToDictionary(c => c.Code),
            };

            var project = new ProjectDto();
            project.EffectsSettings.CodeFocalEffect = effects.First().Code;

            var dataManager = new MockCompiledDataManager(compiledData);
            var subsetManager = new SubsetManager(dataManager, project);
            var data = new ActionData();
            var calculator = new EffectsActionCalculator(project);
            TestLoadAndSummarizeNominal(calculator, data, subsetManager, "TestLoad1");

            CollectionAssert.AreEquivalent(effects, data.AllEffects.ToList());
            Assert.IsNull(data.SelectedEffect);
        }

        /// <summary>
        /// Runs the Effects action: load data and summarize method
        /// given a AOP effects analysis.
        /// </summary>
        [TestMethod]
        public void EffectsActionCalculator_TestLoadAop() {
            var effects = FakeEffectsGenerator.Create(1);
            var compiledData = new CompiledData() {
                AllEffects = effects.ToDictionary(c => c.Code),
            };

            var project = new ProjectDto();
            project.EffectsSettings.CodeFocalEffect = effects.First().Code;
            project.EffectsSettings.IncludeAopNetwork = true;

            var dataManager = new MockCompiledDataManager(compiledData);
            var subsetManager = new SubsetManager(dataManager, project);
            var data = new ActionData();
            var calculator = new EffectsActionCalculator(project);
            TestLoadAndSummarizeNominal(calculator, data, subsetManager, "TestLoad1");

            CollectionAssert.AreEquivalent(effects, data.AllEffects.ToList());
            Assert.AreEqual(effects.First(), data.SelectedEffect);
        }
    }
}
