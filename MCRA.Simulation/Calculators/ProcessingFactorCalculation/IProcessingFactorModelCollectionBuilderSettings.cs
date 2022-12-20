namespace MCRA.Simulation.Calculators.ProcessingFactorCalculation {
    public interface IProcessingFactorModelCollectionBuilderSettings {
        bool IsProcessing { get; }
        bool IsDistribution { get; }
        bool AllowHigherThanOne { get; }
    }
}
