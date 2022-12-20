using MCRA.Simulation.Calculators.ComponentCalculation.NmfCalculation;

namespace MCRA.Simulation.Test.Mock.MockCalculatorSettings {
    public sealed class MockNmfCalculatorSettings : INmfCalculatorSettings {
        public int NumberOfIterations { get; set; }
        public int NumberOfComponents { get; set; }
        public double Sparseness { get; set; }
        public double Epsilon { get; set; }
    }
}
