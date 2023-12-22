using MCRA.Utils.Charting.OxyPlot;

namespace MCRA.Simulation.OutputGeneration {
    public abstract class ReportViolinChartCreatorBase : ViolinChartCreatorBase, IReportChartCreator {
        public abstract string ChartId { get; }
        public abstract string Title { get; }
    }
}
