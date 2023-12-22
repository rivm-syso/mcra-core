using MCRA.Utils.Charting.OxyPlot;
using OxyPlot;

namespace MCRA.Simulation.OutputGeneration {
    public abstract class ReportCorrelationsChartCreatorBase : OxyPlotCorrelationsChartCreator, IReportChartCreator {

        public abstract string ChartId { get; }

        public virtual string Title { get; }

        public virtual int CellSize { get; private set; } = 20;

        protected PlotModel create(double[,] correlations, List<string> names) {
            var plotModel = createScatterHeatmap(correlations, names, names, CellSize);
            plotModel.Title = Title;
            return plotModel;
        }
    }
}
