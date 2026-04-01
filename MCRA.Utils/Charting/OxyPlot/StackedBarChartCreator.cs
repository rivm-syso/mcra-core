using OxyPlot;

namespace MCRA.Utils.Charting.OxyPlot {
    public class StackedBarChartCreator : OxyPlotStackedBarChartCreator {

        private List<BarDataPoint> _data;

        public StackedBarChartCreator(List<BarDataPoint> data) {
            _data = data;
        }

        public override PlotModel Create() {
            return create(_data);
        }
    }
}

