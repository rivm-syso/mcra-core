using MCRA.Utils.Collections;
using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Simulation.Calculators.FoodExtrapolationsCalculation {
    public sealed class FoodExtrapolationCandidatesCalculator {

        private readonly IFoodExtrapolationCandidatesCalculatorSettings _settings;

        public FoodExtrapolationCandidatesCalculator(IFoodExtrapolationCandidatesCalculatorSettings settings) {
            _settings = settings;
        }

        public ICollection<FoodSubstanceExtrapolationCandidates> ComputeExtrapolationCandidates(
            ICollection<Food> foods,
            ICollection<Compound> substances,
            IDictionary<Food, SampleCompoundCollection> samples,
            IDictionary<Food, ICollection<Food>> foodExtrapolations,
            ICollection<SubstanceConversion> substanceConversions,
            IDictionary<(Food, Compound), SubstanceAuthorisation> substanceAuthorisations,
            IDictionary<(Food, Compound), ConcentrationLimit> maximumConcentrationLimits
        ) {
            substanceAuthorisations = _settings.ConsiderAuthorisationsForExtrapolations ? substanceAuthorisations : null;
            maximumConcentrationLimits = _settings.ConsiderMrlForExtrapolations ? maximumConcentrationLimits : null;

            var result = computeDataGaps(foods, substances, samples, _settings.ThresholdForExtrapolation);
            foreach (var dataGap in result) {
                var extrapolationRecords = new List<FoodSubstanceExtrapolationCandidate>();
                if (foodExtrapolations.TryGetValue(dataGap.Food, out var fromFoods)) {
                    var toFoodAuthorised = substanceAuthorisations?.ContainsKey((dataGap.Food, dataGap.Substance)) ?? true;
                    var measuredSubstances = new List<Compound>() { dataGap.Substance };
                    if (substanceConversions != null) {
                        var residueDefinitionSubstances = substanceConversions.Where(r => r.ActiveSubstance == dataGap.Substance).Select(r => r.MeasuredSubstance);
                        measuredSubstances = measuredSubstances.Union(residueDefinitionSubstances).ToList();
                    }
                    foreach (var fromFood in fromFoods) {
                        if (!dataGap.PossibleExtrapolations.TryGetValue(fromFood, out var extrapolations)) {
                            extrapolations = new List<FoodSubstanceExtrapolationCandidate>();
                            dataGap.PossibleExtrapolations.Add(fromFood, extrapolations);
                        }
                        foreach (var measuredSubstance in measuredSubstances) {
                            var fromFoodAuthorised = substanceAuthorisations?.ContainsKey((fromFood, dataGap.Substance)) ?? true;
                            var mrlsEqual = maximumConcentrationLimits == null
                                || (maximumConcentrationLimits.TryGetValue((dataGap.Food, measuredSubstance), out var mrlToFood)
                                && maximumConcentrationLimits.TryGetValue((fromFood, measuredSubstance), out var mrlFromFood)
                                && mrlFromFood.Limit == mrlToFood.Limit
                                && mrlFromFood.ConcentrationUnit == mrlToFood.ConcentrationUnit);
                            if (toFoodAuthorised && fromFoodAuthorised && mrlsEqual) {
                                var record = new FoodSubstanceExtrapolationCandidate() {
                                    MeasuredSubstance = measuredSubstance,
                                    ExtrapolationFood = fromFood,
                                };
                                dataGap.PossibleExtrapolations[fromFood].Add(record);
                            }
                        }
                    }
                }
            }
            return result;
        }

        private static List<FoodSubstanceExtrapolationCandidates> computeDataGaps(ICollection<Food> foods, ICollection<Compound> substances, IDictionary<Food, SampleCompoundCollection> samples, int thresholdForExtrapolation) {
            var dataGaps = new List<FoodSubstanceExtrapolationCandidates>();
            foreach (var food in foods) {
                if (samples.TryGetValue(food, out var collection)) {
                    foreach (var substance in substances) {
                        var substanceRecords = collection.SampleCompoundRecords
                            .Where(r => r.SampleCompounds.ContainsKey(substance) && !r.SampleCompounds[substance].IsMissingValue)
                            .Count();
                        if (substanceRecords < thresholdForExtrapolation) {
                            var record = new FoodSubstanceExtrapolationCandidates() {
                                Food = food,
                                Substance = substance,
                                Measurements = substanceRecords
                            };
                            dataGaps.Add(record);
                        }
                    }
                } else {
                    var records = substances
                        .Select(r => new FoodSubstanceExtrapolationCandidates() {
                            Food = food,
                            Substance = r,
                            Measurements = 0
                        })
                        .ToList();
                    dataGaps.AddRange(records);
                }
            }
            return dataGaps;
        }
    }
}
