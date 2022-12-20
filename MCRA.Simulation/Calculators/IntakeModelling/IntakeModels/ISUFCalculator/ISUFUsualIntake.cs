namespace MCRA.Simulation.Calculators.IntakeModelling {
    /// <summary>
    /// Summarizes structures for the usual exposure according to the ISUF model
    /// </summary>
    public sealed class ISUFUsualIntake {
        public double UsualIntake { get; set; }
        public double Deviate { get; set; }
        public double CumulativeProbability { get; set; }
    }
}
