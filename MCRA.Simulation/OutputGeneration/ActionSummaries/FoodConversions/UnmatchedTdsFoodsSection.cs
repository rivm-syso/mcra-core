using MCRA.Data.Compiled.Wrappers;
using MCRA.General;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class UnmatchedTdsFoodsSection : SummarySection {

        /// <summary>
        /// Summary of failed conversion results that are not found in TDSFoodSampleCompositionTable
        /// </summary>
        public List<ConversionSummaryRecord> UnmatchedFoodsSummaryRecords = new();
        public int FoodsNotFound { get; set; }

        public void Summarize(ICollection<FoodConversionResult> failedFoodConversionResults) {
            var cancelToken = ProgressState?.CancellationToken ?? new System.Threading.CancellationToken();
            UnmatchedFoodsSummaryRecords = failedFoodConversionResults
                .Where(r => !r.ConversionStepResults.Any(s => s.Step == FoodConversionStepType.TDSCompositionExact))
                .AsParallel()
                .WithCancellation(cancelToken)
                .WithDegreeOfParallelism(100)
                .Select(fcr => {
                    var conversionRecord = new ConversionSummaryRecord() {
                        CompoundCode = fcr.Compound?.Code ?? string.Empty,
                        CompoundName = fcr.Compound?.Name ?? string.Empty,
                        FoodAsEatenCode = fcr.FoodAsEaten.Code,
                        FoodAsEatenName = fcr.FoodAsEaten.Name,
                        Steps = (fcr.ConversionStepResults?.Count ?? 0)
                    };
                    conversionRecord.ConversionStepResults = new ConversionStepRecords();
                    conversionRecord.ConversionStepResults.AddRange(fcr.ConversionStepResults
                        .Select(step => new ConversionStepRecord() {
                            FoodCodeFrom = step.FoodCodeFrom,
                            FoodCodeTo = step.FoodCodeTo,
                            Step = step.Step
                        }));
                    return conversionRecord;
                })
                .OrderBy(fcr => fcr.CompoundName, StringComparer.OrdinalIgnoreCase)
                .ThenBy(fcr => fcr.FoodAsEatenName, StringComparer.OrdinalIgnoreCase)
                .ThenBy(fcr => fcr.FoodAsMeasuredName, StringComparer.OrdinalIgnoreCase)
                .ToList();

            UnmatchedFoodsSummaryRecords.TrimExcess();
            FoodsNotFound = UnmatchedFoodsSummaryRecords.Count;
        }
    }
}
