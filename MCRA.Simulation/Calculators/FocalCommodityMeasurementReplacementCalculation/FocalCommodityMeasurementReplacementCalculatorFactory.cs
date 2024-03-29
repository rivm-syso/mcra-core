﻿using MCRA.Utils.ExtensionMethods;
using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;
using MCRA.General;

namespace MCRA.Simulation.Calculators.FocalCommodityMeasurementReplacementCalculation {

    public sealed class FocalCommodityMeasurementReplacementCalculatorFactory {

        private IFocalCommodityMeasurementReplacementCalculatorFactorySettings _settings;
        public FocalCommodityMeasurementReplacementCalculatorFactory(IFocalCommodityMeasurementReplacementCalculatorFactorySettings settings) {
            _settings = settings;
        }

        /// <summary>
        /// Creates a focal commodity replacement calculator that can be used to replace/remove
        /// substance measurements from a background concentration dataset by focal commodity
        /// measurements.
        /// </summary>
        /// <param name="focalSampleCompoundCollections"></param>
        /// <param name="maximumConcentrationLimits"></param>
        /// <param name="substanceConversions"></param>
        /// <param name="concentrationUnit"></param>
        /// <returns></returns>
        public IFocalCommodityMeasurementReplacementCalculator Create(
            ICollection<SampleCompoundCollection> focalSampleCompoundCollections,
            IDictionary<(Food, Compound), ConcentrationLimit> maximumConcentrationLimits,
            ICollection<DeterministicSubstanceConversionFactor> substanceConversions,
            ConcentrationUnit concentrationUnit
        ) {
            switch (_settings.FocalCommodityReplacementMethod) {
                case FocalCommodityReplacementMethod.ReplaceSubstances:
                    return new FocalCommodityMeasurementBySamplesReplacementCalculator(
                        focalSampleCompoundCollections?.ToDictionary(r => r.Food),
                        substanceConversions,
                        _settings.FocalCommodityScenarioOccurrencePercentage,
                        _settings.FocalCommodityConcentrationAdjustmentFactor
                    );
                case FocalCommodityReplacementMethod.MeasurementRemoval:
                    return new FocalCommodityMeasurementRemovalCalculator();
                case FocalCommodityReplacementMethod.ReplaceSubstanceConcentrationsByLimitValue:
                    return new FocalCommodityMeasurementMrlReplacementCalculator(
                        _settings.FocalCommodityScenarioOccurrencePercentage,
                        maximumConcentrationLimits,
                        substanceConversions,
                        _settings.FocalCommodityConcentrationAdjustmentFactor,
                        concentrationUnit
                    );
                default:
                    throw new NotImplementedException($"Cannot create measurement replacement calculator for replacement method {_settings.FocalCommodityReplacementMethod.GetDisplayName()}");
            }
        }
    }
}
