using MCRA.Utils.Statistics;
using MCRA.Data.Compiled;
using MCRA.Data.Compiled.Objects;
using MCRA.Data.Management;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Actions.QsarMembershipModels;
using MCRA.Simulation.Test.Mock;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Actions {
    /// <summary>
    /// Runs the QsarMembershipModels action
    /// </summary>
    [TestClass]
    public class QsarMembershipModelsActionCalculatorTests : ActionCalculatorTestsBase {
        /// <summary>
        /// Runs the QsarMembershipModels action: load data, summarize action result method
        /// </summary>
        [TestMethod]
        public void QsarMembershipModelsActionCalculator_Test() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var project = new ProjectDto();
            var effects = MockEffectsGenerator.Create(3);
            var substances = MockSubstancesGenerator.Create(3);
            var selectedEffect = effects.First();
            var allCompounds = substances;

            var qsarMembershipModels = new Dictionary<string, QsarMembershipModel>();
            var qsarMembershipModel1 = new QsarMembershipModel() {
                Code = "model1",
                Effect = effects.First(),
                Description = "",
                Name = "model1",
                Reference = "Reference",
                Accuracy = random.NextDouble(),
                Sensitivity = random.NextDouble(),
                Specificity = random.NextDouble(),
                MembershipScores = substances.ToDictionary(c => c, c => random.NextDouble()),
            };
            var qsarMembershipModel2 = new QsarMembershipModel() {
                Code = "model2",
                Effect = effects.First(),
                Description = "",
                Name = "model2",
                Reference = "Reference",
                Accuracy = random.NextDouble(),
                Sensitivity = random.NextDouble(),
                Specificity = random.NextDouble(),
                MembershipScores = substances.ToDictionary(c => c, c => random.NextDouble()),
            };
            qsarMembershipModels["model1"] = qsarMembershipModel1;
            qsarMembershipModels["model2"] = qsarMembershipModel2;
            var compiledData = new CompiledData() {
                AllQsarMembershipModels = qsarMembershipModels,
            };

            var dataManager = new MockCompiledDataManager(compiledData);
            var subsetManager = new SubsetManager(dataManager, project);

            var data = new ActionData() {
                SelectedEffect = selectedEffect,
                RelevantEffects = effects,
                AllCompounds = allCompounds,
            };

            var calculator = new QsarMembershipModelsActionCalculator(project);
            var header = TestLoadAndSummarizeNominal(calculator, data, subsetManager, "TestLoad");
            Assert.IsNotNull(data.QsarMembershipModels);
        }
    }
}