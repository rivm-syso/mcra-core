using OxyPlot.Series;

namespace MCRA.Utils.Charting.OxyPlot {
    public class MultipleWhiskerBoxPlotItem {
        private readonly BoxPlotItem _boxPlotItem;

        /// <summary>
        /// The lowerbound is used to plot e,g, the lowest possible value (e.g. lor, loq or lod)
        /// </summary>
        public MultipleWhiskerBoxPlotItem(
            BoxPlotItem boxPlotItem,
            double minWhisker,
            double maxWhisker,
            double lowerBound = 0
        ) {
            _boxPlotItem = boxPlotItem;
            MinWhisker = minWhisker;
            MaxWhisker = maxWhisker;
            LowerBound = lowerBound;
        }
        public double MinWhisker { get; set; }
        public double MaxWhisker { get; set; }
        public double LowerBound { get; set; }
        public double Median => _boxPlotItem.Median;
        public double X => _boxPlotItem.X;
        public double UpperWhisker => _boxPlotItem.UpperWhisker;
        public double LowerWhisker => _boxPlotItem.LowerWhisker;
        public double BoxTop => _boxPlotItem.BoxTop;
        public double BoxBottom => _boxPlotItem.BoxBottom;
        public IList<double> Outliers => _boxPlotItem.Outliers;
        public IList<double> Values => _boxPlotItem.Values;
    }
}
