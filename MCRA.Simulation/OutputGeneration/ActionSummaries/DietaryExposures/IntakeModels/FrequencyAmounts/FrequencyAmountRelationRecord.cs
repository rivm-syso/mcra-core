namespace MCRA.Simulation.OutputGeneration {
    /// <summary>
    /// Stores summary data for the relation between frequencies and amounts
    /// </summary>
    public sealed class FrequencyAmountRelationRecord {
        public double Median { get; set; }
        public double LowerBox { get; set; }
        public double UpperBox { get; set; }
        public double LowerWisker{ get; set; }
        public double UpperWisker { get; set; }
        public List<double> Outliers { get; set; }
        public int NumberOfDays { get; set; }
    }
}
