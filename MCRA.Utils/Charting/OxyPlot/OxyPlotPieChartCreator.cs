using OxyPlot;
using OxyPlot.Series;

namespace MCRA.Utils.Charting.OxyPlot {
    public abstract class OxyPlotPieChartCreator : OxyPlotChartCreator {

        public bool ExplodeFirstSlide { get; set; }

        public override string ChartId {
            get { throw new NotImplementedException(); }
        }

        protected PlotModel create(
            List<PieSlice> pieSlices, 
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
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="records"></param>
        /// <returns></returns>
        protected int getNumberOfSlices<T>(IEnumerable<T> records, int maxSlices = 15) {
            if (records.Count() < maxSlices) {
                return records.Count();
            }
            return maxSlices;
        }
    }
}
