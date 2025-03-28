﻿using MCRA.General;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualConcentrationCalculation;
using MCRA.Simulation.Constants;
using MCRA.Simulation.OutputGeneration.ActionSummaries.HumanMonitoringData;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class HbmCumulativeIndividualDistributionsSection : SummarySection {
        private readonly double _upperWhisker = 95;
        public override bool SaveTemporaryData => true;

        public List<HbmIndividualDistributionBySubstanceRecord> Records { get; set; } = [];
        public List<HbmConcentrationsPercentilesRecord> HbmBoxPlotRecords { get; set; }
        public double? RestrictedUpperPercentile { get; set; }

        public void Summarize(
            HbmCumulativeIndividualCollection collection,
            double lowerPercentage,
            double upperPercentage,
            bool skipPrivacySensitiveOutputs
        ) {
            if (skipPrivacySensitiveOutputs) {
                var maxUpperPercentile = SimulationConstants.MaxUpperPercentage(collection.HbmCumulativeIndividualConcentrations.Count);
                if (_upperWhisker > maxUpperPercentile) {
                    RestrictedUpperPercentile = maxUpperPercentile;
                }
            }
            var result = new List<HbmIndividualDistributionBySubstanceRecord>();
            var percentages = new double[] { lowerPercentage, 50, upperPercentage };
            var positives = collection
                .HbmCumulativeIndividualConcentrations
                .Where(c => c.CumulativeConcentration > 0)
                .ToList();
            var weights = positives.Select(c => c.SimulatedIndividual.SamplingWeight).ToList();
            var percentiles = positives.Select(c => c.CumulativeConcentration).PercentilesWithSamplingWeights(weights, percentages);

            var weightsAll = collection.HbmCumulativeIndividualConcentrations.Select(c => c.SimulatedIndividual.SamplingWeight).ToList();
            var percentilesAll = collection.HbmCumulativeIndividualConcentrations
                .Select(c => c.CumulativeConcentration)
                .PercentilesWithSamplingWeights(weightsAll, percentages);
            var record = new HbmIndividualDistributionBySubstanceRecord {
                Unit = collection.TargetUnit.GetShortDisplayName(TargetUnit.DisplayOption.AppendExpressionType),
                CodeTargetSurface = collection.TargetUnit.Target.Code,
                BiologicalMatrix = collection.TargetUnit.BiologicalMatrix != BiologicalMatrix.Undefined
                    ? collection.TargetUnit.BiologicalMatrix.GetDisplayName()
                    : null,
                ExposureRoute = collection.TargetUnit.ExposureRoute != ExposureRoute.Undefined
                    ? collection.TargetUnit.ExposureRoute.GetDisplayName()
                    : null,
                SubstanceName = "Cumulative",
                SubstanceCode = "Cumulative",
                PercentagePositives = weights.Count / (double)collection.HbmCumulativeIndividualConcentrations.Count * 100,
                MeanPositives = positives.Sum(c => c.CumulativeConcentration * c.SimulatedIndividual.SamplingWeight) / weights.Sum(),
                LowerPercentilePositives = percentiles[0],
                MedianPositives = percentiles[1],
                UpperPercentilePositives = percentiles[2],
                LowerPercentileAll = percentilesAll[0],
                MedianAll = percentilesAll[1],
                UpperPercentileAll = percentilesAll[2],
                IndividualsWithPositiveConcentrations = weights.Count,
                MedianAllUncertaintyValues = [],
                MeanAll = collection.HbmCumulativeIndividualConcentrations.Sum(c => c.CumulativeConcentration * c.SimulatedIndividual.SamplingWeight) / weights.Sum(),
            };
            result.Add(record);

            result = result
                 .Where(r => r.MeanPositives > 0)
                 .ToList();
            Records.AddRange(result);
            summarizeBoxPot(collection);
        }

        private void summarizeBoxPot(HbmCumulativeIndividualCollection cumulativeIndividualCollection) {
            var result = new List<HbmConcentrationsPercentilesRecord>();
            var percentages = new double[] { 5, 10, 25, 50, 75, 90, 95 };
            if (cumulativeIndividualCollection.HbmCumulativeIndividualConcentrations.Any(c => c.CumulativeConcentration > 0)) {
                var weights = cumulativeIndividualCollection.HbmCumulativeIndividualConcentrations.Select(c => c.SimulatedIndividual.SamplingWeight).ToList();
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
                .Select(c => c.SimulatedIndividual.SamplingWeight)
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
