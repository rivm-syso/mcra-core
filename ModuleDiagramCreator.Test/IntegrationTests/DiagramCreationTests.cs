using System.Diagnostics;
using System.Reflection;

namespace ModuleDiagramCreator.Test.IntegrationTests {

    [TestClass]
    public class DiagramCreationTests {

        [TestMethod]
        public void GenerateDiagram_DefaultSettings_ShouldCreateSvgFile() {
            // Arrange
            var asmPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var exePath = Path.Combine(asmPath, "ModuleDiagramCreator.exe");
            var dllPath = Path.Combine(asmPath, "ModuleDiagramCreator.dll");
            var outputImagePath = Path.Combine(asmPath, CreateOptions._defaultDiagramFilename);
            outputImagePath = Path.ChangeExtension(outputImagePath, CreateOptions._defaultOutputFormat);

            // Act
            var process = Environment.OSVersion.Platform == PlatformID.Win32NT
                ? Process.Start(exePath)
                : Process.Start($"dotnet", dllPath);

            process.WaitForExit();

            // Assert
            Assert.AreEqual(0, process.ExitCode, "Please check if you have added a new module. If yes, then add this module to .\\mcra-core\\Tools\\ModuleDiagramGenerator\\ModuleDiagramCreator\\ModuleDiagramDefinitions.xml");
            Assert.IsTrue(File.Exists(outputImagePath));
        }
    }
}
