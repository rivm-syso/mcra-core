﻿using MCRA.Data.Compiled;
using MCRA.Data.Management;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Actions.EffectRepresentations;
using MCRA.Simulation.Test.Mock;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Actions {
    /// <summary>
    /// Runs the EffectRepresentations action: load data and summarize method
    /// </summary>
    [TestClass]
    public class EffectRepresentationsActionCalculatorTests : ActionCalculatorTestsBase {
        /// <summary>
        /// Runs the EffectRepresentations action: load data and summarize method
        /// </summary>
        [TestMethod]
        public void EffectRepresentationsActionCalculator_Test() {
            var effects = FakeEffectsGenerator.Create(3);
            var selectedEffect = FakeEffectsGenerator.Create();
            var responses = FakeResponsesGenerator.Create(2);
            var effectRepresentations = FakeEffectRepresentationsGenerator.Create([selectedEffect], responses);
            var compiledData = new CompiledData() {
                AllEffectRepresentations = effectRepresentations,
            };

            var project = new ProjectDto();
            var dataManager = new MockCompiledDataManager(compiledData);
            var subsetManager = new SubsetManager(dataManager, project);

            var data = new ActionData() {
                RelevantEffects = effects,
                AllEffects = effects,
                Responses = responses.ToDictionary(c => c.Code),
                SelectedEffect = selectedEffect
            };
            var calculator = new EffectRepresentationsActionCalculator(project);
            TestLoadAndSummarizeNominal(calculator, data, subsetManager, "TestLoad1");
            Assert.IsNotNull(data.FocalEffectRepresentations);
            Assert.IsNotNull(data.AllEffectRepresentations);
        }
    }
}