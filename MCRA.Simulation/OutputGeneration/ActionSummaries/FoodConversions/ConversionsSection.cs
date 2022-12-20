using MCRA.Utils.ExtensionMethods;
using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ConversionsSection : SummarySection {

        /// <summary>
        /// Summary of conversion results
        /// </summary>
        public List<ConversionSummaryRecord> Records = new List<ConversionSummaryRecord>();

        public int NumberOfMatchedFoods {
            get {
                return Records.Select(f => f.FoodAsEatenCode).Distinct().Count();
            }
        }

        /// <summary>
        /// Sumarizes food conversion results.
        /// </summary>
        /// <param name="foodConversionResults"></param>
        /// <param name="selectedSubstances"></param>
        public void Summarize(
            ICollection<FoodConversionResult> foodConversionResults,
            ICollection<Compound> selectedSubstances
        ) {
            var cancelToken = ProgressState?.CancellationToken ?? new System.Threading.CancellationToken();
            var allConversionSummaryRecords = foodConversionResults
                .AsParallel()
                .WithCancellation(cancelToken)
                .WithDegreeOfParallelism(100)
                .Select(fcr => {
                    var conversionRecord = new ConversionSummaryRecord() {
                        CompoundCode = fcr.Compound?.Code ?? string.Empty,
                        CompoundName = fcr.Compound?.Name ?? string.Empty,
                        FoodAsEatenCode = fcr.FoodAsEaten.Code,
                        FoodAsEatenName = fcr.FoodAsEaten.Name,
                        FoodAsMeasuredCode = fcr.FoodAsMeasured.Code,
                        FoodAsMeasuredName = fcr.FoodAsMeasured.Name,
                        Proportion = fcr.Proportion,
                        MarketShare = fcr.MarketShare,
                        ProcessingTypeCode = (fcr.ProcessingTypes?.Any() ?? false) ? string.Join(", ", fcr.ProcessingTypes.Select(r => r.Code)) : null,
                        ProcessingTypeName = (fcr.ProcessingTypes?.Any() ?? false) ? string.Join(", ", fcr.ProcessingTypes.Select(r => r.Name)) : null,
                        ProportionProcessedFoodAsMeasured = fcr.Proportion / fcr.ProportionProcessing,
                        ProportionProcessed = fcr.ProportionProcessing,
                        Steps = fcr.ConversionStepResults?.Count() ?? 0
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
                .ToList();

            var groupingResult = allConversionSummaryRecords
                .GroupBy(steps => steps.ConversionStepResults.ToString())
                .ToList();
            foreach (var group in groupingResult) {
                if (group.Select(d => d.CompoundCode).Count() == selectedSubstances.Count) {
                    Records.Add(new ConversionSummaryRecord() {
                        FoodAsEatenCode = group.First().FoodAsEatenCode,
                        FoodAsEatenName = group.First().FoodAsEatenName,
                        FoodAsMeasuredCode = group.First().FoodAsMeasuredCode,
                        FoodAsMeasuredName = group.First().FoodAsMeasuredName,
                        Proportion = group.First().Proportion,
                        ProcessingTypeName = group.First().ProcessingTypeName,
                        ProcessingTypeCode = group.First().ProcessingTypeCode,
                        ProportionProcessed = group.First().ProportionProcessed,
                        ProportionProcessedFoodAsMeasured = group.First().ProportionProcessedFoodAsMeasured,
                        Steps = group.First().Steps,
                        ConversionStepResults = group.First().ConversionStepResults,
                    });
                } else {
                    Records.AddRange(group);
                }
            }
            Records = Records
                .OrderBy(fcr => fcr.FoodAsEatenCode, System.StringComparer.OrdinalIgnoreCase)
                .ThenBy(fcr => fcr.FoodAsMeasuredCode, System.StringComparer.OrdinalIgnoreCase)
                .ThenBy(fcr => fcr.ConversionStepResults.ToString(), System.StringComparer.OrdinalIgnoreCase)
                .ThenBy(fcr => fcr.CompoundCode, System.StringComparer.OrdinalIgnoreCase)
                .ToList();
        }
    }
}
