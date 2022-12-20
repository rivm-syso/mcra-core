namespace MCRA.Simulation.Calculators.NonDietaryIntakeCalculation {
    public interface INonDietaryExposureGeneratorFactorySettings {
        bool MatchSpecificIndividuals { get; }
        bool IsCorrelationBetweenIndividuals { get; }
    }
}
