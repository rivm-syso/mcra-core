namespace MCRA.Simulation.Calculators.ComponentCalculation.NmfCalculation {
    public interface INmfCalculatorSettings {
        int NumberOfIterations { get; }
        int NumberOfComponents { get; }
        double Sparseness { get; }
        double Epsilon { get; }
    }
}
