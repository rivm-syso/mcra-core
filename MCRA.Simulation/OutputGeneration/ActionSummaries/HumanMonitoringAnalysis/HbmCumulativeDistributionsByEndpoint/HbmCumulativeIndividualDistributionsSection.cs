using MCRA.General;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualConcentrationCalculation;
using MCRA.Simulation.OutputGeneration.ActionSummaries.HumanMonitoringData;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class HbmCumulativeIndividualDistributionsSection : SummarySection {

        public List<HbmIndividualDistributionBySubstanceRecord> Records { get; set; } = new();
        public List<HbmConcentrationsPercentilesRecord> HbmBoxPlotRecords { get; set; }

        public void Summarize(
            ICollection<HbmCumulativeIndividualCollection> cumulativeIndividualCollections,
            double lowerPercentage,
            double upperPercentage
        ) {
            var result = new List<HbmIndividualDistributionBySubstanceRecord>();
            foreach (var collection in cumulativeIndividualCollections) {
                var percentages = new double[] { lowerPercentage, 50, upperPercentage };

                var weights = collection.HbmCumulativeIndividualConcentrations.Where(c => c.CumulativeConcentration > 0)
                    .Select(c => c.Individual.SamplingWeight).ToList();
                var percentiles = collection.HbmCumulativeIndividualConcentrations.Where(c => c.CumulativeConcentration > 0)
                    .Select(c => c.CumulativeConcentration)
                    .PercentilesWithSamplingWeights(weights, percentages);

                var weightsAll = collection.HbmCumulativeIndividualConcentrations.Select(c => c.Individual.SamplingWeight).ToList();
                var percentilesAll = collection.HbmCumulativeIndividualConcentrations
                    .Select(c => c.CumulativeConcentration)
                    .PercentilesWithSamplingWeights(weightsAll, percentages);
                var record = new HbmIndividualDistributionBySubstanceRecord {
                    BiologicalMatrix = collection.TargetUnit.BiologicalMatrix.GetDisplayName(),
                    SubstanceName = "Cumulative",
                    SubstanceCode = "Cumulative",
                    PercentagePositives = weights.Count / (double)collection.HbmCumulativeIndividualConcentrations.Count * 100,
                    MeanPositives = collection.HbmCumulativeIndividualConcentrations.Sum(c => c.CumulativeConcentration * c.Individual.SamplingWeight) / weights.Sum(),
                    LowerPercentilePositives = percentiles[0],
                    MedianPositives = percentiles[1],
                    UpperPercentilePositives = percentiles[2],
                    LowerPercentileAll = percentilesAll[0],
                    MedianAll = percentilesAll[1],
                    UpperPercentileAll = percentilesAll[2],
                    IndividualsWithPositiveConcentrations = weights.Count,
                };
                result.Add(record);

                result = result
                     .Where(r => r.MeanPositives > 0)
                     .ToList();
                Records.AddRange(result);
            }
            summarizeBoxPot(cumulativeIndividualCollections);
        }

        private void summarizeBoxPot(ICollection<HbmCumulativeIndividualCollection> cumulativeIndividualCollections) {
            var result = new List<HbmConcentrationsPercentilesRecord>();
            var percentages = new double[] { 5, 10, 25, 50, 75, 90, 95 };
            foreach (var collection in cumulativeIndividualCollections) {
                if (collection.HbmCumulativeIndividualConcentrations.Any(c => c.CumulativeConcentration > 0)) {
                    var weights = collection.HbmCumulativeIndividualConcentrations.Select(c => c.Individual.SamplingWeight).ToList();
                    var allExposures = collection.HbmCumulativeIndividualConcentrations
                        .Select(c => c.CumulativeConcentration)
                        .ToList();
                    var percentiles = allExposures
                        .PercentilesWithSamplingWeights(weights, percentages)
                        .ToList();
                    var positives = allExposures.Where(r => r > 0).ToList();
                    var record = new HbmConcentrationsPercentilesRecord() {
                        MinPositives = positives.Any() ? positives.Min() : 0,
                        MaxPositives = positives.Any() ? positives.Max() : 0,
                        SubstanceCode = "Cumulative",
                        SubstanceName = "Cumulative",
                        Description = $"cumulative - {collection.TargetUnit.BiologicalMatrix}-{collection.TargetUnit.ExpressionType}",
                        Percentiles = percentiles.ToList(),
                        NumberOfPositives = positives.Count,
                        Percentage = positives.Count * 100d / collection.HbmCumulativeIndividualConcentrations.Count
                    };
                    result.Add(record);
                }
            }
            HbmBoxPlotRecords = result;
        }
    }
}
