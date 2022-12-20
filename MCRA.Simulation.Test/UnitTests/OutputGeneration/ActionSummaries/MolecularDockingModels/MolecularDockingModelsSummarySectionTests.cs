using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

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
            var effects = MockEffectsGenerator.Create(1);
            var focalEffect = effects.First();
            var substances = MockSubstancesGenerator.Create(4);
            double threshold = -6;
            var dockingModels = new List<MolecularDockingModel>() {
                MockMolecularDockingModelsGenerator.Create(focalEffect, substances, threshold, new [] { -5, -5, -7, double.NaN }),
                MockMolecularDockingModelsGenerator.Create(focalEffect, substances, threshold, new [] { -5, -7, -7, double.NaN }),
                MockMolecularDockingModelsGenerator.Create(focalEffect, substances, threshold, new [] { -7, -7, -7, double.NaN }),
            };

            var section = new MolecularDockingModelsSummarySection();
            section.Summarize(dockingModels, substances.ToHashSet());
            Assert.AreEqual(3, section.Records.Count);
            AssertIsValidView(section);
            //RenderView(section, filename: "TestSummarize.html");
        }
    }
}
