using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ConcentrationModelCalculation.ConcentrationModels;
using MCRA.Simulation.Calculators.OccurrencePatternsCalculation;

namespace MCRA.Simulation.Calculators.ResidueGeneration {
    public class ResidueGeneratorFactory {

        private readonly bool _shorterThanLifetime;
        private readonly bool _useOccurrencePatternsForResidueGeneration;
        private readonly bool _treatMissingOccurrencePatternsAsNotOccurring;
        private readonly bool _isSampleBased;
        private readonly bool _useEquivalentsModel;
        private readonly ExposureType _exposureType;
        private readonly NonDetectsHandlingMethod _nonDetectsHandlingMethod;
        private readonly ShorterThanLifetimeResidueGenerationMethod _shorterThanLifetimeResidueGenerationMethod;

        public ResidueGeneratorFactory(
            bool useOccurrencePatternsForResidueGeneration,
            bool treatMissingOccurrencePatternsAsNotOccurring,
            bool isSampleBased,
            bool useEquivalentsModel,
            ExposureType exposureType,
            bool shorterThanLifetime,
            NonDetectsHandlingMethod nonDetectsHandlingMethod,
            ShorterThanLifetimeResidueGenerationMethod shorterThanLifetimeResidueGenerationMethod
        ) {
            _shorterThanLifetime = shorterThanLifetime;
            _useOccurrencePatternsForResidueGeneration = useOccurrencePatternsForResidueGeneration;
            _treatMissingOccurrencePatternsAsNotOccurring = treatMissingOccurrencePatternsAsNotOccurring;
            _isSampleBased = isSampleBased;
            _useEquivalentsModel = useEquivalentsModel;
            _exposureType = exposureType;
            _nonDetectsHandlingMethod = nonDetectsHandlingMethod;
            _shorterThanLifetimeResidueGenerationMethod = shorterThanLifetimeResidueGenerationMethod;
        }

        public IResidueGenerator Create(
            IDictionary<Food, SampleCompoundCollection> sampleCompoundCollections,
            IDictionary<(Food, Compound), ConcentrationModel> concentrationModels,
            IDictionary<Food, ConcentrationModel> cumulativeConcentrationModels,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Food, List<MarginalOccurrencePattern>> marginalOccurrencePatterns
        ) {
            if (_exposureType == ExposureType.Chronic 
                && (!_shorterThanLifetime || _shorterThanLifetimeResidueGenerationMethod == ShorterThanLifetimeResidueGenerationMethod.UseMeans)
            ) {
                var result = new MeanConcentrationResidueGenerator(concentrationModels);
                result.Initialize();
                return result;
            } else if (_isSampleBased && _useEquivalentsModel) {
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
            } else if (_isSampleBased && !_useEquivalentsModel) {
                return new SampleBasedResidueGenerator(sampleCompoundCollections);
            } else if (!_isSampleBased) {
                return new SubstanceBasedResidueGenerator(
                    concentrationModels,
                    marginalOccurrencePatterns,
                    _useOccurrencePatternsForResidueGeneration,
                    _treatMissingOccurrencePatternsAsNotOccurring,
                    _nonDetectsHandlingMethod
                );
            } else {
                throw new NotImplementedException();
            }
        }
    }
}
