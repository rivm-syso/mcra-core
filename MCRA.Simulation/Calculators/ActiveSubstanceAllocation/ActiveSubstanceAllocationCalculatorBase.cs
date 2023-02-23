using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;
using MCRA.Simulation.Calculators.SubstanceConversionsCalculation;
using MCRA.Utils.Statistics.RandomGenerators;

namespace MCRA.Simulation.Calculators.ActiveSubstanceAllocation {

    public abstract class ActiveSubstanceAllocationCalculatorBase : IActiveSubstanceAllocationCalculator {

        protected ILookup<Compound, SubstanceConversion> _substanceConversionsLookup;
        protected IDictionary<(Food, Compound), SubstanceAuthorisation> _substanceAuthorisations;
        protected bool _useSubstanceAuthorisations;
        protected bool _retainAllAllocatedSubstancesAfterAllocation = true;
        protected bool _tryFixDuplicateAllocationInconsistencies = true;

        public ActiveSubstanceAllocationCalculatorBase(
            ICollection<SubstanceConversion> substanceConversions,
            IDictionary<(Food, Compound), SubstanceAuthorisation> substanceAuthorisations,
            bool useSubstanceAuthorisations,
            bool retainAllAllocatedSubstancesAfterAllocation,
            bool tryFixDuplicateAllocationInconsistencies
        ) {
            _substanceConversionsLookup = substanceConversions.ToLookup(r => r.MeasuredSubstance);
            _substanceAuthorisations = substanceAuthorisations;
            _useSubstanceAuthorisations = useSubstanceAuthorisations;
            _retainAllAllocatedSubstancesAfterAllocation = retainAllAllocatedSubstancesAfterAllocation;
            _tryFixDuplicateAllocationInconsistencies = tryFixDuplicateAllocationInconsistencies;
        }

        /// <summary>
        /// Apply active substance allocation to the sample substance collection.
        /// </summary>
        /// <param name="sampleCompoundCollections"></param>
        /// <param name="activeSubstances"></param>
        /// <param name="generator"></param>
        /// <param name="progressState"></param>
        /// <returns></returns>
        public List<SampleCompoundCollection> Allocate(
            ICollection<SampleCompoundCollection> sampleCompoundCollections,
            HashSet<Compound> activeSubstances,
            IRandom generator,
            CompositeProgressState progressState = null
        ) {
            return allocate(sampleCompoundCollections, activeSubstances, generator, progressState);
        }

        protected abstract ActiveSubstanceConversionRecord convert(
            SampleCompound sampleCompound,
            Food food,
            SubstanceConversionCollection substanceTranslationCollection,
            IRandom generator
        );

        private List<SampleCompoundCollection> allocate(
            ICollection<SampleCompoundCollection> sampleCompoundCollections,
            HashSet<Compound> activeSubstances,
            IRandom generator,
            CompositeProgressState progressState = null
        ) {
            var seed = generator.Next();
            // Create new active substance sample compound collection for each sample compound collection
            var cancelToken = progressState?.CancellationToken ?? new System.Threading.CancellationToken();
            var result = sampleCompoundCollections
                .AsParallel()
                .WithCancellation(cancelToken)
                .Select(sampleCompoundCollection => {
                    var newSampleCompoundRecords = new List<SampleCompoundRecord>();
                    var food = sampleCompoundCollection.Food;
                    var substanceTranslationsCalculator = new SubstanceConversionSetsCalculator();
                    var substanceTranslationSets = substanceTranslationsCalculator
                        .ComputeFoodSpecificTranslationSets(
                            food,
                            _substanceConversionsLookup,
                            _substanceAuthorisations,
                            _useSubstanceAuthorisations
                        );

                    var localGenerator = new McraRandomGenerator(RandomUtils.CreateSeed(seed, food.Code), true);
                    var orderedSampleCompoundRecords = sampleCompoundCollection.SampleCompoundRecords
                        .OrderBy(r => r.FoodSample?.Code, StringComparer.OrdinalIgnoreCase)
                        .ToList();
                    foreach (var sampleCompoundRecord in orderedSampleCompoundRecords) {
                        var authorised = true;
                        var newSampleCompounds = new List<SampleCompound>();
                        var orderedSampleCompounds = sampleCompoundRecord.SampleCompounds
                            .Select(r => r.Value)
                            .OrderBy(r => r.MeasuredSubstance.Code, StringComparer.OrdinalIgnoreCase)
                            .ToList();
                        foreach (var sampleCompound in orderedSampleCompounds) {
                            var measuredSubstance = sampleCompound.MeasuredSubstance;
                            if (substanceTranslationSets.ContainsKey(measuredSubstance)) {
                                var translationSets = substanceTranslationSets[measuredSubstance];
                                var conversionRecord = convert(sampleCompound, food, translationSets, localGenerator);
                                newSampleCompounds.AddRange(conversionRecord.ActiveSubstanceSampleCompounds);
                                authorised &= conversionRecord.Authorised;
                            } else {
                                authorised &= (!sampleCompound.IsPositiveResidue
                                    || (_substanceAuthorisations?.ContainsKey((food, measuredSubstance)) ?? true)
                                    || (food.BaseFood != null && (_substanceAuthorisations?.ContainsKey((food.BaseFood, measuredSubstance)) ?? true))
                                    );
                                newSampleCompounds.Add(sampleCompound.Clone());
                            }
                        }

                        var groupedSampleCompounds = newSampleCompounds
                            .GroupBy(r => r.ActiveSubstance)
                            .Where(r => _retainAllAllocatedSubstancesAfterAllocation || activeSubstances.Contains(r.Key))
                            .ToDictionary(r => r.Key, r => {
                                var inAnalyticalScope = r.Where(sc => !sc.IsMissingValue).ToList();
                                if (inAnalyticalScope.Any()) {
                                    if (inAnalyticalScope.Count() == 1) {
                                        return inAnalyticalScope.First();
                                    } else if (_tryFixDuplicateAllocationInconsistencies) {
                                        var positives = inAnalyticalScope
                                            .Where(c => c.IsPositiveResidue || c.IsZeroConcentration)
                                            .ToList();
                                        if (positives.Any()) {
                                            // Take the "active substance record" as a basis
                                            var record = positives
                                                .OrderByDescending(c => c.ActiveSubstance == c.MeasuredSubstance)
                                                .ThenBy(c => c.Residue)
                                                .First()
                                                .Clone();
                                            record.Residue = positives.Average(c => c.Residue);
                                            return record;
                                        } else {
                                            // Take the "active substance record" as a basis
                                            var record = inAnalyticalScope
                                                .OrderByDescending(c => c.ActiveSubstance == c.MeasuredSubstance)
                                                .ThenBy(c => c.Lor)
                                                .First()
                                                .Clone();
                                            return record;
                                        }
                                    } else {
                                        var msg = $"Unexpected substance translation in sample {sampleCompoundRecord.FoodSample?.Code}:"
                                            + $" substance {r.Key.Name} ({r.Key.Code}) is translated from multiple measured substances.";
                                        throw new Exception(msg);
                                    }
                                } else {
                                    var record = r.First();
                                    record.MeasuredSubstance = r.Key;
                                    return record;
                                }
                            });

                        newSampleCompoundRecords.Add(
                            new SampleCompoundRecord() {
                                FoodSample = sampleCompoundRecord.FoodSample,
                                SampleCompounds = groupedSampleCompounds,
                                AuthorisedUse = authorised
                            }
                        );
                    }
                    return new SampleCompoundCollection(sampleCompoundCollection.Food, newSampleCompoundRecords);
                }).ToList();

            return result;
        }
    }
}
