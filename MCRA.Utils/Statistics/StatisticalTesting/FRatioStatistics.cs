namespace MCRA.Utils.Statistics {

    /// <summary>
    /// Anova, means
    /// </summary>
    public sealed class FRatioStatistics {
        public double MsBetween { get; set; }
        public double MsWithin { get; set; }
        public double BetweenDf { get; set; }
        public double WithinDf { get; set; }
        public double Probability { get; set; }
        public double Fratio { get { return MsBetween / MsWithin; } }
    }
}
