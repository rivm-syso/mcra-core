using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.MolecularDockingModels {
    /// <summary>
    /// OutputGeneration, ActionSummaries, MolecularDockingModels
    /// </summary>
    [TestClass]
    public class MolecularDockingModelsBindingEnergiesSectionTests : ChartCreatorTestBase {
        /// <summary>
        /// Summarize and test MolecularDockingModelsBindingEnergiesSection view
        /// </summary>
        [TestMethod]
        public void MolecularDockingModelsBindingEnergiesCharts_Test1() {
            var effects = FakeEffectsGenerator.Create(1);
            var focalEffect = effects.First();
            var substances = FakeSubstancesGenerator.Create(4);
            double threshold = -6;
            var dockingModels = new List<MolecularDockingModel>() {
                FakeMolecularDockingModelsGenerator.Create(focalEffect, substances, threshold, [-5, -5, -7, double.NaN]),
                FakeMolecularDockingModelsGenerator.Create(focalEffect, substances, threshold, [-5, -7, -7, double.NaN]),
                FakeMolecularDockingModelsGenerator.Create(focalEffect, substances, threshold, [-7, -7, -7, double.NaN]),
            };

            var section = new MolecularDockingModelsBindingEnergiesSection();
            section.Summarize(dockingModels, substances.ToHashSet());
            var chart = new MolecularDockingModelsBindingEnergiesChartCreator(section);
            RenderChart(chart, $"TestCreate");
            AssertIsValidView(section);
        }
    }
}
