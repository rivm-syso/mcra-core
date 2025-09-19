using MCRA.General;

namespace MCRA.Simulation.Calculators.PbpkModelCalculation.ExposureEventsGeneration {
    public interface IExposureEvent {
        ExposureRoute Route { get; set; }
        double Value { get; set; }
        double Time { get; set; }
    }
}
