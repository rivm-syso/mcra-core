using System.Collections.Generic;
using System.Linq;
using MCRA.Data.Compiled;
using MCRA.Data.Management;
using MCRA.General;
using MCRA.General.Action.Settings.Dto;
using MCRA.Simulation.Action.UncertaintyFactorial;
using MCRA.Simulation.Actions.DoseResponseModels;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using MCRA.Utils.Statistics;
using MCRA.Utils.Test;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Actions {
    /// <summary>
    /// Runs the DoseResponseModels action
    /// </summary>
    [TestClass]
    public class DoseResponseModelsActionCalculatorTests : ActionCalculatorTestsBase {

        /// <summary>
        /// Test load data method of dose response models action calculator.
        /// </summary>
        [TestMethod]
        public void DoseResponseModelsActionCalculator_TestLoadData() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);

            var responses = MockResponsesGenerator.Create(2);
            var substances = MockSubstancesGenerator.Create(3);
            var compiledData = new CompiledData() {
                AllDoseResponseModels = MockDoseResponseModelGenerator.Create(substances, responses, random).ToDictionary(c => c.IdDoseResponseModel),
            };
            var dataManager = new MockCompiledDataManager(compiledData);

            var project = new ProjectDto();
            var calculator = new DoseResponseModelsActionCalculator(project);

            var data = new ActionData();
            var subsetManager = new SubsetManager(dataManager, project);
            var header = TestLoadAndSummarizeNominal(calculator, data, subsetManager, "TestLoad1");
            Assert.AreEqual(2, data.DoseResponseModels.Count);
            writeOutput(header, "DoseResponseModelsActionCalculator_TestLoadData");
        }
        /// <summary>
        /// Test compute method of dose response models action calculator.
        /// </summary>
        [TestMethod]
        public void DoseResponseModelsActionCalculator_TestCompute() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);

            var responses = MockResponsesGenerator.Create(2);
            var substances = MockSubstancesGenerator.Create(2);
            var effects = MockEffectsGenerator.Create(2);
            var experiments = MockDoseResponseExperimentsGenerator.Create(substances, responses);
            var effectRepresentations = MockEffectRepresentationsGenerator.Create(effects, responses);

            var project = new ProjectDto();
            var data = new ActionData() {
                Responses = responses.ToDictionary(c => c.Code, c => c),
                AllCompounds = substances,
                AllEffects = effects,
                ReferenceCompound = substances.First(),
                FocalEffectRepresentations = effectRepresentations.Where(c => c.Effect == effects.First()).Select(c => c).ToList(),
                SelectedResponseExperiments = experiments.Take(1).ToList()
            };

            var calculator = new DoseResponseModelsActionCalculator(project);
            var (header, _) = TestRunUpdateSummarizeNominal(project, calculator, data, "DoseResponseModels");
            writeOutput(header, "DoseResponseModelsActionCalculator_TestCompute");
            var factorialSet = new UncertaintyFactorialSet(UncertaintySource.RPFs);
            var uncertaintySourceGenerators = factorialSet.UncertaintySources.ToDictionary(r => r, r => random as IRandom);
            TestRunUpdateSummarizeUncertainty(calculator, data, header, random, factorialSet, uncertaintySourceGenerators);
        }


        private static void writeOutput(SectionHeader header, string outputFolder) {
            var outputPath = TestUtilities.CreateTestOutputPath(outputFolder);
            var dict = new Dictionary<string, string>();
            //header.SaveTablesAsCsv(new DirectoryInfo(outputPath), 0, dict);
        }
    }
}
