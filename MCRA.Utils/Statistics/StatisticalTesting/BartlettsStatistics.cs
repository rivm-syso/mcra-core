namespace MCRA.Utils.Statistics {
    /// <summary>
    /// Anova, variances
    /// </summary>
    public class BartlettsStatistics {
        public double M { get; set; }
        public double C { get; set; }
        public double ChiSquare { get { return M / C; } }
        public double Probability { get; set; }
    }
}
