namespace MCRA.General.ModuleDefinitions.Interfaces {
    public interface IIntakeModelCalculationSettings {
        CovariateModelType CovariateModelType { get; }
        FunctionType FunctionType { get; }
        double TestingLevel { get; }
        TestingMethodType TestingMethod { get; }
        int MinDegreesOfFreedom { get; }
        int MaxDegreesOfFreedom { get; }
    }
}
