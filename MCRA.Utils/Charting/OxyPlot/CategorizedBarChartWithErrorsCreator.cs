using OxyPlot;

namespace MCRA.Utils.Charting.OxyPlot {
    public class CategorizedBarChartWithErrorsCreator : OxyPlotCategorizedBarChartWithErrorsCreator {

        private List<CategorizedColumnWithErrorItem> _data;
        private bool _uncertainty;

        public CategorizedBarChartWithErrorsCreator(
            List<CategorizedColumnWithErrorItem> data,
            bool uncertainty) {
                _data = data;
                _uncertainty = uncertainty;
        }

        public override PlotModel Create() {
            return create(_data, uncertainty: _uncertainty);
        }
    }
}

