using MCRA.Simulation.OutputGeneration.Helpers;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.Test;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration {

    /// <summary>
    /// Base class for summary section tests.
    /// </summary>
    public abstract class ChartCreatorTestBase : SectionTestBase {

        protected static readonly string _chartOutputPath = Path.Combine(TestUtilities.TestOutputPath, "ChartCreators");

        /// <summary>
        /// Creates the chart creator tests output folder.
        /// </summary>
        /// <param name="_"></param>
        [AssemblyInitialize]
        public static new void MyTestInitialize(TestContext _) {
            if (!Directory.Exists(_chartOutputPath)) {
                Directory.CreateDirectory(_chartOutputPath);
            }
        }

        /// <summary>
        /// Renders the chart, which is stored in the default chart output path.
        /// </summary>
        /// <param name="chartCreator"></param>
        /// <param name="name"></param>
        public void RenderChart(IReportChartCreator chartCreator, string name) {
            var outputPath = Path.Combine(_chartOutputPath, GetType().Name);
            if (!Directory.Exists(outputPath)) {
                Directory.CreateDirectory(outputPath);
            }
            Assert.IsTrue(!string.IsNullOrEmpty(chartCreator.ChartId));
            chartCreator.CreateToPng(Path.Combine(outputPath, $"{name}.png"));
        }

        /// <summary>
        /// Renders the chart, which is stored in the default chart output path.
        /// </summary>
        /// <param name="chartCreator"></param>
        /// <param name="name"></param>
        /// <param name="chartFileType"></param>
        public void TestRender(
            IReportChartCreator chartCreator,
            string name,
            ChartFileType chartFileType = ChartFileType.Png
        ) {
            var outputPath = Path.Combine(_chartOutputPath, GetType().Name);
            if (!Directory.Exists(outputPath)) {
                Directory.CreateDirectory(outputPath);
            }
            Assert.IsTrue(!string.IsNullOrEmpty(chartCreator.ChartId));
            if (chartFileType == ChartFileType.Svg) {
                chartCreator.CreateToSvg(Path.Combine(outputPath, $"{name}.svg"));
            } else if (chartFileType == ChartFileType.Png) {
                chartCreator.CreateToPng(Path.Combine(outputPath, $"{name}.png"));
            } else {
                throw new NotImplementedException();
            }
        }
    }
}
