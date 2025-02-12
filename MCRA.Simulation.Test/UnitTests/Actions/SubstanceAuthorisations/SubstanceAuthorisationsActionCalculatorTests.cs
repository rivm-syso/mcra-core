﻿using MCRA.Data.Compiled;
using MCRA.Data.Management;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Actions.SubstanceAuthorisations;
using MCRA.Simulation.Test.Mock;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Actions {
    /// <summary>
    /// Runs the SubstanceAuthorisations action
    /// </summary>
    [TestClass]
    public class SubstanceAuthorisationsActionCalculatorTests : ActionCalculatorTestsBase {

        /// <summary>
        /// Runs the SubstanceAuthorisations action: load data and summarize method
        /// </summary>
        [TestMethod]
        public void SubstanceAuthorisationsActionCalculator_TestLoadAndSummarize() {
            var foods = FakeFoodsGenerator.Create(3);
            var substances = FakeSubstancesGenerator.Create(3);
            var compiledData = new CompiledData() {
                AllSubstanceAuthorisations = FakeSubstanceAuthorisationsGenerator.Create(foods, substances),
            };
            var project = new ProjectDto();
            var data = new ActionData();
            var dataManager = new MockCompiledDataManager(compiledData);
            var subsetManager = new SubsetManager(dataManager, project);
            var calculator = new SubstanceAuthorisationsActionCalculator(project);
            var header =TestLoadAndSummarizeNominal(calculator, data, subsetManager, $"SubstanceAuthorisations");
            Assert.IsNotNull(data.SubstanceAuthorisations);
            Assert.AreEqual(9,data.SubstanceAuthorisations.Count);
            WriteReport(header, "TestLoadAndSummarize.html");
        }
    }
}