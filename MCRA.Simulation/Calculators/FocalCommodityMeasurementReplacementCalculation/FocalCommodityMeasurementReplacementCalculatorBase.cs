using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;
using MCRA.General;

namespace MCRA.Simulation.Calculators.FocalCommodityMeasurementReplacementCalculation {

    public abstract class FocalCommodityMeasurementReplacementCalculatorBase : IFocalCommodityMeasurementReplacementCalculator {

        /// <summary>
        /// Substance conversion factors.
        /// </summary>
        public ICollection<DeterministicSubstanceConversionFactor> SubstanceConversions { get; set; }

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

        public FocalCommodityMeasurementReplacementCalculatorBase(
            ICollection<DeterministicSubstanceConversionFactor> substanceConversions,
            double focalCommodityScenarioOccurrencePercentage,
            double focalCommodityConcentrationAdjustmentFactor
        ) {
            SubstanceConversions = substanceConversions;
            FocalCommodityScenarioOccurrenceFraction = focalCommodityScenarioOccurrencePercentage / 100D;
            FocalCommodityConcentrationAdjustmentFactor = focalCommodityConcentrationAdjustmentFactor;
        }

        /// <summary>
        /// Replaces substance measurements of the specified focal combinations.
        /// </summary>
        /// <param name="baseSampleCompoundCollections"></param>
        /// <param name="focalCombinations"></param>
        /// <param name="generator"></param>
        /// <returns></returns>
        /// <summary>
        /// Replaces substance measurements of the specified focal combinations.
        /// </summary>
        /// <param name="baseSampleCompoundCollections"></param>
        /// <param name="focalCombinations"></param>
        /// <param name="generator"></param>
        /// <returns></returns>
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

                // Get the substances from the grouping
                var substancesForReplacement = focalCombinationGroup.Select(r => r.Substance).ToList();

                // If the background sample collection does not even contain samples for the focal food, then fail
                if (!result.TryGetValue(food, out var baseSampleCompoundCollection)) {
                    throw new Exception($"Background concentration data does not contain samples for {food.Name} ({food.Code}).");
                }

                // If background concentration dataset contains too few samples for replacement, fail
                if (baseSampleCompoundCollection.SampleCompoundRecords.Count < MinimalNumberOfReplacementSamples) {
                    throw new Exception($"Too few samples for {food.Name} ({food.Code}) in the background data set (should be at least {MinimalNumberOfReplacementSamples} whereas {baseSampleCompoundCollection.SampleCompoundRecords.Count} are available).");
                }

                // Get substance conversions (if specified)
                var substanceConversionsLookup = createSubstanceConversionFactorsLookup(food);

                // Get the sample compound records that will replace the background measurements
                var randomizedImputationRecords = drawImputationRecords(
                    food,
                    substancesForReplacement,
                    baseSampleCompoundCollection.SampleCompoundRecords.Count,
                    FocalCommodityScenarioOccurrenceFraction,
                    substanceConversionsLookup,
                    generator
                );

                // Randomize replacement
                var randomizedRecordsToBeImputed = baseSampleCompoundCollection.SampleCompoundRecords
                    .Shuffle(generator)
                    .ToList();

                // Replace measurement records
                replaceFocalCommodityMeasurements(
                    randomizedRecordsToBeImputed,
                    randomizedImputationRecords,
                    substancesForReplacement,
                    substanceConversionsLookup);
            }

            return result;
        }

        protected abstract List<SampleCompoundRecord> drawImputationRecords(
            Food food,
            ICollection<Compound> substances,
            int numberOfSamples,
            double replacementFraction,
            IDictionary<Compound, List<DeterministicSubstanceConversionFactor>> substanceConversions,
            IRandom random
        );


        protected static SampleCompoundRecord createZeroConcentrationsFocalCommoditySample(ICollection<Compound> substances, IDictionary<Compound, List<DeterministicSubstanceConversionFactor>> substanceConversions) {
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
