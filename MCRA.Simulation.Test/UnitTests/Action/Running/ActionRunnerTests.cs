using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.OutputGeneration.Helpers;
using MCRA.Simulation.OutputManagement;
using MCRA.Simulation.Test.Helpers;

namespace MCRA.Simulation.Test.UnitTests.Action.Running {
    [TestClass]
    public class ActionRunnerTests {

        protected static string _reportOutputPath = Path.Combine(TestUtilities.TestOutputPath, "ActionRunner");

        [TestMethod]
        public void ActionRunner_TestSummarizeSettings() {
            var project = new ProjectDto();
            var actionMapping = ActionMappingFactory.Create(project, ActionType.Risks);
            var runner = new ActionRunner(project);
            var toc = new SummaryToc(new InMemorySectionManager());
            runner.SummarizeSettings(actionMapping, toc);
            Assert.IsTrue(toc.SubSectionHeaders.Any());
            WriteReport(toc, "TestSummarizeSettings");
        }

        private void WriteReport(SummaryToc toc, string filename) {
            var reportBuilder = new ReportBuilder(toc);
            var html = reportBuilder.RenderReport(null, false, null);
            if (!string.IsNullOrEmpty(filename)) {
                filename = Path.HasExtension(filename) ? filename : $"{filename}.html";
                var outputPath = _reportOutputPath;
                if (!Directory.Exists(outputPath)) {
                    Directory.CreateDirectory(outputPath);
                }
                File.WriteAllText(Path.Combine(outputPath, filename), html);
            }
        }
    }
}
