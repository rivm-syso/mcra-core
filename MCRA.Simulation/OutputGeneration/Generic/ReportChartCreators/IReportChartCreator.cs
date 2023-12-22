using MCRA.Utils.Charting;

namespace MCRA.Simulation.OutputGeneration {
    public interface IReportChartCreator : IChartCreator {

        /// <summary>
        /// Unique identifier of the chart.
        /// </summary>
        string ChartId { get; }

        /// <summary>
        /// Chart title.
        /// </summary>
        string Title { get; }

    }
}
