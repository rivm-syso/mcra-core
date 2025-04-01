using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ProcessingFactorCalculation;

namespace MCRA.Simulation.Calculators.FocalCommodityMeasurementReplacementCalculation {

    public abstract class FocalCommodityMeasurementReplacementCalculatorBase : IFocalCommodityMeasurementReplacementCalculator {

        /// <summary>
        /// Substance conversion factors.
        /// </summary>
        public ICollection<DeterministicSubstanceConversionFactor> SubstanceConversions { get; set; }

        /// <summary>
        /// Processing factors provider to be used for measurement replacement of measurements
        /// of processed variants/derivatives of the focal food.
        /// </summary>
        public IProcessingFactorProvider ProcessingFactorProvider { get; set; }

        /// <summary>
        /// When replacing measurements using sampling, this threshold states the minimal
        /// number of background measurements for the focal commodity that is required
        /// for replacement. I.e., if there is only one sample for, e.g., apples, then
        /// sample-based replacement replaces only one measurement.
        /// </summary>
        public int MinimalNumberOfReplacementSamples { get; set; } = 10;

        /// <summary>
        /// The fraction of measurements that should be replaced.
        /// </summary>
        public double FocalCommodityScenarioOccurrenceFraction { get; set; } = 1D;

        /// <summary>
        /// Optional adjustment factor for the focal food/substance concentration.
        /// </summary>
        public double FocalCommodityConcentrationAdjustmentFactor { get; set; } = 1D;

        /// <summary>
        /// Option to also replace measurements for processed derivatives of the focal
        /// foods. If this option is selected, then also processing factor corrections
        /// will be applied.
        /// </summary>
        public bool FocalCommodityIncludeProcessedDerivatives { get; set; } = true;

        public FocalCommodityMeasurementReplacementCalculatorBase(
            ICollection<DeterministicSubstanceConversionFactor> substanceConversions,
            double focalCommodityScenarioOccurrencePercentage,
            double focalCommodityConcentrationAdjustmentFactor,
            bool focalCommodityIncludeProcessedDerivatives,
            IProcessingFactorProvider processingFactorProvider
        ) {
            SubstanceConversions = substanceConversions;
            FocalCommodityScenarioOccurrenceFraction = focalCommodityScenarioOccurrencePercentage / 100D;
            FocalCommodityConcentrationAdjustmentFactor = focalCommodityConcentrationAdjustmentFactor;
            FocalCommodityIncludeProcessedDerivatives = focalCommodityIncludeProcessedDerivatives;
            ProcessingFactorProvider = processingFactorProvider;
        }

        /// <summary>
        /// Replaces substance measurements of the specified focal combinations.
        /// </summary>
        public Dictionary<Food, SampleCompoundCollection> Compute(
            IDictionary<Food, SampleCompoundCollection> baseSampleCompoundCollections,
            ICollection<(Food Food, Compound Substance)> focalCombinations,
            IRandom generator
        ) {
            var result = baseSampleCompoundCollections.Values
                .ToDictionary(r => r.Food, r => r.Clone());

            var groupedFocalCombinations = focalCombinations.GroupBy(r => r.Food);

            // Loop over groupings by food
            foreach (var focalCombinationGroup in groupedFocalCombinations) {

                // This is the focal commodity food
                var food = focalCombinationGroup.Key;

                // If the background sample collection does not contain any samples for the focal food, then fail
                if (!result.TryGetValue(food, out var baseSampleCompoundCollection)) {
                    throw new Exception($"Background concentration data does not contain samples for {food.Name} ({food.Code}).");
                }

                // Get the substances from the grouping
                var substancesForReplacement = focalCombinationGroup.Select(r => r.Substance).ToList();

                // Replace measurements in base sample compound collection by focal measurements
                replaceSampleCompoundCollectionMeasurements(
                    food,
                    substancesForReplacement,
                    baseSampleCompoundCollection,
                    generator
                );

                // Find sample compound collections for processed focal commodity
                var processedFocalFoodSampleCompoundCollections = result
                    .Where(r => r.Key != food && r.Key.BaseFood == food)
                    .ToList();

                // Also perform measurement replacement for the focal food sample compound collections
                if (FocalCommodityIncludeProcessedDerivatives) {
                    foreach (var collection in processedFocalFoodSampleCompoundCollections) {
                        replaceSampleCompoundCollectionMeasurements(
                            food,
                            substancesForReplacement,
                            collection.Value,
                            generator
                        );
                    }
                }
            }

            return result;
        }

