using MCRA.General;

namespace MCRA.Simulation.Calculators.KineticModelCalculation.PbpkModelCalculation.ExposureEvent {
    public class RepeatingExposureEvent : IExposureEvent {
        public ExposureRoute Route { get; set; }
        public double TimeStart { get; set; }
        public double? TimeEnd { get; set; }
        public double Interval { get; set; }
        public double Value { get; set; }
    }
}
