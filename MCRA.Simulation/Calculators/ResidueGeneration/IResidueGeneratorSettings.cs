using MCRA.General;

namespace MCRA.Simulation.Calculators.ResidueGeneration {
    public interface IResidueGeneratorSettings {
        NonDetectsHandlingMethod NonDetectsHandlingMethod { get; }
        bool UseOccurrencePatternsForResidueGeneration { get; }
        bool TreatMissingOccurrencePatternsAsNotOccurring { get; }

        bool IsSampleBased { get; }

        bool UseEquivalentsModel { get; }
        ExposureType ExposureType { get; }
    }
}
