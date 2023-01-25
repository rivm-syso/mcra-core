using OxyPlot;
using OxyPlot.Core.Drawing;

namespace MCRA.Utils.Charting.OxyPlot {
    public class HorizontalHeatmapCreator : OxyPlotHeatMapCreator {

        private readonly string _title;

        public void CreateToFile(PlotModel plotModel, string filename) {
            plotModel.Background = OxyColors.White;
            PngExporter.Export(plotModel, filename, 500, 350, 96);
        }

        public override string ChartId => throw new NotImplementedException();

        public HorizontalHeatmapCreator(string title) {
            _title = title;
        }

        public override PlotModel Create() {
            return createDefaultPlotModel(_title);
        }
    }
}
