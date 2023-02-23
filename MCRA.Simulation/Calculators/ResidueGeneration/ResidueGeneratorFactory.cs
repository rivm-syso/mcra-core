using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;
using MCRA.General;
using MCRA.Simulation.Calculators.ConcentrationModelCalculation.ConcentrationModels;
using MCRA.Simulation.Calculators.OccurrencePatternsCalculation;

namespace MCRA.Simulation.Calculators.ResidueGeneration {
    public class ResidueGeneratorFactory {
        private readonly IResidueGeneratorSettings _settings;

        public ResidueGeneratorFactory(
            IResidueGeneratorSettings settings
        ) {
            _settings = settings;
        }

        public IResidueGenerator Create(
            IDictionary<Food, SampleCompoundCollection> sampleCompoundCollections,
            IDictionary<(Food, Compound), ConcentrationModel> concentrationModels,
            IDictionary<Food, ConcentrationModel> cumulativeConcentrationModels,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Food, List<MarginalOccurrencePattern>> marginalOccurrencePatterns
        ) {
            if (_settings.ExposureType == ExposureType.Chronic) {
                var result = new MeanConcentrationResidueGenerator(concentrationModels);
                result.Initialize();
                return result;
            } else if (_settings.IsSampleBased && _settings.UseEquivalentsModel) {
                var result =  new EquivalentsModelResidueGenerator(
                    relativePotencyFactors,
                    cumulativeConcentrationModels,
                    sampleCompoundCollections
                );
                result.Initialize(
                    relativePotencyFactors.Keys, 
                    sampleCompoundCollections.Keys
                );
                return result;
            } else if (_settings.IsSampleBased && !_settings.UseEquivalentsModel) {
                return new SampleBasedResidueGenerator(sampleCompoundCollections);
            } else if (!_settings.IsSampleBased) {
                return new SubstanceBasedResidueGenerator(
                    concentrationModels,
                    marginalOccurrencePatterns,
                    _settings
                );
            } else {
                throw new NotImplementedException();
            }
        }
    }
}
