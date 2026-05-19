using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.ModuleDefinitions.Interfaces;
using MCRA.Simulation.Calculators.CompoundResidueCollectionCalculation;
using MCRA.Simulation.Calculators.ConcentrationModelBuilder;
using MCRA.Simulation.Calculators.ConcentrationModelCalculation.ConcentrationModels;

namespace MCRA.Simulation.Calculators.FoodConcentrationModelBuilders {

    /// <summary>
    /// Factory class for generating concentration models.
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    public sealed class FoodConcentrationModelFactory(IConcentrationModelCalculationSettings settings) {
        private readonly IConcentrationModelCalculationSettings _settings = settings;

        /// <summary>
        /// Generates a concentration model of the specified type for the specified food and substance.
        /// </summary>
        public ConcentrationModel CreateModelAndCalculateParameters(
            Food food,
            Compound substance,
            ConcentrationModelType concentrationModelType,
            FoodSubstanceResidueCollection compoundResidueCollection,
            ConcentrationDistribution concentrationDistribution,
            ConcentrationLimit maximumResidueLimit,
            double occurrenceFrequency,
            ConcentrationUnit concentrationUnit
        ) {
            if (compoundResidueCollection == null) {
                compoundResidueCollection = new FoodSubstanceResidueCollection { Food = food, Compound = substance, };
            }

            var parametersCalculated = false;
            var model = createConcentrationModel(concentrationModelType);
            while (!parametersCalculated) {
                model.Compound = substance;
                model.Unit = new StandardConcentrationUnitSpec(concentrationUnit);
                model.DesiredModelType = concentrationModelType;
                model.Residues = compoundResidueCollection;
                model.ConcentrationDistribution = concentrationDistribution;
                model.NonDetectsHandlingMethod = _settings.NonDetectsHandlingMethod;
                model.FractionOfLor = _settings.NonDetectsHandlingMethod != NonDetectsHandlingMethod.ReplaceByZero
                    ? _settings.FractionOfLor : double.NaN;
                if (maximumResidueLimit != null) {
                    var mrlUnitCorrection = maximumResidueLimit.ConcentrationUnit.GetConcentrationUnitMultiplier(concentrationUnit);
                    model.MaximumResidueLimit = mrlUnitCorrection * maximumResidueLimit.Limit;
                } else {
                    model.MaximumResidueLimit = double.NaN;
                }
                model.FractionOfMrl = _settings.IsFallbackMrl ? _settings.FractionOfMrl : double.NaN;

                var (occurenceFraction, correctedOccurenceFraction) = ConcentrationModelBuilderBase
                    .CalculateOccurrenceFractions(occurrenceFrequency, compoundResidueCollection);
                model.OccurenceFraction = occurenceFraction;
                model.CorrectedOccurenceFraction = correctedOccurenceFraction;

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
        private static ConcentrationModel createConcentrationModel(ConcentrationModelType modelType) {
            ConcentrationModel model = null;
            switch (modelType) {
                case ConcentrationModelType.Empirical:
                    model = new CMEmpirical();
                    break;
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
                case ConcentrationModelType.Constant:
                    model = new CMConstant();
                    break;
            }
            return model;
        }

        /// <summary>
        /// Returns the fallback concentration model type for the given concentration model type and the provided
        /// concentration model settings.
        /// </summary>
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
