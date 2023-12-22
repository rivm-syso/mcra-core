namespace MCRA.Utils.Charting.OxyPlot {
    public class BoxPlotDataPoint {
        public double LowerWisker { get; set; }
        public double UpperWisker { get; set; }
        public double LowerBox { get; set; }
        public double UpperBox { get; set; }
        public double Median { get; set; }
        public double Reference { get; set; }
        public List<double> Outliers { get; set; }
    }
}
