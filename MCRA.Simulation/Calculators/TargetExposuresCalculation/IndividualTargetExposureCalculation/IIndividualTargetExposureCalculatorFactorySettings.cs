using MCRA.General;

namespace MCRA.Simulation.Calculators.TargetExposuresCalculation.IndividualTargetExposureCalculation {
    public interface IIndividualTargetExposureCalculatorFactorySettings {
        IntakeModelType IntakeModelType { get; }
        ExposureType ExposureType { get; }
    }
}
