using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.Stratification;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.AggregateExposures;
using MCRA.Simulation.Constants;
using MCRA.Simulation.Objects;
using MCRA.Utils.Statistics;
using static MCRA.General.TargetUnit;

namespace MCRA.Simulation.OutputGeneration {

    /// <summary>
    /// Stores the total transformed exposure distribution in bins, is used for plotting the transformed exposure distribution
    /// </summary>
    public class InternalDistributionTotalSection : InternalDistributionSectionBase, IIntakeDistributionSection {
        public override bool SaveTemporaryData => true;

        private static readonly double _upperWhisker = 95;
        private static readonly double[] _percentages = [5, 10, 25, 50, 75, 90, 95];
        public bool ShowOutliers { get; set; }
        public double? RestrictedUpperPercentile { get; set; }
        public List<ExposureDistributionRecord> Records { get; set; }
        public List<ExposureDistributionPercentileRecord> BoxPlotRecords { get; set; }
        public List<ExposureDistributionPercentileRecord> StratifiedExposureBoxPlotRecords { get; set; }

        public void Summarize(
            ICollection<AggregateIndividualExposure> aggregateIndividualExposures,
            ICollection<Compound> substances,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            TargetUnit targetUnit,
            PopulationStratifier outputStratifier,
            double lowerPercentage,
            double upperPercentage,
            bool skipPrivacySensitiveOutputs,
            List<int> coExposureIds = null

        ) {
            if (substances.Count == 1) {
                relativePotencyFactors = relativePotencyFactors
                    ?? substances.ToDictionary(r => r, r => 1D);
                membershipProbabilities = membershipProbabilities
                    ?? substances.ToDictionary(r => r, r => 1D);
            }
            var percentages = new double[] { lowerPercentage, 50, upperPercentage };
            if (skipPrivacySensitiveOutputs) {
                var maxUpperPercentile = SimulationConstants.MaxUpperPercentage(aggregateIndividualExposures.Count);
                if (_upperWhisker > maxUpperPercentile) {
                    RestrictedUpperPercentile = maxUpperPercentile;
                }
            }
            ShowOutliers = !skipPrivacySensitiveOutputs;
            TargetUnit = targetUnit;

            // Total distribution section: histogram and cumulative distribution chart
            SummarizeUnstratifiedBinsGraph(
                aggregateIndividualExposures,
                relativePotencyFactors,
                membershipProbabilities,
                outputStratifier,
                targetUnit,
                coExposureIds
            );

            SummarizeStratifiedBinsGraph(
                aggregateIndividualExposures,
                relativePotencyFactors,
                membershipProbabilities,
                outputStratifier,
                targetUnit
            );

            var exposures = aggregateIndividualExposures
                .Select(c => (
                    SimulatedIndividual: c.SimulatedIndividual,
                    Exposure: c.GetTotalExposureAtTarget(
                        targetUnit.Target,
                        relativePotencyFactors,
                        membershipProbabilities
                    )
                )).ToList();

            Records = summarizeExposureRecords(
                exposures,
                percentages
            );

            if (outputStratifier != null) {
                var groups = exposures
                    .GroupBy(r => outputStratifier.GetLevel(r.SimulatedIndividual));
                var groupRecords = groups
                    .SelectMany(r => summarizeExposureRecords([.. r], percentages, r.Key));
                Records.AddRange(groupRecords);
                StratifiedExposureBoxPlotRecords = [.. groups.SelectMany(r => summarizeBoxPlotsRecords([.. r], targetUnit, r.Key))];
            }

            BoxPlotRecords = summarizeBoxPlotsRecords(
                exposures,
                targetUnit
            );
        }

        public void SummarizeUncertainty(
            ICollection<AggregateIndividualExposure> aggregateExposures,
            IDictionary<Compound, double> rpfs,
            IDictionary<Compound, double> memberships,
            TargetUnit targetUnit,
            PopulationStratifier populationStratifier,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound
        ) {
            UncertaintyLowerLimit = uncertaintyLowerBound;
            UncertaintyUpperLimit = uncertaintyUpperBound;
            {
            var exposures = aggregateExposures
                    .Select(c => (
                        Exposure: c.GetTotalExposureAtTarget(
                            targetUnit.Target,
                            rpfs,
                            memberships
                        ),
                        SimulatedIndividual: c.SimulatedIndividual
                    ));

            var weights = exposures
                .Select(c => c.SimulatedIndividual.SamplingWeight)
                .ToList();
            _percentiles.AddUncertaintyValues(exposures.Select(c => c.Exposure).PercentilesWithSamplingWeights(weights, [.. _percentiles.XValues]));
                }
            if (populationStratifier != null) {
                var exposures = aggregateExposures
                    .Select(c => (
                        Exposure: c.GetTotalExposureAtTarget(
                            targetUnit.Target,
                            rpfs,
                            memberships
                        ),
                        SimulatedIndividual: c.SimulatedIndividual,
                        StratificationLevel: populationStratifier.GetLevel(c.SimulatedIndividual).Code
                    ))
                    .GroupBy(c => c.StratificationLevel)
                    .ToList();
                foreach (var group in exposures) {
                    var weights = group
                        .Select(c => c.SimulatedIndividual.SamplingWeight)
                        .ToList();
                    var collection = StratifiedPercentiles.Where(c => c.Item1 == group.Key).FirstOrDefault().Item2;
                    if (collection != null) {
                        var percentiles = group
                            .Select(c => c.Exposure)
                            .PercentilesWithSamplingWeights(weights, [.. collection.XValues]);
                        collection.AddUncertaintyValues(percentiles);
                    }
                }
            } 
        }

