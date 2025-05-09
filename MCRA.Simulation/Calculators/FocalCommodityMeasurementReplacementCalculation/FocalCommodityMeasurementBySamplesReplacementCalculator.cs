﻿using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ProcessingFactorCalculation;

namespace MCRA.Simulation.Calculators.FocalCommodityMeasurementReplacementCalculation {

    public sealed class FocalCommodityMeasurementBySamplesReplacementCalculator : FocalCommodityMeasurementReplacementCalculatorBase {

        /// <summary>
        /// Focal commodity sample/substance collection used for measurement replacement.
        /// </summary>
        private readonly IDictionary<Food, SampleCompoundCollection> _focalCommoditySampleCompoundCollections;

        public FocalCommodityMeasurementBySamplesReplacementCalculator(
            IDictionary<Food, SampleCompoundCollection> focalCommoditySampleCompoundCollections,
            ICollection<DeterministicSubstanceConversionFactor> substanceConversions,
            double focalCommodityScenarioOccurrencePercentage,
            double focalCommodityConcentrationAdjustmentFactor,
            bool focalCommodityIncludeProcessedDerivatives,
            IProcessingFactorProvider processingFactorProvider
        ) : base(
            substanceConversions,
            focalCommodityScenarioOccurrencePercentage,
            focalCommodityConcentrationAdjustmentFactor,
            focalCommodityIncludeProcessedDerivatives,
            processingFactorProvider
        ) {
            _focalCommoditySampleCompoundCollections = focalCommoditySampleCompoundCollections;
        }

        protected override List<SampleCompoundRecord> drawImputationRecords(
            Food food,
            ICollection<Compound> substances,
            int numberOfSamples,
            double replacementFraction,
            IDictionary<Compound, List<DeterministicSubstanceConversionFactor>> substanceConversions,
            IRandom random
        ) {

            // Try get focal commodity sample compound collection
            SampleCompoundCollection focalCommoditySampleCompoundCollection = null;
            _focalCommoditySampleCompoundCollections?.TryGetValue(food, out focalCommoditySampleCompoundCollection);

            // If no focal commodity replacement record were found, then throw exception
            if (!focalCommoditySampleCompoundCollection?.SampleCompoundRecords?.Any() ?? true) {
                throw new Exception($"No replacement measurements found for focal commodity {food.Name} ({food.Code}).");
            }
            var focalSampleCompoundRecords = focalCommoditySampleCompoundCollection.SampleCompoundRecords;

            // Create the specific number of sample compound records
            var result = new List<SampleCompoundRecord>(numberOfSamples);
            for (int i = 0; i < numberOfSamples; i++) {
                if (random.NextDouble() <= replacementFraction) {
                    // Add active substance record(s) based on focal commodity sample record
                    var sampleCompoundRecord = new SampleCompoundRecord() {
                        SampleCompounds = []
                    };

                    // Draw the focal commodity sample
                    var focalSampleCompoundRecord = focalSampleCompoundRecords[random.Next(focalSampleCompoundRecords.Count)];

                    // Loop over focal substances (at measured substance level)
                    foreach (var measuredSubstance in substances) {
                        var isMissing = !focalSampleCompoundRecord.SampleCompounds.TryGetValue(measuredSubstance, out var measuredSubstanceSampleCompound);

                        var resType = isMissing ? ResType.MV : measuredSubstanceSampleCompound.ResType;

                        if (substanceConversions?.ContainsKey(measuredSubstance) ?? false) {
                            var activeSubstanceConversions = substanceConversions[measuredSubstance];
                            foreach (var substanceConversion in activeSubstanceConversions) {
                                var sampleCompound = new SampleCompound() {
                                    MeasuredSubstance = measuredSubstance,
                                    ActiveSubstance = substanceConversion.ActiveSubstance,
                                    Residue = (measuredSubstanceSampleCompound?.IsPositiveResidue ?? false)
                                        ? FocalCommodityConcentrationAdjustmentFactor * substanceConversion.ConversionFactor * measuredSubstanceSampleCompound.Residue
                                        : double.NaN,
                                    ResType = resType,
                                    Loq = !isMissing
                                        ? substanceConversion.ConversionFactor * measuredSubstanceSampleCompound.Loq
                                        : double.NaN,
                                    Lod = !isMissing
                                        ? substanceConversion.ConversionFactor * measuredSubstanceSampleCompound.Lod
                                        : double.NaN,
                                };
                                sampleCompoundRecord.SampleCompounds.Add(substanceConversion.ActiveSubstance, sampleCompound);
                            }
                        } else {
                            var sampleCompound = new SampleCompound() {
                                MeasuredSubstance = measuredSubstance,
                                ActiveSubstance = measuredSubstance,
                                Residue = (measuredSubstanceSampleCompound?.IsPositiveResidue ?? false)
                                        ? FocalCommodityConcentrationAdjustmentFactor * measuredSubstanceSampleCompound.Residue
                                        : double.NaN,
                                ResType = resType,
                                Loq = !isMissing
                                        ? measuredSubstanceSampleCompound.Loq
                                        : double.NaN,
                                Lod = !isMissing
                                        ? measuredSubstanceSampleCompound.Lod
                                        : double.NaN,
                            };
                            sampleCompoundRecord.SampleCompounds.Add(measuredSubstance, sampleCompound);
                        }
                    }
                    result.Add(sampleCompoundRecord);
                } else {
                    // Add zero-concentration active substance record(s)
                    var sampleCompoundRecord = createZeroConcentrationsFocalCommoditySample(substances, substanceConversions);
                    result.Add(sampleCompoundRecord);
                }
            }
            return result;
        }
    }
}
