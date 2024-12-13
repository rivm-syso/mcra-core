using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.CompoundResidueCollectionCalculation;
using MCRA.Simulation.Calculators.ConcentrationModelCalculation.ConcentrationModels;

namespace MCRA.Simulation.Calculators.ConcentrationModelCalculation {

    /// <summary>
    /// Factory class for generating concentration models.
    /// </summary>
    public sealed class ConcentrationModelFactory {
        private IConcentrationModelCalculationSettings _settings;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="nonDetectsHandlingMethod"></param>
        /// <param name="isFallbackMrl"></param>
        /// <param name="fractionOfLOR"></param>
        /// <param name="fractionOfMrl"></param>
        public ConcentrationModelFactory(IConcentrationModelCalculationSettings settings) {
            _settings = settings;
        }

        /// <summary>
        /// Generates a concentration model of the specified type for the specified food and substance.
        /// </summary>
        /// <param name="food"></param>
        /// <param name="substance"></param>
        /// <param name="concentrationModelType"></param>
        /// <param name="compoundResidueCollection"></param>
        /// <param name="concentrationDistribution"></param>
        /// <param name="concentrationUnit"></param>
        /// <returns></returns>
        public ConcentrationModel CreateModelAndCalculateParameters(
            Food food,
            Compound substance,
            ConcentrationModelType concentrationModelType,
            CompoundResidueCollection compoundResidueCollection,
            ConcentrationDistribution concentrationDistribution,
            ConcentrationLimit maximumResidueLimit,
            double occurrenceFrequency,
            ConcentrationUnit concentrationUnit
        ) {
            if (compoundResidueCollection == null) {
                compoundResidueCollection = new CompoundResidueCollection { Food = food, Compound = substance, };
            }

            var parametersCalculated = false;
            var model = createConcentrationModel(concentrationModelType);
            while (!parametersCalculated) {
                model.Compound = substance;
                model.ConcentrationUnit = concentrationUnit;
                model.DesiredModelType = concentrationModelType;
                model.Residues = compoundResidueCollection;
                model.ConcentrationDistribution = concentrationDistribution;
                model.NonDetectsHandlingMethod = _settings.NonDetectsHandlingMethod;
                model.FractionOfLor = _settings.NonDetectsHandlingMethod != NonDetectsHandlingMethod.ReplaceByZero ? _settings.FractionOfLor : double.NaN;
                if (maximumResidueLimit != null) {
                    var mrlUnitCorrection = maximumResidueLimit.ConcentrationUnit.GetConcentrationUnitMultiplier(concentrationUnit);
                    model.MaximumResidueLimit = mrlUnitCorrection * maximumResidueLimit.Limit;
                } else {
                    model.MaximumResidueLimit = double.NaN;
                }
                model.FractionOfMrl = _settings.IsFallbackMrl ?  _settings.FractionOfMrl : double.NaN;

                if (compoundResidueCollection.NumberOfResidues > 0) {
                    model.WeightedAgriculturalUseFraction = !(double.IsNaN(occurrenceFrequency)) ? occurrenceFrequency : 1D - compoundResidueCollection.FractionZeros;
                    model.CorrectedWeightedAgriculturalUseFraction = model.WeightedAgriculturalUseFraction;
                    model.CorrectedWeightedAgriculturalUseFraction = Math.Max(model.CorrectedWeightedAgriculturalUseFraction, compoundResidueCollection.FractionPositives);
                    model.CorrectedWeightedAgriculturalUseFraction = Math.Min(model.CorrectedWeightedAgriculturalUseFraction, 1 - compoundResidueCollection.FractionZeros);
                } else {
                    model.WeightedAgriculturalUseFraction = !double.IsNaN(occurrenceFrequency) ? occurrenceFrequency : 1D;
                    model.CorrectedWeightedAgriculturalUseFraction = model.WeightedAgriculturalUseFraction;
                }

                parametersCalculated = model.CalculateParameters();
                if (!parametersCalculated) {
                    concentrationModelType = GetFallbackConcentrationModelType(concentrationModelType, _settings.IsFallbackMrl);
                    model = createConcentrationModel(concentrationModelType);
                }
            }
            return model;
        }

