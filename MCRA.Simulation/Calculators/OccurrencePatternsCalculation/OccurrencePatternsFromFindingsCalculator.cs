using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Objects;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.ProgressReporting;

namespace MCRA.Simulation.Calculators.OccurrencePatternsCalculation {

    public sealed class OccurrencePatternsFromFindingsCalculator(IOccurrencePatternsFromFindingsCalculatorSettings settings) {

        private readonly IOccurrencePatternsFromFindingsCalculatorSettings _settings = settings;

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

        public ICollection<MarginalOccurrencePattern> Compute(
            ICollection<Food> foods,
            IDictionary<Food, SampleCompoundCollection> sampleCompoundCollections,
            CompositeProgressState progressState = null
        ) {
            var cancelToken = progressState?.CancellationToken ?? new();

            var result = foods
                .AsParallel()
                .WithCancellation(cancelToken)
                .SelectMany(food => {
                    var foodRecords = new List<MarginalOccurrencePattern>();
                    if (sampleCompoundCollections.TryGetValue(food, out var foodSampleCompoundRecords)) {
                        foodRecords = computeFoodUsePatterns(foodSampleCompoundRecords.SampleCompoundRecords, food);
                    }
                    rescale(food, foodRecords, _settings.Rescale, _settings.OnlyScaleAuthorised);
                    return foodRecords;
                })
                .OrderBy(r => r.Code)
                .ToList();
            return result;
        }

        private List<MarginalOccurrencePattern> computeFoodUsePatterns(
            IList<SampleCompoundRecord> sampleCompoundRecords,
            Food food
        ) {
            //create a resulting list of samplecompoundrecords
            //with a list of substances that have a missing value
            var recordsWithMissings = sampleCompoundRecords
                .Select(scr =>
                    scr.SampleCompounds
                        .Where(s => s.Value.IsMissingValue)
                        .Select(s => s.Key)
                        .ToHashSet()
                ).ToList();

            var foodOccurrencePatterns = sampleCompoundRecords
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
                    if (_settings.Rescale && _settings.OnlyScaleAuthorised && isAuthorisedCount > 0 && isAuthorisedCount < positiveFindingsCount) {
                        throw new Exception("Unexpected: occurrence pattern from both authorised and unauthorised uses");
                    }
                    var analyticalScopeCount = recordsWithMissings.Count(rwm => rwm.Count == 0 || !rwm.Overlaps(mixtureSubstances));

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
