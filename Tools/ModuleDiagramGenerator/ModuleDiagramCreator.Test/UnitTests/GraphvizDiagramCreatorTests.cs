using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModuleDiagramCreator.DiagramCreators;
using ModuleDiagramCreator.Test.Helpers;

namespace ModuleDiagramCreator.Test.IntegrationTests {

    [TestClass]
    public class GraphvizDiagramCreatorTests {

        [TestMethod]
        public void DiagramCreator_TestCreate() {
            var outputPath = TestUtilities.GetOrCreateTestOutputPath("CreateSVG");

            var outputImagePath = Path.Combine(outputPath, CreateOptions._defaultDiagramFilename);
            outputImagePath = Path.ChangeExtension(outputImagePath, CreateOptions._defaultOutputFormat);

            var diagramCreator = new GraphvizDiagramCreator();
            var options = new CreateOptions() {
                OutputDotFile = true,
                OutputFormat = CreateOptions._defaultOutputFormat,
                LayoutAlgorithm = "fdp",
                DiagramFilename = outputImagePath,
                OutputDir = outputPath,
            };
            diagramCreator.CreateToFile(options);
        }
    }
}
