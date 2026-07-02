using System.Reflection;
using ModuleDiagramCreator.DiagramCreators;

namespace ModuleDiagramCreator.Test.IntegrationTests {

    [TestClass]
    public class DiagramCreationTests {

        [TestMethod]
        public void GenerateDiagram_DefaultSettings_ShouldCreateSvgFile() {
            // Arrange
            var asmPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var outputImagePath = Path.Combine(asmPath, CreateOptions._defaultDiagramFilename);
            outputImagePath = Path.ChangeExtension(outputImagePath, CreateOptions._defaultOutputFormat);

            var diagramCreator = new GraphvizDiagramCreator();

            var options = new CreateOptions() {
                OutputDotFile = true,
                LayoutAlgorithm = "dot",
                OutputFormat = CreateOptions._defaultOutputFormat,
                DiagramFilename = CreateOptions._defaultDiagramFilename
            };

            // Act
            diagramCreator.CreateToFile(options);

            // Assert
            Assert.IsTrue(File.Exists(outputImagePath), "Please check if you have added a new module. If yes, then " +
                "add this module to .\\mcra-core\\ModuleDiagramGenerator\\Resources\\ModuleDiagramDefinitions.xml");
        }
    }
}
