using MCRA.General;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualDayConcentrationCalculation;
using MCRA.Simulation.OutputGeneration.ActionSummaries.HumanMonitoringData;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class HbmCumulativeIndividualDayDistributionsSection : SummarySection {

        public List<HbmIndividualDayDistributionBySubstanceRecord> Records { get; set; } = new();
        public List<HbmConcentrationsPercentilesRecord> HbmBoxPlotRecords { get; set; }

        public void Summarize(
            ICollection<HbmCumulativeIndividualDayCollection> cumulativeIndividualDayCollections,
            double lowerPercentage,
            double upperPercentage
        ) {
            foreach (var collection in cumulativeIndividualDayCollections) {
                var result = new List<HbmIndividualDayDistributionBySubstanceRecord>();
                var percentages = new double[] { lowerPercentage, 50, upperPercentage };

                var weights = collection.HbmCumulativeIndividualDayConcentrations.Where(c => c.CumulativeConcentration > 0)
                    .Select(c => c.Individual.SamplingWeight).ToList();
                var percentiles = collection.HbmCumulativeIndividualDayConcentrations.Where(c => c.CumulativeConcentration > 0)
                    .Select(c => c.CumulativeConcentration)
                    .PercentilesWithSamplingWeights(weights, percentages);

                var weightsAll = collection.HbmCumulativeIndividualDayConcentrations.Select(c => c.Individual.SamplingWeight).ToList();
                var percentilesAll = collection.HbmCumulativeIndividualDayConcentrations
                    .Select(c => c.CumulativeConcentration)
                    .PercentilesWithSamplingWeights(weightsAll, percentages);
                var record = new HbmIndividualDayDistributionBySubstanceRecord {
                    BiologicalMatrix = collection.TargetUnit.BiologicalMatrix.GetDisplayName(),
                    SubstanceName = "Cumulative",
                    SubstanceCode = "Cumulative",
                    PercentagePositives = weights.Count / (double)cumulativeIndividualDayCollections.Count * 100,
                    MeanPositives = collection.HbmCumulativeIndividualDayConcentrations.Sum(c => c.CumulativeConcentration * c.Individual.SamplingWeight) / weights.Sum(),
                    LowerPercentilePositives = percentiles[0],
                    Median = percentiles[1],
                    UpperPercentilePositives = percentiles[2],
                    LowerPercentileAll = percentilesAll[0],
                    MedianAll = percentilesAll[1],
                    UpperPercentileAll = percentilesAll[2],
                    NumberOfDays = weights.Count,
                };
                result.Add(record);

                result = result
                     .Where(r => r.MeanPositives > 0)
                     .ToList();
                Records.AddRange(result);
            }
            summarizeBoxPot(cumulativeIndividualDayCollections);
        }

        private void summarizeBoxPot(
            ICollection<HbmCumulativeIndividualDayCollection> cumulativeIndividualDayCollections) {
            var result = new List<HbmConcentrationsPercentilesRecord>();
            var percentages = new double[] { 5, 10, 25, 50, 75, 90, 95 };
            foreach (var collection in cumulativeIndividualDayCollections) {
                if (collection.HbmCumulativeIndividualDayConcentrations.Any(c => c.CumulativeConcentration > 0)) {
                    var weights = collection.HbmCumulativeIndividualDayConcentrations
                        .Select(c => c.Individual.SamplingWeight)
                        .ToList();
                    var allExposures = collection.HbmCumulativeIndividualDayConcentrations
                        .Select(c => c.CumulativeConcentration)
                        .ToList();
                    var percentiles = allExposures
                        .PercentilesWithSamplingWeights(weights, percentages)
                        .ToList();
                    var positives = allExposures.Where(r => r > 0).ToList();
                    var record = new HbmConcentrationsPercentilesRecord() {
                        MinPositives = positives.Any() ? positives.Min() : 0,
                        MaxPositives = positives.Any() ? positives.Max() : 0,
                        SubstanceCode = "cumulative",
                        SubstanceName = "Cumulative",
                        Description = $"cumulative -{collection.TargetUnit.BiologicalMatrix}-{collection.TargetUnit.ExpressionType}",
                        Percentiles = percentiles.ToList(),
                        NumberOfPositives = positives.Count,
                        Percentage = positives.Count * 100d / collection.HbmCumulativeIndividualDayConcentrations.Count
                    };
                    result.Add(record);
                }
            }
            HbmBoxPlotRecords = result;
        }
    }
}
