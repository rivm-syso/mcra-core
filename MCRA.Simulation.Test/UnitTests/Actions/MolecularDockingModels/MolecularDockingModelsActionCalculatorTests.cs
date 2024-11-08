using MCRA.Utils.Statistics;
using MCRA.Data.Compiled;
using MCRA.Data.Compiled.Objects;
using MCRA.Data.Management;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Actions.MolecularDockingModels;
using MCRA.Simulation.Test.Mock;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Actions {
    /// <summary>
    /// Runs the MolecularDockingModels action
    /// </summary>
    [TestClass]
    public class MolecularDockingModelsActionCalculatorTests : ActionCalculatorTestsBase {
        /// <summary>
        /// Runs the MolecularDockingModels action: load data and summarize method
        /// </summary>
        [TestMethod]
        public void MolecularDockingModelsActionCalculator_Test() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var effects = FakeEffectsGenerator.Create(3);
            var substances = FakeSubstancesGenerator.Create(3);
            var selectedEffect = effects.First();
            var relevantEffects = effects;
            var adverseOutcomePathwayNetwork = new AdverseOutcomePathwayNetwork() {
                AdverseOutcome = effects.First(),
                Code = "network",
                Description = "Description",
                Name = "Name",
                Reference = "Reference",
                RiskTypeString = "RiskTypeString"
            };
            var allMolecularDockingModels = new Dictionary<string, MolecularDockingModel>();
            var molecularDockingModel1 = new MolecularDockingModel() {
                Code = "model1",
                Effect = effects.First(),
                Description = "",
                Name = "model1",
                Reference = "Reference",
                Threshold = 1,
                NumberOfReceptors = 2,
                BindingEnergies = substances.ToDictionary(c => c, c => random.NextDouble()),
            };
            var molecularDockingModel2 = new MolecularDockingModel() {
                Code = "model2",
                Effect = effects.First(),
                Description = "",
                Name = "model2",
                Reference = "Reference",
                Threshold = 1,
                NumberOfReceptors = 2,
                BindingEnergies = substances.ToDictionary(c => c, c => random.NextDouble()),
            };
            allMolecularDockingModels["model1"] = molecularDockingModel1;
            allMolecularDockingModels["model2"] = molecularDockingModel2;
            var compiledData = new CompiledData() {
                AllMolecularDockingModels = allMolecularDockingModels,
            };
            var project = new ProjectDto();
            var dataManager = new MockCompiledDataManager(compiledData);
            var subsetManager = new SubsetManager(dataManager, project);

            var data = new ActionData() {
                SelectedEffect = selectedEffect,
                RelevantEffects = relevantEffects,
                AllCompounds = substances,
                AdverseOutcomePathwayNetwork = adverseOutcomePathwayNetwork
            };
            var calculator = new MolecularDockingModelsActionCalculator(project);
            TestLoadAndSummarizeNominal(calculator, data, subsetManager, "TestLoad");
            Assert.IsNotNull(data.MolecularDockingModels);
        }
    }
}