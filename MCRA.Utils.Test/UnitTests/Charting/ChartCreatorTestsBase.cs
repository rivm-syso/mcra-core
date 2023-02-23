using MCRA.Utils.Charting;
using MCRA.Utils.Test.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Utils.Test.UnitTests.Charting {

    [TestClass]
    public abstract class ChartCreatorTestsBase {

        protected static string _chartCreatorTestsOutputPath =
            Path.Combine(TestResourceUtilities.TestOutputsPath, "ChartCreators");

        protected void WriteSvg(IChartCreator chartCreator, string testName) {
            var path = Path.Combine(_chartCreatorTestsOutputPath, GetType().Name);
            var fileName = Path.Combine(_chartCreatorTestsOutputPath, GetType().Name, $"{testName}.svg");
            if (!Directory.Exists(path)) {
                Directory.CreateDirectory(path);
            }
            chartCreator.CreateToSvg(fileName);
            Assert.IsTrue(File.Exists(fileName));
        }

        protected void WritePng(IChartCreator chartCreator, string testName) {
            var path = Path.Combine(_chartCreatorTestsOutputPath, GetType().Name);
            var fileName = Path.Combine(_chartCreatorTestsOutputPath, GetType().Name, $"{testName}.png");
            if (!Directory.Exists(path)) {
                Directory.CreateDirectory(path);
            }
            chartCreator.CreateToPng(fileName);
            Assert.IsTrue(File.Exists(fileName));
        }
    }
}
