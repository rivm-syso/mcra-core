namespace MCRA.Simulation.Calculators.DustExposureCalculation {
    public interface IDustExposureGeneratorFactorySettings {
        bool MatchSpecificIndividuals { get; }
        bool IsCorrelationBetweenIndividuals { get; }
    }
}
