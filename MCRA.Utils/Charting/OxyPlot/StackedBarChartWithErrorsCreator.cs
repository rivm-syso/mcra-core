using OxyPlot;

namespace MCRA.Utils.Charting.OxyPlot {
    public class StackedBarChartWithErrorsCreator : OxyPlotStackedBarChartWithErrorsCreator {

        private List<StackedColumnWithErrorItem> _data;
        private bool _uncertainty;

        public StackedBarChartWithErrorsCreator(
            List<StackedColumnWithErrorItem> data,
            bool uncertainty) {
                _data = data;
                _uncertainty = uncertainty;
        }

        public override PlotModel Create() {
            return create(_data, uncertainty: _uncertainty);
        }
    }
}

