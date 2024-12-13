using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.MolecularDockingModels {
    /// <summary>
    /// OutputGeneration, ActionSummaries, MolecularDockingModels
    /// </summary>
    [TestClass]
    public class MolecularDockingModelsSummarySectionTests : SectionTestBase {
        /// <summary>
        /// Summarize and test MolecularDockingModelsSummarySection view
        /// </summary>
        [TestMethod]
        public void MolecularDockingModelsSummarySectionSection_TestSummarize() {
            var effects = FakeEffectsGenerator.Create(1);
            var focalEffect = effects.First();
            var substances = FakeSubstancesGenerator.Create(4);
            double threshold = -6;
            var dockingModels = new List<MolecularDockingModel>() {
                FakeMolecularDockingModelsGenerator.Create(focalEffect, substances, threshold, [-5, -5, -7, double.NaN]),
                FakeMolecularDockingModelsGenerator.Create(focalEffect, substances, threshold, [-5, -7, -7, double.NaN]),
                FakeMolecularDockingModelsGenerator.Create(focalEffect, substances, threshold, [-7, -7, -7, double.NaN]),
            };

            var section = new MolecularDockingModelsSummarySection();
            section.Summarize(dockingModels, substances.ToHashSet());
            Assert.AreEqual(3, section.Records.Count);
            AssertIsValidView(section);
            //RenderView(section, filename: "TestSummarize.html");
        }
    }
}
