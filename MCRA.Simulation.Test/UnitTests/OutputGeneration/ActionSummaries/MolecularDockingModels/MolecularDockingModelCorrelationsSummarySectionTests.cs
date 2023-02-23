using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.MolecularDockingModels {
    /// <summary>
    /// OutputGeneration, ActionSummaries, MolecularDockingModels
    /// </summary>
    [TestClass]
    public class MolecularDockingModelCorrelationsSummarySectionTests : ChartCreatorTestBase {
        /// <summary>
        /// Summarize and test MolecularDockingModelCorrelationsSummarySection view
        /// </summary>
        [TestMethod]
        public void MolecularDockingModelCorrelationsSummarySectionTests_TestSummarize() {
            var effects = MockEffectsGenerator.Create(1);
            var focalEffect = effects.First();
            var substances = MockSubstancesGenerator.Create(4);
            double threshold = -6;
            var dockingModels = new List<MolecularDockingModel>() {
                MockMolecularDockingModelsGenerator.Create(focalEffect, substances, threshold, new [] { -5, -5, -7, double.NaN }),
                MockMolecularDockingModelsGenerator.Create(focalEffect, substances, threshold, new [] { -5, -7, -7, double.NaN }),
                MockMolecularDockingModelsGenerator.Create(focalEffect, substances, threshold, new [] { -7, -7, -7, double.NaN }),
            };
            var section = new MolecularDockingModelCorrelationsSummarySection();
            section.Summarize(dockingModels, substances.ToHashSet());

            AssertIsValidView(section);
            //RenderView(section, filename: "TestSummarize.html");
        }
    }
}
