using MCRA.General;

namespace MCRA.Simulation.Calculators.KineticModelCalculation.PbpkModelCalculation.ExposureEvent {
    public interface IExposureEvent {
        ExposurePathType Route { get; set; }
        double Value { get; set; }
    }
}