        private void replaceSampleCompoundCollectionMeasurements(
            Food focalFood,
            List<Compound> focalSubstances,
            SampleCompoundCollection baseSampleCompoundCollection,
            IRandom generator
        ) {
            // If background concentration dataset contains too few samples for replacement, then fail
            if (baseSampleCompoundCollection.SampleCompoundRecords.Count < MinimalNumberOfReplacementSamples) {
                throw new Exception($"Too few samples for {baseSampleCompoundCollection.Food.Name} ({baseSampleCompoundCollection.Food.Code}) in the background data set (should be at least {MinimalNumberOfReplacementSamples} whereas {baseSampleCompoundCollection.SampleCompoundRecords.Count} are available).");
            }

            // Get substance conversions (if specified)
            var substanceConversionsLookup = createSubstanceConversionFactorsLookup(focalFood);

            // Get the sample compound records that will replace the background measurements
            var imputationRecords = drawImputationRecords(
                focalFood,
                focalSubstances,
                baseSampleCompoundCollection.SampleCompoundRecords.Count,
                FocalCommodityScenarioOccurrenceFraction,
                substanceConversionsLookup,
                generator
            );

            // Apply processing correction
            if (FocalCommodityIncludeProcessedDerivatives
                && focalFood != baseSampleCompoundCollection.Food
                && ProcessingFactorProvider != null
            ) {
                var processingType = baseSampleCompoundCollection.Food.ProcessingTypes.First();
                var substanceProcessingCorrections = focalSubstances
                    .Select(r => (Substance: r, Factor: ProcessingFactorProvider.GetNominalProcessingFactor(focalFood, r, processingType)))
                    .Where(r => !double.IsNaN(r.Factor))
                    .ToDictionary(r => r.Substance, r => r.Factor);
                foreach (var imputationRecord in imputationRecords) {
                    foreach (var substanceProcessingCorrection in substanceProcessingCorrections) {
                        if (imputationRecord.SampleCompounds.TryGetValue(substanceProcessingCorrection.Key, out var sampleCompound)) {
                            var pf = substanceProcessingCorrection.Value;
                            sampleCompound.Lod *= pf;
                            sampleCompound.Loq *= pf;
                            sampleCompound.Residue *= pf;
                        }
                    }
                }
            }

            // Randomize replacement
            var randomizedRecordsToBeImputed = baseSampleCompoundCollection.SampleCompoundRecords
                .Shuffle(generator)
                .ToList();

            // Replace measurement records
            replaceFocalCommodityMeasurements(
                randomizedRecordsToBeImputed,
                imputationRecords,
                focalSubstances,
                substanceConversionsLookup
            );
        }

        protected abstract List<SampleCompoundRecord> drawImputationRecords(
            Food food,
            ICollection<Compound> substances,
            int numberOfSamples,
            double replacementFraction,
            IDictionary<Compound, List<DeterministicSubstanceConversionFactor>> substanceConversions,
            IRandom random
        );

        protected static SampleCompoundRecord createZeroConcentrationsFocalCommoditySample(
            ICollection<Compound> substances,
            IDictionary<Compound, List<DeterministicSubstanceConversionFactor>> substanceConversions
        ) {
            // Add zero-concentration active substance record(s)
            var sampleCompoundRecord = new SampleCompoundRecord() {
                SampleCompounds = []
            };
            foreach (var measuredSubstance in substances) {
                if (substanceConversions?.ContainsKey(measuredSubstance) ?? false) {
                    var activeSubstanceConversions = substanceConversions[measuredSubstance];
                    foreach (var substanceConversion in activeSubstanceConversions) {
                        var sampleCompound = new SampleCompound() {
                            MeasuredSubstance = measuredSubstance,
                            ActiveSubstance = substanceConversion.ActiveSubstance,
                            Residue = 0,
                            ResType = ResType.VAL,
                            Loq = double.NaN,
                            Lod = double.NaN,
                        };
                        sampleCompoundRecord.SampleCompounds.Add(substanceConversion.ActiveSubstance, sampleCompound);
                    }
                } else {
                    var sampleCompound = new SampleCompound() {
                        MeasuredSubstance = measuredSubstance,
                        ActiveSubstance = measuredSubstance,
                        Residue = 0,
                        ResType = ResType.VAL,
                        Loq = double.NaN,
                        Lod = double.NaN,
                    };
                    sampleCompoundRecord.SampleCompounds.Add(measuredSubstance, sampleCompound);
                }
            }

            return sampleCompoundRecord;
        }

        private Dictionary<Compound, List<DeterministicSubstanceConversionFactor>> createSubstanceConversionFactorsLookup(
            Food food
        ) {
            // Get substance conversions (if specified)
            return SubstanceConversions?
                .Where(r => r.Food == null || r.Food == food)
                .GroupBy(r => r.MeasuredSubstance)
                .ToDictionary(
                    r => r.Key,
                    g => {
                        if (g.Any(r => r.Food != null)) {
                            return g.Where(r => r.Food != null).ToList();
                        }
                        return g.ToList();
                    });
        }

        private static void replaceFocalCommodityMeasurements(
            List<SampleCompoundRecord> randomizedRecordsToBeImputed,
            List<SampleCompoundRecord> randomizedImputationRecords,
            List<Compound> substancesForReplacement,
            Dictionary<Compound, List<DeterministicSubstanceConversionFactor>> substanceConversionsLookup
        ) {
            // Replace/assign the substance concentration records to existing samples
            var counter = 0;
            while (counter < randomizedImputationRecords.Count && counter < randomizedRecordsToBeImputed.Count) {
                var recordForImputation = randomizedRecordsToBeImputed[counter];
                var imputationRecord = randomizedImputationRecords[counter];
                foreach (var measuredSubstance in substancesForReplacement) {
                    if (substanceConversionsLookup?.ContainsKey(measuredSubstance) ?? false) {
                        var activeSubstanceConversions = substanceConversionsLookup[measuredSubstance];
                        // Remove measured substance record (if present)
                        recordForImputation.SampleCompounds.Remove(measuredSubstance);
                        foreach (var substanceConversion in activeSubstanceConversions) {
                            // Add active substance record(s) (if present)
                            var activeSubstance = substanceConversion.ActiveSubstance;
                            recordForImputation.SampleCompounds[activeSubstance] = imputationRecord.SampleCompounds[activeSubstance];
                        }
                    } else {
                        recordForImputation.SampleCompounds[measuredSubstance] = imputationRecord.SampleCompounds[measuredSubstance];
                    }
                }
                counter++;
            }
        }
    }
}
