using MCRA.Utils.ExtensionMethods;
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ProcessingFactorCalculation;
using MCRA.General.ModuleDefinitions.Interfaces;

namespace MCRA.Simulation.Calculators.FocalCommodityMeasurementReplacementCalculation {

    public sealed class FocalCommodityMeasurementReplacementCalculatorFactory {

        private readonly IFocalCommodityMeasurementReplacementCalculatorFactorySettings _settings;

        public FocalCommodityMeasurementReplacementCalculatorFactory(
            IFocalCommodityMeasurementReplacementCalculatorFactorySettings settings
        ) {
            _settings = settings;
        }

        /// <summary>
        /// Creates a focal commodity replacement calculator that can be used to replace/remove
        /// substance measurements from a background concentration dataset by focal commodity
        /// measurements.
        /// </summary>
        public IFocalCommodityMeasurementReplacementCalculator Create(
            ICollection<SampleCompoundCollection> focalSampleCompoundCollections,
            IDictionary<(Food, Compound), ConcentrationLimit> maximumConcentrationLimits,
            ICollection<DeterministicSubstanceConversionFactor> substanceConversions,
            IProcessingFactorProvider processingFactorProvider,
            ConcentrationUnit concentrationUnit
        ) {
            switch (_settings.FocalCommodityReplacementMethod) {
                case FocalCommodityReplacementMethod.ReplaceSubstances:
                    return new FocalCommodityMeasurementBySamplesReplacementCalculator(
                        focalSampleCompoundCollections?.ToDictionary(r => r.Food),
                        substanceConversions,
                        _settings.FocalCommodityScenarioOccurrencePercentage,
                        _settings.FocalCommodityConcentrationAdjustmentFactor,
                        _settings.FocalCommodityIncludeProcessedDerivatives,
                        processingFactorProvider
                    );
                case FocalCommodityReplacementMethod.MeasurementRemoval:
                    return new FocalCommodityMeasurementRemovalCalculator();
                case FocalCommodityReplacementMethod.ReplaceSubstanceConcentrationsByLimitValue:
                case FocalCommodityReplacementMethod.ReplaceSubstanceConcentrationsByProposedLimitValue:
                    return new FocalCommodityMeasurementMrlReplacementCalculator(
                        _settings.FocalCommodityScenarioOccurrencePercentage,
                        maximumConcentrationLimits,
                        substanceConversions,
                        _settings.FocalCommodityIncludeProcessedDerivatives,
                        processingFactorProvider,
                        _settings.FocalCommodityConcentrationAdjustmentFactor,
                        concentrationUnit
                    );
                default:
                    throw new NotImplementedException($"Cannot create measurement replacement calculator for replacement method {_settings.FocalCommodityReplacementMethod.GetDisplayName()}");
            }
        }
    }
}
