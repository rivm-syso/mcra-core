namespace MCRA.Simulation.OutputGeneration {
    public sealed class SigmaSizeRecord {
        /// <summary>
        /// Percentage e.g. 50, 90, 95, 97.5
        /// </summary>
        public double Percentage { get; set; }

        /// <summary>
        /// Sample size
        /// </summary>
        public int Size { get; set; }

        /// <summary>
        /// Number of percentiles
        /// </summary>
        public int NumberOfValues { get; set; }

        /// <summary>
        /// The sigma of the percentiles
        /// </summary>
        public double Sigma { get; set; }
    }
}
