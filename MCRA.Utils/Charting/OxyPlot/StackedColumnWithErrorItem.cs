namespace MCRA.Utils.Charting.OxyPlot {
    public sealed class StackedColumnWithErrorItem : ColumnWithErrorItem {

        public StackedColumnWithErrorItem() {
        }

        public StackedColumnWithErrorItem(double value,
            double lower,
            double upper,
            Dictionary<string, double> subcategories,
            int categoryIndex = -1,
            string label = null
        ) {
            Value = value;
            Lower = lower;
            Upper = upper;
            Subcategories = subcategories;
            CategoryIndex = categoryIndex;
            Label = label;
        }

        public Dictionary<string, double> Subcategories { get; set; }
    }
}
