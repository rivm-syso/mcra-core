using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation;
using MCRA.Simulation.OutputGeneration.ActionSummaries.HumanMonitoringData;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class HbmCumulativeIndividualDayDistributionsSection : SummarySection {

        public List<HbmIndividualDayDistributionBySubstanceRecord> Records { get; set; }
        public List<HbmConcentrationsPercentilesRecord> HbmBoxPlotRecords { get; set; }

        public void Summarize(
            ICollection<HbmCumulativeIndividualDayConcentration> cumulativeConcentrations,
            string biologicalMatrix,
            double lowerPercentage,
            double upperPercentage
        ) {
            var result = new List<HbmIndividualDayDistributionBySubstanceRecord>();
            var percentages = new double[] { lowerPercentage, 50, upperPercentage };

            var weights = cumulativeConcentrations.Where(c => c.CumulativeConcentration > 0)
                .Select(c => c.Individual.SamplingWeight).ToList();
            var percentiles = cumulativeConcentrations.Where(c => c.CumulativeConcentration > 0)
                .Select(c => c.CumulativeConcentration)
                .PercentilesWithSamplingWeights(weights, percentages);

            var weightsAll = cumulativeConcentrations.Select(c => c.Individual.SamplingWeight).ToList();
            var percentilesAll = cumulativeConcentrations
                .Select(c => c.CumulativeConcentration)
                .PercentilesWithSamplingWeights(weightsAll, percentages);
            var record = new HbmIndividualDayDistributionBySubstanceRecord {
                BiologicalMatrix = biologicalMatrix,
                SubstanceName = "Cumulative",
                SubstanceCode = "Cumulative",
                Percentage = weights.Count / (double)cumulativeConcentrations.Count * 100,
                Mean = cumulativeConcentrations.Sum(c => c.CumulativeConcentration * c.Individual.SamplingWeight) / weights.Sum(),
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
                 .Where(r => r.Mean > 0)
                 .ToList();
            Records = result;
            summarizeBoxPot(cumulativeConcentrations, biologicalMatrix);
        }

        private void summarizeBoxPot(
            ICollection<HbmCumulativeIndividualDayConcentration> cumulativeConcentrations,
            string biologicalMatrix
        ) {
            var result = new List<HbmConcentrationsPercentilesRecord>();
            var percentages = new double[] { 5, 10, 25, 50, 75, 90, 95 };

            if (cumulativeConcentrations.Any(c => c.CumulativeConcentration > 0)) {
                var weights = cumulativeConcentrations
                    .Select(c => c.Individual.SamplingWeight)
                    .ToList();
                var allExposures = cumulativeConcentrations
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
                    Description = $"cumulative -{biologicalMatrix}",
                    Percentiles = percentiles.ToList(),
                    NumberOfPositives = positives.Count,
                    Percentage = positives.Count * 100d / cumulativeConcentrations.Count
                };
                result.Add(record);
            }
            HbmBoxPlotRecords = result;
        }
    }
}
