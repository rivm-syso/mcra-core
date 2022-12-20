namespace MCRA.Simulation.Calculators.IntakeModelling {
    /// <summary>
    /// Contains structures to perform model checking and plot diagnostics
    /// </summary>
    public sealed class IsufModelDiagnostics {
        public double Z { get; set; }
        public double Zhat { get; set; }
        public double GZ { get; set; }
        public double TransformedDailyIntakes { get; set; }
    }
}
