namespace MCRA.Simulation.Calculators.KineticModelCalculation.PbpkModelCalculation.SbmlModelCalculation {
    public class SimulationOutput {
        public double[] Time { get; set; }
        public Dictionary<string, List<double>> OutputTimeSeries { get; set; }
        public Dictionary<string, double> CompartmentVolumes { get; set; }
    }
}
