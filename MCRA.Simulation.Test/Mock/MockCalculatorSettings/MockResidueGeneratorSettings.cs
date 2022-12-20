using MCRA.General;
using MCRA.Simulation.Calculators.ResidueGeneration;

namespace MCRA.Simulation.Test.Mock.MockCalculatorSettings {
    public sealed class MockResidueGeneratorSettings : IResidueGeneratorSettings {
        public NonDetectsHandlingMethod NonDetectsHandlingMethod { get; set; }

        public bool UseOccurrencePatternsForResidueGeneration { get; set; }

        public bool TreatMissingOccurrencePatternsAsNotOccurring { get; set; }

        public bool IsSampleBased { get; set; }

        public bool UseEquivalentsModel { get; set; }

        public ExposureType ExposureType { get; set; }
    }
}
