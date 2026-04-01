using MCRA.Utils.Charting.OxyPlot;
using MCRA.Utils.ExtensionMethods;
using OxyPlot.Series;

namespace MCRA.Simulation.OutputGeneration {
    public abstract class ReportStackedBarChartCreatorBase : OxyPlotStackedBarChartCreator, IReportChartCreator {

        public abstract string ChartId { get; }

        public virtual string Title { get; }

    }
}
