using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.PbkModelDefinitions {

    [TestClass]
    public class PbkModelDefinitionSummarySectionTests : SectionTestBase {

        [TestMethod]
        [DataRow("simple_lifetime")]
        [DataRow("EuroMixGenericPbk")]
        public void PbkModelDefinitionSummarySection_TestSummarizeAndRender(
            string name
        ) {
            var filename = $"Resources/PbkModels/{name}.sbml";
            var model = FakePbkModelDefinitionsGenerator.CreateFromSbml(filename);
            var section = new PbkModelDefinitionSummarySection();
            section.Summarize(model);
            AssertIsValidView(section);
            RenderView(section, filename: $"TestSummarize_{name}.html");
        }
    }
}
