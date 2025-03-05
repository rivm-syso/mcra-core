using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ProcessedModelledFoodConsumptionSummarySection : SummarySection {

        private double _lowerPercentage;
        private double _upperPercentage;
        public List<ProcessedModelledFoodConsumptionDataRecord> Records { get; set; }
        public bool UseSamplingWeights { get; set; }

        /// <summary>
        ///  Weighted
        /// This is with compound information included (if needed for different conversion routes)
        /// </summary>
        /// <param name="simulatedIndividualDays"></param>
        /// <param name="consumptionsPerModelledFood"></param>
        /// <param name="lowerPercentage"></param>
        /// <param name="upperPercentage"></param>
        public void Summarize(
            ICollection<IndividualDay> simulatedIndividualDays,
            ICollection<ConsumptionsByModelledFood> consumptionsPerModelledFood,
            double lowerPercentage,
            double upperPercentage
        ) {
            var othersProcessingType = new ProcessingType() { Name = "Others" };
            var cancelToken = ProgressState?.CancellationToken ?? new();
            _lowerPercentage = lowerPercentage;
            _upperPercentage = upperPercentage;
            var percentages = new double[] { _lowerPercentage, 50, _upperPercentage, 95 };
            var totalNumberOfIndividualDays = simulatedIndividualDays.Count;
            var totalSamplingWeightsAllDays = simulatedIndividualDays.Sum(c => c.Individual.SamplingWeight);
            var result = new List<ProcessedModelledFoodConsumptionDataRecord>();
            Records = consumptionsPerModelledFood
                .AsParallel()
                .WithCancellation(cancelToken)
                .GroupBy(gr => (gr.FoodAsMeasured, ProcessingType: gr.ProcessingTypes?.LastOrDefault() ?? othersProcessingType))
                .Select(g => {
                    var groupIndividualDays = g
                        .GroupBy(gr => (gr.Individual, gr.Day))
                        .ToList();
                    var consumptionAmountsPerIndividualDay = g.GroupBy(co => (co.Day, co.Individual)).Select(gdi => gdi.Sum(di => di.AmountFoodAsMeasured));
                    var nConsumptionDays = groupIndividualDays.Count;
                    var weights = g.GroupBy(co => (co.Day, co.Individual)).Select(gdi => gdi.First().Individual.SamplingWeight).ToList();
                    var totalSamplingWeightsConsumptionDays = groupIndividualDays.Select(c => c.First().Individual.SamplingWeight).Sum();
                    var totalConsumption = g.Sum(b => b.AmountFoodAsMeasured * b.Individual.SamplingWeight) * (g.Key.FoodAsMeasured.MarketShare?.Percentage ?? 100D) / 100D;
                    var percentiles = consumptionAmountsPerIndividualDay.PercentilesWithSamplingWeights(weights, percentages);
                    var samplingWeightsZeros = totalSamplingWeightsAllDays - totalSamplingWeightsConsumptionDays;
                    var percentilesAll = consumptionAmountsPerIndividualDay.PercentilesAdditionalZeros(weights, percentages, samplingWeightsZeros);
                    return new ProcessedModelledFoodConsumptionDataRecord() {
                        FoodCode = g.Key.FoodAsMeasured.Code,
                        FoodName = g.Key.FoodAsMeasured.Name,
                        ProcessingTypeCode = g.Key.ProcessingType.Code,
                        ProcessingTypeName = g.Key.ProcessingType.Name,
                        ProportionProcessing = g.Average(r => r.ProportionProcessing),
                        MeanConsumption = totalConsumption / totalSamplingWeightsConsumptionDays,
                        Percentile25Consumption = percentiles[0],
                        MedianConsumption = percentiles[1],
                        Percentile75Consumption = percentiles[2],
                        Percentile95Consumption = percentiles[3],
                        MeanConsumptionAll = totalConsumption / totalSamplingWeightsAllDays,
                        Percentile25ConsumptionAll = percentilesAll[0],
                        MedianConsumptionAll = percentilesAll[1],
                        Percentile75ConsumptionAll = percentilesAll[2],
                        Percentile95ConsumptionAll = percentilesAll[3],
                        NumberOfConsumptionDays = nConsumptionDays,
                        TotalNumberOfIndividualDays = totalNumberOfIndividualDays,
                        TotalSamplingWeightsAllDays = totalSamplingWeightsAllDays,
                        TotalSamplingWeightsConsumptionDays = totalSamplingWeightsConsumptionDays,
                    };
                })
                .OrderBy(r => r.FoodName, StringComparer.OrdinalIgnoreCase)
                .ThenBy(r => r.FoodCode, StringComparer.OrdinalIgnoreCase)
                .ThenBy(r => r.ProcessingTypeName, StringComparer.OrdinalIgnoreCase)
                .ThenBy(r => r.ProcessingTypeCode, StringComparer.OrdinalIgnoreCase)
                .ToList();
            Records.TrimExcess();
        }
    }
}
