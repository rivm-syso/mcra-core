using MCRA.Utils.Charting.OxyPlot;

namespace MCRA.Simulation.OutputGeneration {
    public abstract class ReportCategorizedBarChartWithErrorsCreatorBase : OxyPlotCategorizedBarChartWithErrorsCreator, IReportChartCreator {

        public abstract string ChartId { get; }

        public virtual string Title { get; }

    }
}
