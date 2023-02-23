using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.ProgressReporting;
using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Simulation.Calculators.OccurrencePatternsCalculation {

    public sealed class OccurrencePatternsFromFindingsCalculator {

        private readonly IOccurrencePatternsFromFindingsCalculatorSettings _settings;

        private const string _sep = "\a";

        #region Helpers

        internal class SampleCompoundOccurrencePatternComparer : IEqualityComparer<SampleCompoundRecord> {
            public bool Equals(SampleCompoundRecord x, SampleCompoundRecord y) {
                return OccurrencePatternString(x) == OccurrencePatternString(y);
            }

            public int GetHashCode(SampleCompoundRecord obj) {
                return OccurrencePatternString(obj).GetChecksum();
            }

            public string OccurrencePatternString(SampleCompoundRecord record) {
                var positiveSubstanceCodes = record.SampleCompounds
                    .Where(r => r.Value.IsPositiveResidue)
                    .Select(r => r.Key.Code)
                    .OrderBy(r => r, StringComparer.OrdinalIgnoreCase);
                return string.Join(_sep, positiveSubstanceCodes);
            }
        }

        #endregion
        public OccurrencePatternsFromFindingsCalculator(IOccurrencePatternsFromFindingsCalculatorSettings settings) {
            _settings = settings;
        }

        public ICollection<MarginalOccurrencePattern> Compute(
            ICollection<Food> foods,
            ICollection<SampleCompoundCollection> sampleCompoundCollections,
            CompositeProgressState progressState = null
        ) {
            var sampleCompoundCollectionLookup = sampleCompoundCollections.ToDictionary(r => r.Food);
            var cancelToken = progressState?.CancellationToken ?? new System.Threading.CancellationToken();
            var result = foods
                .AsParallel()
                .WithCancellation(cancelToken)
                .SelectMany(food => {
                    var foodRecords = computeFoodUsePatterns(sampleCompoundCollectionLookup, food);
                    rescale(food, foodRecords, _settings.Rescale, _settings.IsOnlyScaleAuthorised);
                    return foodRecords;
                })
                .ToList();
            return result;
        }

        private List<MarginalOccurrencePattern> computeFoodUsePatterns(Dictionary<Food, SampleCompoundCollection> sampleCompoundCollectionLookup, Food food) {
            if(!sampleCompoundCollectionLookup.TryGetValue(food, out var foodSampleCompoundRecords)) {
                return new List<MarginalOccurrencePattern>();
            }
            var foodOccurrencePatterns = foodSampleCompoundRecords.SampleCompoundRecords
                .GroupBy(r => r, new SampleCompoundOccurrencePatternComparer())
                .Where(g => g.Key.SampleCompounds.Values.Any(r => r.IsPositiveResidue))
                .Select((g, i) => {
                    var mixtureSubstances = g.Key.SampleCompounds.Values
                        .Where(sc => sc.IsPositiveResidue)
                        .Select(sc => sc.ActiveSubstance)
                        .ToHashSet();
                    var positiveFindingsCount = g.Count();
                    var isAuthorisedCount = g.Count(r => r.AuthorisedUse);
                    var isAuthorised = isAuthorisedCount > 0;
                    if (_settings.Rescale && _settings.IsOnlyScaleAuthorised && isAuthorisedCount > 0 && isAuthorisedCount < positiveFindingsCount) {
                        throw new Exception("Unexpected: occurrence pattern from both authorised and unauthorised uses");
                    }
                    var analyticalScopeCount = foodSampleCompoundRecords.SampleCompoundRecords
                        .Count(fsc => mixtureSubstances.All(ms => !fsc.SampleCompounds[ms].IsMissingValue));
                    return new MarginalOccurrencePattern() {
                        Food = food,
                        Compounds = mixtureSubstances,
                        OccurrenceFraction = positiveFindingsCount / (double)analyticalScopeCount,
                        AuthorisedUse = isAuthorised,
                        AnalyticalScopeCount = analyticalScopeCount,
                        PositiveFindingsCount = positiveFindingsCount,
                        Code = $"AU {food.Name} ({i})",
                    };
                })
                .ToList();
            var total = foodOccurrencePatterns.Sum(r => r.OccurrenceFraction);
            if (total < 1D) {
                foodOccurrencePatterns.Add(new MarginalOccurrencePattern() {
                    Food = food,
                    Compounds = new HashSet<Compound>(),
                    OccurrenceFraction = 1D - total,
                    AuthorisedUse = true,
                    AnalyticalScopeCount = 0,
                    PositiveFindingsCount = 0,
                    Code = $"AU {food.Name} (empty)",
                });
            }
            return foodOccurrencePatterns;
        }

        private static void rescale(Food food, List<MarginalOccurrencePattern> foodOccurrencePatterns, bool scaleUp, bool onlyScaleAuthorised) {
            var sumOccurrenceFractions = foodOccurrencePatterns
                .Where(r => r.Compounds.Any())
                .Sum(r => r.OccurrenceFraction);

            if (sumOccurrenceFractions > 1D) {
                // Scale down all use patterns
                var useRecords = foodOccurrencePatterns.Where(r => r.Compounds.Any());
                foreach (var record in useRecords) {
                    record.OccurrenceFraction = record.OccurrenceFraction / sumOccurrenceFractions;
                }
                foodOccurrencePatterns.RemoveAll(r => !r.Compounds.Any());
            } else if (scaleUp && sumOccurrenceFractions < 1D) {
                // Scale up authorised uses to 100% total use (if possible)
                var unAuthorisedPatternsFraction = foodOccurrencePatterns
                    .Where(r => onlyScaleAuthorised && r.Compounds.Any() && !(r.AuthorisedUse ?? true))
                    .Sum(r => r.OccurrenceFraction);
                var substanceAuthorisations = foodOccurrencePatterns.Where(r => r.Compounds.Any() && (!onlyScaleAuthorised || (r.AuthorisedUse ?? true)));
                var substanceAuthorisationFraction = substanceAuthorisations.Sum(r => r.OccurrenceFraction);
                var factor = (1 - unAuthorisedPatternsFraction) / substanceAuthorisationFraction;
                foreach (var pattern in substanceAuthorisations) {
                    pattern.OccurrenceFraction *= factor;
                }
            }

            // Recompute no-use fraction (and include/exclude from use patterns)
            var noUseFraction = 1D - foodOccurrencePatterns.Where(r => r.Compounds.Any()).Sum(r => r.OccurrenceFraction);
            if (noUseFraction > 1e-10) {
                var noUseRecord = foodOccurrencePatterns.FirstOrDefault(r => !r.Compounds.Any());
                if (noUseRecord == null) {
                    noUseRecord = new MarginalOccurrencePattern() {
                        Food = food,
                        AuthorisedUse = true,
                        Code = $"No use {food.Name}",
                        OccurrenceFraction = noUseFraction
                    };
                    foodOccurrencePatterns.Add(noUseRecord);
                } else {
                    noUseRecord.OccurrenceFraction = noUseFraction;
                }
            } else {
                foodOccurrencePatterns.RemoveAll(r => !r.Compounds.Any());
            }
        }
    }
}
