using MCRA.General;

namespace MCRA.Simulation.Calculators.KineticModelCalculation.SbmlModelCalculation {
    public sealed class ExternalRouteExposureEvents {
        public TimeUnit TimeScale { get; set; }
        public TargetUnit ExposureUnit { get; set; }
        public ExposurePathType Route { get; set; }
        public List<(double Time, double Value)> Events { get; set; }
    }
}
