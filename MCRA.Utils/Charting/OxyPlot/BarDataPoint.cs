namespace MCRA.Utils.Charting.OxyPlot {
    public class BarDataPoint {

        public string Category { get; set; }
        public string Serie { get; set; }
        public double Contribution { get; set; }
        public BarDataPoint(string category, string label, double contribution) {
            Serie = label;
            Contribution = contribution;
            Category = category;
        }
    }
}
