using MCRA.General;

namespace MCRA.Simulation.Calculators.KineticConversionFactorModels {
    public class KineticConversionFactorModelParametrisation : IKineticConversionFactorModelParametrisation {
        public double Factor { get; set; }
        public double? Age { get; set; }
        public GenderType Gender { get; set; }
    }
}
