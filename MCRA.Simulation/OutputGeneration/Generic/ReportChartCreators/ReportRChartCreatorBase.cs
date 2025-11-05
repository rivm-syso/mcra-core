using MCRA.Utils.Charting.OxyPlot;
using MCRA.Utils.R.REngines;

namespace MCRA.Simulation.OutputGeneration {
    public abstract class ReportRChartCreatorBase : RChartCreatorBase, IReportChartCreator {

        public abstract string ChartId { get; }
        public virtual string Title { get; }

        protected abstract void createPlot(Action<RDotNetEngine> openPlot, Action<RDotNetEngine> closePlot);

        public override void CreateToPng(string fileName) {
            Action<RDotNetEngine> openPlot = (rEngine) => rEngine.EvaluateNoReturn("png('" + fileName.Replace(@"\", "/") + "')");
            Action<RDotNetEngine> closePlot = (rEngine) => rEngine.EvaluateNoReturn("dev.off()");
            createPlot(openPlot, closePlot);
        }

        public override void CreateToSvg(string fileName) {
            Action<RDotNetEngine> openPlot = (rEngine) => rEngine.EvaluateNoReturn("svg('" + fileName.Replace(@"\", "/") + "')");
            Action<RDotNetEngine> closePlot = (rEngine) => rEngine.EvaluateNoReturn("dev.off()");
            createPlot(openPlot, closePlot);
        }

        public override string ToSvgString(int width, int height) {
            Action<RDotNetEngine> openPlot = (rEngine) => {
                rEngine.LoadLibrary($"svglite", null, true);
                rEngine.EvaluateNoReturn("s <- svgstring(standalone = FALSE)");
            };
            string result = null;
            Action<RDotNetEngine> closePlot = (rEngine) => {
                rEngine.EvaluateNoReturn("dev.off()");
                result = rEngine.EvaluateString("s()");
            };
            createPlot(openPlot, closePlot);
            return result;
        }

        public override void WritePngToStream(Stream stream) {
            throw new NotImplementedException();
        }
    }
}
