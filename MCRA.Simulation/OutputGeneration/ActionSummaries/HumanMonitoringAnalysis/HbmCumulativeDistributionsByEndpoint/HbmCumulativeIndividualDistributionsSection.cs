using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation;
using MCRA.Simulation.OutputGeneration.ActionSummaries.HumanMonitoringData;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class HbmCumulativeIndividualDistributionsSection : SummarySection {

        public List<HbmIndividualDistributionBySubstanceRecord> Records { get; set; }
        public List<HbmConcentrationsPercentilesRecord> HbmBoxPlotRecords { get; set; }

        public void Summarize(
            ICollection<HbmCumulativeIndividualConcentration> cumulativeConcentrations,
            HumanMonitoringSamplingMethod biologicalMatrix,
            double lowerPercentage,
            double upperPercentage
        ) {
            var result = new List<HbmIndividualDistributionBySubstanceRecord>();
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
            var record = new HbmIndividualDistributionBySubstanceRecord {
                ExposureRoute = biologicalMatrix.ExposureRoute,
                BiologicalMatrix = biologicalMatrix.Compartment,
                SamplingType = biologicalMatrix.SampleType,
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
                IndividualsWithPositiveConcentrations = weights.Count,
            };
            result.Add(record);

            result = result
                 .Where(r => r.Mean > 0)
                 .OrderBy(s => s.ExposureRoute, System.StringComparer.OrdinalIgnoreCase)
                 .ToList();
            Records = result;
            summarizeBoxPot(cumulativeConcentrations, biologicalMatrix);
        }

        private void summarizeBoxPot(
              ICollection<HbmCumulativeIndividualConcentration> cumulativeConcentrations,
              HumanMonitoringSamplingMethod biologicalMatrix
          ) {
            var result = new List<HbmConcentrationsPercentilesRecord>();
            var percentages = new double[] { 5, 10, 25, 50, 75, 90, 95 };
            var weights = cumulativeConcentrations.Select(c => c.Individual.SamplingWeight).ToList();
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
                SubstanceCode = "Cumulative",
                SubstanceName = "Cumulative",
                Description = $"cumulative - {biologicalMatrix.Compartment} - {biologicalMatrix.SampleType}",
                Percentiles = percentiles.ToList(),
                NumberOfPositives = positives.Count,
                Percentage = positives.Count * 100d / cumulativeConcentrations.Count
            };
            result.Add(record);
            HbmBoxPlotRecords = result;
        }
    }
}
