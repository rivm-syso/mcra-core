using MCRA.General;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmKineticConversionFactor {
    public abstract class KineticConversionFactorModelParametrisationBase : IKineticConversionFactorModelParametrisation {
        public double? Age { get; set; }
        public GenderType Gender { get; set; }
    }
}
