namespace MCRA.Simulation.Calculators.IntakeModelling.IntakeModels.ISUFCalculator {
    public interface IISUFModelCalculationSettings {
        int GridPrecision { get; }
        int NumberOfIterations { get; }
        bool IsSplineFit { get; }
    }
}
