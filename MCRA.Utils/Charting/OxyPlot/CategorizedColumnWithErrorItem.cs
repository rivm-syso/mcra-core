namespace MCRA.Utils.Charting.OxyPlot {
    public sealed class CategorizedColumnWithErrorItem : ColumnWithErrorItem {

        public CategorizedColumnWithErrorItem() {
        }

        public CategorizedColumnWithErrorItem(double value,
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
