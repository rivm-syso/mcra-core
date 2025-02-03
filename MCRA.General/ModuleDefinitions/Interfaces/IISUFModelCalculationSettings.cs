namespace MCRA.General.ModuleDefinitions.Interfaces {
    public interface IISUFModelCalculationSettings {
        int GridPrecision { get; }
        int NumberOfIterations { get; }
        bool IsSplineFit { get; }
    }
}
