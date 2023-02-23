using MCRA.General;

namespace MCRA.Simulation.Calculators.SingleValueRisksCalculation {
    public interface IExposureSource {
        string Code { get; }
        string Name { get; }
        ExposureRouteType Route { get; set; }
    }
}
