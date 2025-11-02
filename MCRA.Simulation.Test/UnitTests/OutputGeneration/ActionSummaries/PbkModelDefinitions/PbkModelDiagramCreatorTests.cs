using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.OutputGeneration.Helpers;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.PbkModelDefinitions {

    [TestClass]
    public class PbkModelDiagramCreatorTests : ChartCreatorTestBase {

        [TestMethod]
        [DataRow("simple_lifetime", ChartFileType.Svg)]
        [DataRow("simple_lifetime", ChartFileType.Png)]
        [DataRow("EuroMixGenericPbk", ChartFileType.Svg)]
        [DataRow("EuroMixGenericPbk", ChartFileType.Png)]
        public void PbkModelDiagramCreator_TestCreate(string name, ChartFileType fileType) {
            var filename = $"Resources/PbkModels/{name}.sbml";
            var model = FakePbkModelDefinitionsGenerator.CreateFromSbml(filename);
            var section = new PbkModelDefinitionSummarySection();
            section.Summarize(model);
            var chart = new PbkModelDiagramCreator(section);
            TestRender(chart, $"TestCreate{fileType}_{name}", fileType);
        }
    }
}
