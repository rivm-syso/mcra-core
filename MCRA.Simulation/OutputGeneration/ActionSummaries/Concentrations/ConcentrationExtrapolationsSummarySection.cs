using MCRA.Simulation.Calculators.FoodExtrapolationsCalculation;

namespace MCRA.Simulation.OutputGeneration {

    /// <summary>
    /// Summarizes the sample origin fractions per food.
    /// </summary>
    public sealed class ConcentrationExtrapolationsSummarySection : SummarySection {

        public List<ConcentrationExtrapolationSummaryRecord> Records { get; set; }

        public void Summarize(ICollection<FoodSubstanceExtrapolationCandidates> extrapolationRecords) {
            Records = [];

            foreach (var extrapolationRecord in extrapolationRecords) {
                if (extrapolationRecord.PossibleExtrapolations.Any()) {
                    foreach (var foodExtrapolations in extrapolationRecord.PossibleExtrapolations) {
                        if (foodExtrapolations.Value.Any()) {
                            var possibleExtrapolations = extrapolationRecord.PossibleExtrapolations.SelectMany(r => r.Value).ToList();
                            foreach (var possibleExtrapolation in possibleExtrapolations) {
                                var record = new ConcentrationExtrapolationSummaryRecord() {
                                    FoodCode = extrapolationRecord.Food.Code,
                                    FoodName = extrapolationRecord.Food.Name,
                                    ActiveSubstanceCode = extrapolationRecord.Substance.Code,
                                    ActiveSubstanceName = extrapolationRecord.Substance.Name,
                                    NumberOfMeasurements = extrapolationRecord.Measurements,
                                    ExtrapolatedFoodCode = possibleExtrapolation.ExtrapolationFood?.Code,
                                    ExtrapolatedFoodName = possibleExtrapolation.ExtrapolationFood?.Name,
                                    MeasuredSubstanceCode = possibleExtrapolation.MeasuredSubstance?.Code,
                                    MeasuredSubstanceName = possibleExtrapolation.MeasuredSubstance?.Name
                                };
                                Records.Add(record);
                            }
                        } else {
                            var record = new ConcentrationExtrapolationSummaryRecord() {
                                FoodCode = extrapolationRecord.Food.Code,
                                FoodName = extrapolationRecord.Food.Name,
                                ActiveSubstanceCode = extrapolationRecord.Substance.Code,
                                ActiveSubstanceName = extrapolationRecord.Substance.Name,
                                ExtrapolatedFoodCode = foodExtrapolations.Key?.Code,
                                ExtrapolatedFoodName = foodExtrapolations.Key?.Name,
                            };
                            Records.Add(record);
                        }
                    }
                } else {
                    var record = new ConcentrationExtrapolationSummaryRecord() {
                        FoodCode = extrapolationRecord.Food.Code,
                        FoodName = extrapolationRecord.Food.Name,
                        ActiveSubstanceCode = extrapolationRecord.Substance.Code,
                        ActiveSubstanceName = extrapolationRecord.Substance.Name,
                    };
                    Records.Add(record);
                }
            }
            Records = Records
                .OrderBy(r => r.FoodName, StringComparer.OrdinalIgnoreCase)
                .ThenBy(r => r.FoodCode, StringComparer.OrdinalIgnoreCase)
                .ThenBy(r => r.ActiveSubstanceName, StringComparer.OrdinalIgnoreCase)
                .ThenBy(r => r.ActiveSubstanceCode, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }
    }
}

