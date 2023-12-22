using MCRA.Utils.Charting.OxyPlot;
using MCRA.Utils.R.REngines;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Utils.Test.UnitTests.Charting.R {
    [TestClass]
    public class RChartCreatorBaseTests : ChartCreatorTestsBase {

        public class MockChartCreator : RChartCreatorBase {

            public override void CreateToPng(string fileName) {
                throw new NotImplementedException();
            }

            public override void CreateToSvg(string fileName) {
                var plotName = fileName.Replace(@"\", "/");
                using (var R = new RDotNetEngine()) {
                    try {
                        R.EvaluateNoReturn("svg('" + plotName + "')");
                        R.EvaluateNoReturn("plot(1,1)");
                        R.EvaluateNoReturn("dev.off()");
                    } finally {
                    }
                }
            }

            public override string ToSvgString(int width, int height) {
                throw new NotImplementedException();
            }

            public override void WritePngToStream(Stream stream) {
                throw new NotImplementedException();
            }
        }

        [TestMethod]
        public void RChartCreatorBase_TestCreate() {
            var mockChartCreator = new MockChartCreator();
            WriteSvg(mockChartCreator, "TestCreate");
        }
    }
}
