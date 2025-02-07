using MCRA.General;

namespace MCRA.Simulation.Calculators.KineticModelCalculation.PbpkModelCalculation.ExposureEvent {
    public class SingleExposureEvent : IExposureEvent {
        public ExposurePathType Route { get; set; }
        public double Time { get; set; }
        public double Value { get; set; }
    }
}
