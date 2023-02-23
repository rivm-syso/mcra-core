using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;
using MCRA.General;

namespace MCRA.Simulation.Calculators.FoodExtrapolationsCalculation {
    public sealed class SampleCompoundExtrapolationCalculator {

        public enum ExtrapolationMethod {
            AddSamples,
            ReplaceMissingValues
        }

        /// <summary>
        /// Creates sample compound collections with extrapolated sample compound collections
        /// for the foods that were marked for extrapolation and for which extrapolation is
        /// possible.
        /// </summary>
        /// <param name="sampleCompoundCollections"></param>
        /// <param name="maximumConcentrationLimits"></param>
        /// <param name="substanceAuthorisations"></param>
        /// <param name="extrapolationCandidates"></param>
        /// <returns></returns>
        public static ICollection<SampleCompoundCollection> CreateExtrapolationRecords(
            Dictionary<Food, SampleCompoundCollection> sampleCompoundCollections,
            IDictionary<(Food, Compound), ConcentrationLimit> maximumConcentrationLimits,
            IDictionary<(Food, Compound), SubstanceAuthorisation> substanceAuthorisations,
            ICollection<FoodSubstanceExtrapolationCandidates> extrapolationCandidates
        ) {
            var result = new List<SampleCompoundCollection>();
            var extrapolationsByFood = extrapolationCandidates
                .Where(r => r.PossibleExtrapolations.Any())
                .GroupBy(r => r.Food);
            foreach (var extrapolatedFoods in extrapolationsByFood) {
                var extrapolationFoods = extrapolatedFoods
                    .SelectMany(r => r.PossibleExtrapolations.Keys)
                    .Distinct()
                    .ToList();
                var extrapolationRecords = new List<SampleCompoundRecord>();
                foreach (var extrapolationFood in extrapolationFoods) {
                    sampleCompoundCollections.TryGetValue(extrapolationFood, out var extrapolationSamples);
                    if (extrapolationSamples != null) {
                        foreach (var extrapolationSample in extrapolationSamples.SampleCompoundRecords) {
                            var extrapolatedSampleCompounds = extrapolationSample.SampleCompounds.Values
                                .Select(sc => {
                                    var extrapolationFoodAuthorised = (substanceAuthorisations?.ContainsKey((extrapolationFood, sc.ActiveSubstance)) ?? true);
                                    var extrapolatedFoodAuthorised = (substanceAuthorisations?.ContainsKey((extrapolatedFoods.Key, sc.ActiveSubstance)) ?? true);
                                    maximumConcentrationLimits.TryGetValue((extrapolatedFoods.Key, sc.MeasuredSubstance), out var mrlExtrapolatedFood);
                                    maximumConcentrationLimits.TryGetValue((extrapolationFood, sc.MeasuredSubstance), out var mrlExtrapolationFood);
                                    if (extrapolatedFoodAuthorised
                                        && extrapolationFoodAuthorised
                                        && mrlExtrapolatedFood != null
                                        && mrlExtrapolationFood != null
                                        && mrlExtrapolatedFood.Limit == mrlExtrapolationFood.Limit
                                        && mrlExtrapolatedFood.ConcentrationUnit == mrlExtrapolatedFood.ConcentrationUnit) {
                                        return new SampleCompound() {
                                            ActiveSubstance = sc.ActiveSubstance,
                                            MeasuredSubstance = sc.MeasuredSubstance,
                                            ResType = sc.ResType,
                                            Loq = sc.Loq,
                                            Lod = sc.Lod,
                                            Residue = sc.Residue
                                        };
                                    } else {
                                        return new SampleCompound() {
                                            ActiveSubstance = sc.ActiveSubstance,
                                            MeasuredSubstance = sc.MeasuredSubstance,
                                            ResType = ResType.MV,
                                        };

                                    }
                                }).ToList();
                            var extrapolatedSample = new SampleCompoundRecord() {
                                FoodSample = extrapolationSample.FoodSample,
                                SampleCompounds = extrapolatedSampleCompounds.ToDictionary(r => r.ActiveSubstance),
                            };
                            extrapolationRecords.Add(extrapolatedSample);
                        }
                    }
                }
                var extrapolationCollection = new SampleCompoundCollection(extrapolatedFoods.Key, extrapolationRecords);
                result.Add(extrapolationCollection);
            }
            return result;
        }
    }
}
