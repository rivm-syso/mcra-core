using OxyPlot.Series;

namespace MCRA.Utils.Charting.OxyPlot {
    public sealed class ColumnWithErrorItem : BarItem {

        public ColumnWithErrorItem() {
        }

        public ColumnWithErrorItem(double value,
            double lower,
            double upper,
            int categoryIndex = -1,
            string label = null
        ) {
            Value = value;
            Lower = lower;
            Upper = upper;
            CategoryIndex = categoryIndex;
            Label = label;
        }

        public double Lower { get; set; }

        public double Upper { get; set; }

        public string Label { get; set; }
    }
}
