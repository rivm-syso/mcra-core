using MCRA.General;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmKineticConversionFactor {
    public interface IKineticConversionFactorModelParametrisation {
        double? Age { get; set; }
        GenderType Gender { get; set; }
    }
}
