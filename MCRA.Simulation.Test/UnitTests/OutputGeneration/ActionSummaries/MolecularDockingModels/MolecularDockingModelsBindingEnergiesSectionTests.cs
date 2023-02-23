using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
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
            var effects = MockEffectsGenerator.Create(1);
            var focalEffect = effects.First();
            var substances = MockSubstancesGenerator.Create(4);
            double threshold = -6;
            var dockingModels = new List<MolecularDockingModel>() {
                MockMolecularDockingModelsGenerator.Create(focalEffect, substances, threshold, new [] { -5, -5, -7, double.NaN }),
                MockMolecularDockingModelsGenerator.Create(focalEffect, substances, threshold, new [] { -5, -7, -7, double.NaN }),
                MockMolecularDockingModelsGenerator.Create(focalEffect, substances, threshold, new [] { -7, -7, -7, double.NaN }),
            };

            var section = new MolecularDockingModelsBindingEnergiesSection();
            section.Summarize(dockingModels, substances.ToHashSet());
            var chart = new MolecularDockingModelsBindingEnergiesChartCreator(section);
            RenderChart(chart, $"TestCreate");
            AssertIsValidView(section);
        }
    }
}
