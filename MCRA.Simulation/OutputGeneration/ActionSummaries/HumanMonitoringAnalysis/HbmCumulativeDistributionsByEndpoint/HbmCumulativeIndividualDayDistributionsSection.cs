using MCRA.General;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualDayConcentrationCalculation;
using MCRA.Simulation.OutputGeneration.ActionSummaries.HumanMonitoringData;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class HbmCumulativeIndividualDayDistributionsSection : SummarySection {

        public override bool SaveTemporaryData => true;

        public List<HbmIndividualDayDistributionBySubstanceRecord> Records { get; set; } = new();
        public List<HbmConcentrationsPercentilesRecord> HbmBoxPlotRecords { get; set; }

        public void Summarize(
            HbmCumulativeIndividualDayCollection collection,
            double lowerPercentage,
            double upperPercentage
        ) {
            var result = new List<HbmIndividualDayDistributionBySubstanceRecord>();
            var percentages = new double[] { lowerPercentage, 50, upperPercentage };
            var positives = collection
                .HbmCumulativeIndividualDayConcentrations
                .Where(c => c.CumulativeConcentration > 0)
                .Select(c => c)
                .ToList();
            var weights = positives.Select(c => c.Individual.SamplingWeight).ToList();
            var percentiles = positives.Select(c => c.CumulativeConcentration).PercentilesWithSamplingWeights(weights, percentages);

            var weightsAll = collection.HbmCumulativeIndividualDayConcentrations.Select(c => c.Individual.SamplingWeight).ToList();
            var percentilesAll = collection.HbmCumulativeIndividualDayConcentrations
                .Select(c => c.CumulativeConcentration)
                .PercentilesWithSamplingWeights(weightsAll, percentages);
            var record = new HbmIndividualDayDistributionBySubstanceRecord {
                Unit = collection.TargetUnit.GetShortDisplayName(TargetUnit.DisplayOption.AppendExpressionType),
                BiologicalMatrix = collection.TargetUnit.BiologicalMatrix != BiologicalMatrix.Undefined
                            ? collection.TargetUnit.BiologicalMatrix.GetDisplayName()
                            : null,
                ExposureRoute = collection.TargetUnit.ExposureRoute != ExposureRouteType.Undefined
                            ? collection.TargetUnit.ExposureRoute.GetDisplayName()
                            : null,
                SubstanceName = "Cumulative",
                SubstanceCode = "Cumulative",
                PercentagePositives = weights.Count / (double)collection.HbmCumulativeIndividualDayConcentrations.Count * 100,
                MeanPositives = positives.Sum(c => c.CumulativeConcentration * c.Individual.SamplingWeight) / weights.Sum(),
                LowerPercentilePositives = percentiles[0],
                Median = percentiles[1],
                UpperPercentilePositives = percentiles[2],
                LowerPercentileAll = percentilesAll[0],
                MedianAll = percentilesAll[1],
                UpperPercentileAll = percentilesAll[2],
                NumberOfDays = weights.Count,
                MedianAllUncertaintyValues = new List<double>(),
                MeanAll = collection.HbmCumulativeIndividualDayConcentrations.Sum(c => c.CumulativeConcentration * c.Individual.SamplingWeight) / weights.Sum(),
            };
            result.Add(record);

            result = result
                 .Where(r => r.MeanPositives > 0)
                 .ToList();
            Records.AddRange(result);
            summarizeBoxPot(collection);
        }

        private void summarizeBoxPot(
            HbmCumulativeIndividualDayCollection cumulativeIndividualDayCollection
         ) {
            var result = new List<HbmConcentrationsPercentilesRecord>();
            var percentages = new double[] { 5, 10, 25, 50, 75, 90, 95 };
            if (cumulativeIndividualDayCollection.HbmCumulativeIndividualDayConcentrations.Any(c => c.CumulativeConcentration > 0)) {
                var weights = cumulativeIndividualDayCollection.HbmCumulativeIndividualDayConcentrations
                    .Select(c => c.Individual.SamplingWeight)
                    .ToList();
                var allExposures = cumulativeIndividualDayCollection.HbmCumulativeIndividualDayConcentrations
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
                    Percentage = positives.Count * 100d / cumulativeIndividualDayCollection.HbmCumulativeIndividualDayConcentrations.Count,
                    Unit = cumulativeIndividualDayCollection.TargetUnit?.GetShortDisplayName()
                };
                result.Add(record);
            }
            HbmBoxPlotRecords = result;
        }

        public void SummarizeUncertainty(
            HbmCumulativeIndividualDayCollection cumulativeIndividualDayCollections,
            double lowerBound,
            double upperBound
        ) {
            var weightsAll = cumulativeIndividualDayCollections
                .HbmCumulativeIndividualDayConcentrations
                .Select(c => c.Individual.SamplingWeight)
                .ToList();
            var medianAll = cumulativeIndividualDayCollections
                .HbmCumulativeIndividualDayConcentrations
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
