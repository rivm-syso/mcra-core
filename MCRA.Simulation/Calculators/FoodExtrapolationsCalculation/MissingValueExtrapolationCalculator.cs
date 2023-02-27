using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;

namespace MCRA.Simulation.Calculators.FoodExtrapolationsCalculation {
    public sealed class MissingValueExtrapolationCalculator {

        public static void ExtrapolateMissingValues(
            IDictionary<Food, SampleCompoundCollection> sampleCompoundCollections,
            ICollection<FoodSubstanceExtrapolationCandidates> foodSubstanceExtrapolationCandidatesCollections,
            IRandom generator
        ) {
            var result = new List<SampleCompoundCollection>();

            var candidatesByFood = foodSubstanceExtrapolationCandidatesCollections.GroupBy(r => r.Food);
            foreach (var extrapolationCandidatesCollection in candidatesByFood) {
                var food = extrapolationCandidatesCollection.Key;
                if (sampleCompoundCollections.TryGetValue(food, out var collection)) {
                    foreach (var foodSubstanceExtrapolationCandidates in extrapolationCandidatesCollection) {
                        var activeSubstance = foodSubstanceExtrapolationCandidates.Substance;
                        var imputationRecords = new List<SampleCompound>();
                        var possibleExtrapolations = foodSubstanceExtrapolationCandidates.PossibleExtrapolations.SelectMany(r => r.Value).ToList();
                        foreach (var extrapolation in possibleExtrapolations) {
                            var extrapolationFood = extrapolation.ExtrapolationFood;
                            var measuredSubstance = extrapolation.MeasuredSubstance;
                            if (sampleCompoundCollections.TryGetValue(extrapolationFood, out var extrapolationCollection)) {
                                var extrapolationRecords = extrapolationCollection.SampleCompoundRecords
                                    .Where(r => r.SampleCompounds.TryGetValue(activeSubstance, out var sampleCompound)
                                        && !sampleCompound.IsMissingValue
                                        && sampleCompound.MeasuredSubstance == measuredSubstance)
                                    .Select(r => r.SampleCompounds[activeSubstance])
                                    .ToList();
                                imputationRecords.AddRange(extrapolationRecords);
                            }
                        }

                        var recordsToBeImputed = collection.SampleCompoundRecords
                        .Where(r => !r.SampleCompounds.ContainsKey(activeSubstance) || r.SampleCompounds[activeSubstance].IsMissingValue)
                        .ToList();

                        var randomizedRecordsToBeImputed = recordsToBeImputed
                            .Shuffle(generator)
                            .ToList();
                        var randomizedImputationRecords = imputationRecords
                            .Shuffle(generator)
                            .ToList();

                        var counter = 0;
                        while (counter < randomizedImputationRecords.Count && counter < randomizedRecordsToBeImputed.Count) {
                            var newSampleCompound = randomizedImputationRecords[counter].Clone();
                            newSampleCompound.IsExtrapolated = true;
                            randomizedRecordsToBeImputed[counter].SampleCompounds[activeSubstance] = newSampleCompound;
                            counter++;
                        }
                    }
                }
            }
        }
    }
}
