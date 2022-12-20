using OxyPlot;
using OxyPlot.Series;

namespace MCRA.Utils.Charting.OxyPlot {
    public sealed class ConfidenceIntervalDataPoint : ColumnItem {

        public ConfidenceIntervalDataPoint() {
        }

        public ConfidenceIntervalDataPoint(double value, double lowerConfidenceLimit, double upperConfidenceLimit, int categoryIndex = -1, OxyColor color = new OxyColor()) {
            Value = value;
            LowerConfidenceLimit = lowerConfidenceLimit;
            UpperConfidenceLimit = upperConfidenceLimit;
            CategoryIndex = categoryIndex;
            Color = color;
        }

        public double LowerConfidenceLimit { get; set; }

        public double UpperConfidenceLimit { get; set; }
    }
}
