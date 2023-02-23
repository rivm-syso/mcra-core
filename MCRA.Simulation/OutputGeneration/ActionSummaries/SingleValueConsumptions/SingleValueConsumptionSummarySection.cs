using MCRA.Data.Compiled.Wrappers;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class SingleValueConsumptionSummarySection : ActionSummaryBase {

        public List<SingleValueConsumptionSummaryRecord> Records { get; set; }

        public void Summarize(
            ICollection<SingleValueConsumptionModel> singleValueConsumptionsByModelledFood
        ) {
            var cancelToken = ProgressState?.CancellationToken ?? new System.Threading.CancellationToken();
            Records = singleValueConsumptionsByModelledFood
                .Select(r => {
                    var processingTypeCode = r.ProcessingTypes != null
                        ? string.Join("-", r.ProcessingTypes?.Select(p => p.Code) ?? null)
                        : null;
                    var processingTypeName = r.ProcessingTypes != null
                        ? string.Join("-", r.ProcessingTypes?.Select(p => p.Name) ?? null)
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
