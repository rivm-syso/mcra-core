using MCRA.Simulation.Calculators.ComponentCalculation.DriverSubstanceCalculation;

namespace MCRA.Simulation.Test.Mock.MockCalculatorSettings {
    public sealed class MockDriverSubstanceCalculatorSettings : IDriverSubstanceCalculatorSettings {
        public double TotalExposureCutOff { get; set; }
        public double RatioCutOff { get; set; }
    }
}
