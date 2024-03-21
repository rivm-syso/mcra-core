using MCRA.General;

namespace MCRA.Simulation.Calculators.KineticModelCalculation.SbmlModelCalculation {
    public sealed class SimulationInput {
        public ExposurePathType Route { get; set; }
        public List<(double Time, double Value)> Events { get; set; }
    }
}
