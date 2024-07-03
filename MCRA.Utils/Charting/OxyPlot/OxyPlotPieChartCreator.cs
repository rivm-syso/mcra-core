using OxyPlot;
using OxyPlot.Series;

namespace MCRA.Utils.Charting.OxyPlot {
    public abstract class OxyPlotPieChartCreator : OxyPlotChartCreator {

        public bool ExplodeFirstSlide { get; set; }

        protected PlotModel create(
            IEnumerable<PieSlice> pieSlices,
            int maxSlices,
            OxyPalette palette = null
        ) {
            var plotModel = createDefaultPlotModel(string.Empty);
            plotModel.IsLegendVisible = true;

            if (palette == null) {
                palette = CustomPalettes.GorgeousTone(maxSlices);
            }
            plotModel.Legends.Add(new CustomPieLegend());

            var pieSeries = new CustomPieSeries() {
                Pallete = palette,
                Slices = pieSlices.Take(maxSlices).ToList(),
            };

            plotModel.DefaultColors = palette.Colors;

            var othersSlices = pieSlices.Skip(maxSlices).ToList();
            var othersContribution = othersSlices.Sum(c => c.Value);
            if (othersContribution != 0) {
                pieSeries.Slices.Add(new PieSlice(label: $"{"others"} (n={othersSlices.Count})", othersContribution) { IsExploded = true });
                plotModel.DefaultColors.Add(OxyColors.LightGray);
            }

            if (ExplodeFirstSlide) {
                pieSeries.Slices.First().IsExploded = true;
            }
            plotModel.Series.Add(pieSeries);
            return plotModel;
        }

        /// <summary>
        /// Determine the number of slices of the pie.
        /// When the others category contains one record, show the record itself and not label "others (n = 1)"
        /// </summary>
        /// <param name="records">The pie records.</param>
        /// <param name="maxSlices">Absolute maximum number of slices.</param>
        /// <param name="minContributionFraction">Minimum pie contribution (fraction) of a slice.</param>
        /// <returns></returns>
        protected int getNumberOfSlices(
            IEnumerable<PieSlice> records,
            int maxSlices = 15,
            double minContributionFraction = 0.01
        ) {
            var n = records.Count();
            if (records.Any()) {
                var sum = records.Sum(r => r.Value);
                records = !double.IsNaN(minContributionFraction)
                    ? records.Where(r => r.Value / sum > minContributionFraction).ToList()
                    : records;
                var nFilter = records.Count();
                n = n - nFilter == 1 ? n : nFilter;
            }
            return  Math.Min(n, maxSlices);
        }
    }
}
