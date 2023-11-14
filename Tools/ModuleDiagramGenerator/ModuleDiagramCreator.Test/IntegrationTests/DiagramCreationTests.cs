using System.Diagnostics;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ModuleDiagramCreator.Test.IntegrationTests {

    [TestClass]
    public class DiagramCreationTests {

        [TestMethod]
        public void GenerateDiagram_DefaultSettings_ShouldCreateSvgFile() {
            // Arrange
            var exePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "ModuleDiagramCreator.exe");
            var outputImagePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), CreateOptions._defaultDiagramFilename);
            outputImagePath = Path.ChangeExtension(outputImagePath, CreateOptions._defaultOutputFormat);

            // Act
            var process = Process.Start(exePath);
            process.WaitForExit();

            // Assert
            Assert.AreEqual(0, process.ExitCode, "Please check if you have added a new module. If yes, then add this module to .\\mcra-core\\Tools\\ModuleDiagramGenerator\\ModuleDiagramCreator\\ModuleDiagramDefinitions.xml");
            Assert.IsTrue(File.Exists(outputImagePath));
        }
    }
}
