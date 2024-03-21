using MCRA.General;

namespace MCRA.Simulation.Calculators.KineticModelCalculation.SbmlModelCalculation {
    public class SimulationOutput {
        public double[] Time { get; set; }
        public Dictionary<string, List<double>> OutputTimeSeries { get; set; }
    }
}
