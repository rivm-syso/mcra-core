using MCRA.Simulation.Calculators.NonDietaryIntakeCalculation;

namespace MCRA.Simulation.Test.Mock.MockCalculatorSettings {
    public sealed class MockNonDietaryExposureGeneratorFactorySettings : INonDietaryExposureGeneratorFactorySettings {
        public bool MatchSpecificIndividuals { get; set; }
        public bool IsCorrelationBetweenIndividuals { get; set; }
    }
}
