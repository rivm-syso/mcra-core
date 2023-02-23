using MCRA.Utils.Charting.OxyPlot;
using MCRA.Utils.ExtensionMethods;
using OxyPlot.Series;

namespace MCRA.Simulation.OutputGeneration {
    public abstract class PieChartCreatorBase : OxyPlotPieChartCreator {

        /// <summary>
        /// Gets a splitting point for separating the relevant pie-slices from the others category based on
        /// a limit fraction (the cumulated contribution fraction of the highest contributing slices) and a
        /// limit count (being the maximum number of slices that may be selected). The method returns the
        /// number of records to take so both limits apply.
        /// </summary>
        /// <param name="pieSlices"></param>
        /// <param name="maxContribution"></param>
        /// <param name="maxRecords"></param>
        /// <returns></returns>
        protected int getPieSplit(IEnumerable<PieSlice> pieSlices, double maxContribution, int maxRecords) {
            if (pieSlices.Count() <= maxRecords) {
                return pieSlices.Count();
            }
            maxRecords = (maxRecords == -1) ? pieSlices.Count() : maxRecords;
            var sum = pieSlices.Sum(r => r.Value);
            var cumulatedWeights = pieSlices.CumulativeWeights(s => s.Value / sum);
            var splitPoint = cumulatedWeights
                .Select((value, index) => (value, index))
                .Where(r => r.value > maxContribution || r.index >= maxRecords - 1)
                .Select(r => r.index)
                .First();
            return splitPoint + 1;
        }
    }
}