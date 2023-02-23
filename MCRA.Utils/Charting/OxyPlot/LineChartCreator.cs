using OxyPlot;

namespace MCRA.Utils.Charting.OxyPlot {
    public class LineChartCreator : OxyPlotHistogramCreator {

        public override string ChartId => throw new NotImplementedException();

        private readonly string _title;

        public LineChartCreator(string title) {
            _title = title;
        }

        public override PlotModel Create() {
            var plotModel = createDefaultPlotModel(_title);
            return plotModel;
        }
    }
}
