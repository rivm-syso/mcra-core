using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ConcentrationModelCalculation.ConcentrationModels;
using MCRA.Simulation.Calculators.OccurrencePatternsCalculation;

namespace MCRA.Simulation.Calculators.ResidueGeneration {
    public class ResidueGeneratorFactory {

        private readonly bool _useOccurrencePatternsForResidueGeneration;
        private readonly bool _treatMissingOccurrencePatternsAsNotOccurring;
        private readonly bool _isSampleBased;
        private readonly bool _useEquivalentsModel;
        private readonly ExposureType _exposureType;
        private readonly NonDetectsHandlingMethod _nonDetectsHandlingMethod;

        public ResidueGeneratorFactory(
            bool useOccurrencePatternsForResidueGeneration,
            bool treatMissingOccurrencePatternsAsNotOccurring,
            bool isSampleBased,
            bool useEquivalentsModel,
            ExposureType exposureType,
            NonDetectsHandlingMethod nonDetectsHandlingMethod
        ) {
            _useOccurrencePatternsForResidueGeneration = useOccurrencePatternsForResidueGeneration;
            _treatMissingOccurrencePatternsAsNotOccurring = treatMissingOccurrencePatternsAsNotOccurring;
            _isSampleBased = isSampleBased;
            _useEquivalentsModel = useEquivalentsModel;
            _exposureType = exposureType;
            _nonDetectsHandlingMethod = nonDetectsHandlingMethod;
        }

        public IResidueGenerator Create(
            IDictionary<Food, SampleCompoundCollection> sampleCompoundCollections,
            IDictionary<(Food, Compound), ConcentrationModel> concentrationModels,
            IDictionary<Food, ConcentrationModel> cumulativeConcentrationModels,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Food, List<MarginalOccurrencePattern>> marginalOccurrencePatterns
        ) {
            if (_exposureType == ExposureType.Chronic) {
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
