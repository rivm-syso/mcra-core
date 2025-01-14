using MCRA.Data.Compiled.Wrappers;

namespace MCRA.Simulation.Calculators.ComponentCalculation.ExposureMatrixCalculation {
    public sealed class ClusterRecord {
        public int ClusterId { get; set; }
        public List<SimulatedIndividual> SimulatedIndividuals { get; set; }
        public List<int> Indices {  get ; set; }
    }
}
