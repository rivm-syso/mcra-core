using MCRA.General;

namespace MCRA.Simulation.Calculators.KineticConversionFactorModels {
    public interface IKineticConversionFactorModelParametrisation {
        double? Age { get; set; }
        GenderType Gender { get; set; }
        double Factor { get; set; }
    }
}