        /// <summary>
        /// Creates a new concentration model instance of the specified type.
        /// </summary>
        /// <param name="modelType">The model-type to instantiate</param>
        /// <returns></returns>
        private static ConcentrationModel createConcentrationModel(ConcentrationModelType modelType) {
            ConcentrationModel model = null;
            switch (modelType) {
                case ConcentrationModelType.Empirical:
                    model = new CMEmpirical();
                    break;
                //case ConcentrationModelType.CensoredLogNormalEstimatedLOR:
                //    model = new CMCensoredLogNormalEstimatedLOR();
                //    break;
                case ConcentrationModelType.CensoredLogNormal:
                    model = new CMCensoredLogNormal();
                    break;
                case ConcentrationModelType.ZeroSpikeCensoredLogNormal:
                    model = new CMZeroSpikeCensoredLogNormal();
                    break;
                case ConcentrationModelType.NonDetectSpikeLogNormal:
                    model = new CMNonDetectSpikeLogNormal();
                    break;
                case ConcentrationModelType.NonDetectSpikeTruncatedLogNormal:
                    model = new CMNonDetectSpikeTruncatedLogNormal();
                    break;
                case ConcentrationModelType.MaximumResidueLimit:
                    model = new CMMaximumResidueLimit();
                    break;
                case ConcentrationModelType.SummaryStatistics:
                    model = new CMSummaryStatistics();
                    break;
            }
            return model;
        }

        /// <summary>
        /// Returns the fallback concentration model type for the given concentration model type and the provided
        /// concentration model settings.
        /// </summary>
        /// <param name="concentrationModelType"></param>
        /// <param name="includeMrlFallback"></param>
        /// <returns></returns>
        public static ConcentrationModelType GetFallbackConcentrationModelType(ConcentrationModelType concentrationModelType, bool includeMrlFallback) {
            if (includeMrlFallback) {
                return concentrationModelType switch {
                    ConcentrationModelType.ZeroSpikeCensoredLogNormal => ConcentrationModelType.CensoredLogNormal,
                    ConcentrationModelType.CensoredLogNormal => ConcentrationModelType.NonDetectSpikeLogNormal,
                    ConcentrationModelType.NonDetectSpikeTruncatedLogNormal => ConcentrationModelType.NonDetectSpikeLogNormal,
                    ConcentrationModelType.NonDetectSpikeLogNormal => ConcentrationModelType.MaximumResidueLimit,
                    ConcentrationModelType.SummaryStatistics => ConcentrationModelType.Empirical,
                    ConcentrationModelType.MaximumResidueLimit => ConcentrationModelType.Empirical,
                    ConcentrationModelType.Empirical => ConcentrationModelType.Empirical,
                    _ => ConcentrationModelType.Empirical,
                };
            } else {
                return concentrationModelType switch {
                    ConcentrationModelType.ZeroSpikeCensoredLogNormal => ConcentrationModelType.CensoredLogNormal,
                    ConcentrationModelType.CensoredLogNormal => ConcentrationModelType.NonDetectSpikeLogNormal,
                    ConcentrationModelType.NonDetectSpikeTruncatedLogNormal => ConcentrationModelType.NonDetectSpikeLogNormal,
                    ConcentrationModelType.NonDetectSpikeLogNormal => ConcentrationModelType.Empirical,
                    ConcentrationModelType.SummaryStatistics => ConcentrationModelType.Empirical,
                    ConcentrationModelType.Empirical => ConcentrationModelType.Empirical,
                    _ => ConcentrationModelType.Empirical,
                };
            }
        }
    }
}
