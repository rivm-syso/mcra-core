using MCRA.Utils.Charting;
using MCRA.Utils.Test.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Utils.Test.UnitTests.Statistics.Distributions {
    [TestClass]
    public abstract class DistributionsTestsBase {

        protected static string _chartsOutputPath =
            Path.Combine(TestUtilities.TestOutputsPath, "Distributions");

        protected void WritePng(IChartCreator chartCreator, string testName) {
            var path = _chartsOutputPath;
            var fileName = Path.Combine(path, $"{testName}.png");
            if (!Directory.Exists(path)) {
                Directory.CreateDirectory(path);
            }
            chartCreator.CreateToPng(fileName);
            Assert.IsTrue(File.Exists(fileName));
        }
    }
}
