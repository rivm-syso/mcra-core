namespace MCRA.Simulation.Calculators.PbkModelCalculation.SbmlModelCalculation {
    public sealed class SimulationOutput {
        public double[] Time { get; set; }
        public Dictionary<string, double[]> OutputTimeSeries { get; set; }
        public Dictionary<string, double> OutputStates { get; set; }
    }
}
