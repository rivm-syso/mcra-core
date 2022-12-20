using MCRA.General;

namespace MCRA.Simulation.Calculators.IntakeModelling.IntakeModels {
    public interface IIntakeModelCalculationSettings {
        CovariateModelType CovariateModelType { get; }
        FunctionType FunctionType { get; }
        double TestingLevel { get; }
        TestingMethodType TestingMethod { get; }
        int MinDegreesOfFreedom { get; }
        int MaxDegreesOfFreedom { get; }
    }
}