        public List<ExposureDistributionRecord> summarizeExposureRecords(
            List<(SimulatedIndividual SimulatedIndividual, double Exposure)> exposures,
            double[] percentages,
            IStratificationLevel stratification = null
        ) {
            var records = new List<ExposureDistributionRecord>();
            if (exposures.Any(c => c.Exposure > 0)) {
                var record = getExposureRecord(
                    exposures,
                    percentages,
                    stratification
                );
                records.Add(record);
            }
            return records;
        }

        private List<ExposureDistributionPercentileRecord> summarizeBoxPlotsRecords(
            List<(SimulatedIndividual SimulatedIndividual, double Exposure)> exposures,
            TargetUnit targetUnit,
            IStratificationLevel stratification = null
        ) {
            var records = new List<ExposureDistributionPercentileRecord>();
            if (exposures.Any(c => c.Exposure > 0)) {
                var boxPlotRecord = getBoxPlotRecord(
                    exposures,
                    targetUnit,
                    stratification
                );
                records.Add(boxPlotRecord);
            }
            return records;
        }

        private ExposureDistributionRecord getExposureRecord(
            List<(SimulatedIndividual SimulatedIndividual, double Exposure)> exposures,
            double[] percentages,
            IStratificationLevel stratification
        ) {
            var weights = exposures.Where(c => c.Exposure > 0)
                .Select(idi => idi.SimulatedIndividual.SamplingWeight)
                .ToList();
            var percentiles = exposures.Where(c => c.Exposure > 0)
                .Select(c => c.Exposure)
                .PercentilesWithSamplingWeights(weights, percentages);
            var weightsAll = exposures
                .Select(idi => idi.SimulatedIndividual.SamplingWeight)
                .ToList();
            var percentilesAll = exposures
                .Select(c => c.Exposure)
                .PercentilesWithSamplingWeights(weightsAll, percentages);
            var total = exposures.Sum(c => c.Exposure * c.SimulatedIndividual.SamplingWeight);
            var record = new ExposureDistributionRecord {
                Stratification = stratification?.Name,
                Percentage = weights.Count / (double)exposures.Count * 100,
                MeanAll = total / weightsAll.Sum(),
                Mean = total / weights.Sum(),
                Percentile25 = percentiles[0],
                Median = percentiles[1],
                Percentile75 = percentiles[2],
                Percentile25All = percentilesAll[0],
                MedianAll = percentilesAll[1],
                Percentile75All = percentilesAll[2],
                NumberOfDays = weights.Count,
            };
            return record;
        }
        private static ExposureDistributionPercentileRecord getBoxPlotRecord(
            List<(SimulatedIndividual SimulatedIndividual, double Exposure)> exposures,
            TargetUnit targetUnit,
            IStratificationLevel stratification
        ) {
            var weights = exposures
                .Select(c => c.SimulatedIndividual.SamplingWeight)
                .ToList();
            var allExposures = exposures
                .Select(c => c.Exposure)
                .ToList();
            var percentiles = allExposures
                .PercentilesWithSamplingWeights(weights, _percentages)
                .ToList();
            var positives = allExposures
                .Where(r => r > 0)
                .ToList();
            var outliers = allExposures
                .Where(c => c > percentiles[4] + 3 * (percentiles[4] - percentiles[2])
                    || c < percentiles[2] - 3 * (percentiles[4] - percentiles[2]))
                .Select(c => c)
                .ToList();
            var record = new ExposureDistributionPercentileRecord() {
                Stratification = stratification?.Name,
                MinPositives = positives.Any() ? positives.Min() : 0,
                MaxPositives = positives.Any() ? positives.Max() : 0,
                Percentiles = percentiles,
                NumberOfPositives = positives.Count,
                Percentage = positives.Count * 100d / exposures.Count,
                Unit = targetUnit.GetShortDisplayName(DisplayOption.AppendExpressionType),
                Outliers = outliers,
                NumberOfOutLiers = outliers.Count,
            };
            return record;
        }
    }
}
