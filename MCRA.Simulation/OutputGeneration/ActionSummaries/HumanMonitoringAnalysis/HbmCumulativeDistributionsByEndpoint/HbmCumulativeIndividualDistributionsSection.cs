using MCRA.General;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualConcentrationCalculation;
using MCRA.Simulation.OutputGeneration.ActionSummaries.HumanMonitoringData;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class HbmCumulativeIndividualDistributionsSection : SummarySection {

        public override bool SaveTemporaryData => true;

        public List<HbmIndividualDistributionBySubstanceRecord> Records { get; set; } = new();
        public List<HbmConcentrationsPercentilesRecord> HbmBoxPlotRecords { get; set; }

        public void Summarize(
            HbmCumulativeIndividualCollection cumulativeIndividualCollection,
            double lowerPercentage,
            double upperPercentage
        ) {
            var result = new List<HbmIndividualDistributionBySubstanceRecord>();
            var percentages = new double[] { lowerPercentage, 50, upperPercentage };
            var positives = cumulativeIndividualCollection
                .HbmCumulativeIndividualConcentrations
                .Where(c => c.CumulativeConcentration > 0)
                .Select(c => c)
                .ToList();
            var weights = positives.Select(c => c.Individual.SamplingWeight).ToList();
            var percentiles = positives.Select(c => c.CumulativeConcentration).PercentilesWithSamplingWeights(weights, percentages);

            var weightsAll = cumulativeIndividualCollection.HbmCumulativeIndividualConcentrations.Select(c => c.Individual.SamplingWeight).ToList();
            var percentilesAll = cumulativeIndividualCollection.HbmCumulativeIndividualConcentrations
                .Select(c => c.CumulativeConcentration)
                .PercentilesWithSamplingWeights(weightsAll, percentages);
            var record = new HbmIndividualDistributionBySubstanceRecord {
                Unit = cumulativeIndividualCollection.TargetUnit.GetShortDisplayName(TargetUnit.DisplayOption.AppendExpressionType),
                BiologicalMatrix = cumulativeIndividualCollection.TargetUnit.BiologicalMatrix.GetDisplayName(),
                SubstanceName = "Cumulative",
                SubstanceCode = "Cumulative",
                PercentagePositives = weights.Count / (double)cumulativeIndividualCollection.HbmCumulativeIndividualConcentrations.Count * 100,
                MeanPositives = positives.Sum(c => c.CumulativeConcentration * c.Individual.SamplingWeight) / weights.Sum(),
                LowerPercentilePositives = percentiles[0],
                MedianPositives = percentiles[1],
                UpperPercentilePositives = percentiles[2],
                LowerPercentileAll = percentilesAll[0],
                MedianAll = percentilesAll[1],
                UpperPercentileAll = percentilesAll[2],
                IndividualsWithPositiveConcentrations = weights.Count,
                MedianAllUncertaintyValues = new List<double>(),
                MeanAll = cumulativeIndividualCollection.HbmCumulativeIndividualConcentrations.Sum(c => c.CumulativeConcentration * c.Individual.SamplingWeight) / weights.Sum(),
            };
            result.Add(record);

            result = result
                 .Where(r => r.MeanPositives > 0)
                 .ToList();
            Records.AddRange(result);
            summarizeBoxPot(cumulativeIndividualCollection);
        }

        private void summarizeBoxPot(HbmCumulativeIndividualCollection cumulativeIndividualCollection) {
            var result = new List<HbmConcentrationsPercentilesRecord>();
            var percentages = new double[] { 5, 10, 25, 50, 75, 90, 95 };
            if (cumulativeIndividualCollection.HbmCumulativeIndividualConcentrations.Any(c => c.CumulativeConcentration > 0)) {
                var weights = cumulativeIndividualCollection.HbmCumulativeIndividualConcentrations.Select(c => c.Individual.SamplingWeight).ToList();
                var allExposures = cumulativeIndividualCollection.HbmCumulativeIndividualConcentrations
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
                    Description = $"Cumulative",
                    Percentiles = percentiles.ToList(),
                    NumberOfPositives = positives.Count,
                    Percentage = positives.Count * 100d / cumulativeIndividualCollection.HbmCumulativeIndividualConcentrations.Count,
                    Unit = cumulativeIndividualCollection.TargetUnit?.GetShortDisplayName()
                };
                result.Add(record);
            }
            HbmBoxPlotRecords = result;
        }

        public void SummarizeUncertainty(
            HbmCumulativeIndividualCollection cumulativeIndividualCollection,
            double lowerBound,
            double upperBound
        ) {
            var weightsAll = cumulativeIndividualCollection
                .HbmCumulativeIndividualConcentrations
                .Select(c => c.Individual.SamplingWeight)
                .ToList();
            var medianAll = cumulativeIndividualCollection
                .HbmCumulativeIndividualConcentrations
                .Select(c => c.CumulativeConcentration)
                .PercentilesWithSamplingWeights(weightsAll, 50);
            var record = Records.FirstOrDefault();
            if (record != null) {
                record.MedianAllUncertaintyValues.Add(medianAll);
                record.LowerUncertaintyBound = lowerBound;
                record.UpperUncertaintyBound = upperBound;
            }
        }
    }
}
