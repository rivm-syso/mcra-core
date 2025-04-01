using MCRA.Simulation.Objects;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class SingleValueConsumptionSummarySection : ActionSummarySectionBase {

        public List<SingleValueConsumptionSummaryRecord> Records { get; set; }

        public void Summarize(
            ICollection<SingleValueConsumptionModel> singleValueConsumptionsByModelledFood
        ) {
            Records = singleValueConsumptionsByModelledFood
                .Select(r => {
                    var processingTypeCode = r.ProcessingTypes != null
                        ? string.Join("-", r.ProcessingTypes?.Select(p => p.Code))
                        : null;
                    var processingTypeName = r.ProcessingTypes != null
                        ? string.Join("-", r.ProcessingTypes?.Select(p => p.Name))
                        : null;
                    return new SingleValueConsumptionSummaryRecord() {
                        FoodCode = r.Food.Code,
                        FoodName = r.Food.Name,
                        ProcessingTypeCode = processingTypeCode,
                        ProcessingTypeName = processingTypeName,
                        ProportionProcessing = r.ProcessingCorrectionFactor,
                        MeanConsumption = r.MeanConsumption,
                        MedianConsumption = r.GetPercentile(50),
                        LargePortion = r.GetPercentile(97.5),
                        Bodyweight = r.BodyWeight

                    };
                })
                .ToList();
        }
    }
}
