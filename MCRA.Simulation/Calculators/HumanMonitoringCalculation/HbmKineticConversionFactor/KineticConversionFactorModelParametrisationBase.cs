using MCRA.General;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmKineticConversionFactor {
    public abstract class KineticConversionFactorModelParametrisationBase : IKineticConversionFactorModelParametrisation {
        public double Factor { get; set; }
        public double? Age { get; set; }
        public GenderType Gender { get; set; }
    }
}
