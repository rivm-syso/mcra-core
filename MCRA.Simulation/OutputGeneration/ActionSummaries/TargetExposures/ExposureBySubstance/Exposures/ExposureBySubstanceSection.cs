using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.Stratification;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.AggregateExposures;
using MCRA.Simulation.Constants;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class ExposureBySubstanceSection : SummarySection {
        public override bool SaveTemporaryData => true;

        private static readonly double[] _percentages = [5, 10, 25, 50, 75, 90, 95];
        private static readonly double _upperWhisker = 95;

        public int NumberOfIntakes { get; set; }
        public bool ShowOutliers { get; set; }
        public double? RestrictedUpperPercentile { get; set; }
        public List<ExposureBySubstanceRecord> ExposureRecords { get; set; }
        public List<ExposureBySubstancePercentileRecord> ExposureBoxPlotRecords { get; set; }
        public List<ExposureBySubstancePercentileRecord> StratifiedExposureBoxPlotRecords { get; set; }
        public TargetUnit TargetUnit { get; set; }

        public void Summarize(
            ICollection<AggregateIndividualExposure> aggregateIndividualExposures,
            ICollection<AggregateIndividualDayExposure> aggregateIndividualDayExposures,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            IDictionary<(ExposureRoute, Compound), double> kineticConversionFactors,
            ICollection<Compound> substances,
            double lowerPercentage,
            double upperPercentage,
            TargetUnit targetUnit,
            bool skipPrivacySensitiveOutputs,
            PopulationStratifier outputStratifier,
            bool isPerPerson
        ) {
            var percentages = new double[] { lowerPercentage, 50, upperPercentage };
            var aggregateExposures = aggregateIndividualExposures
                ?? aggregateIndividualDayExposures.Cast<AggregateIndividualExposure>().ToList();

            if (skipPrivacySensitiveOutputs) {
                var maxUpperPercentile = SimulationConstants.MaxUpperPercentage(aggregateExposures.Count);
                if (_upperWhisker > maxUpperPercentile) {
                    RestrictedUpperPercentile = maxUpperPercentile;
                }
            }

            ShowOutliers = !skipPrivacySensitiveOutputs;
            TargetUnit = targetUnit;
            NumberOfIntakes = aggregateExposures.Count;
            ExposureRecords = summarizeExposureRecords(
                aggregateExposures,
                substances,
                relativePotencyFactors,
                membershipProbabilities,
                kineticConversionFactors,
                isPerPerson,
                percentages
            );
            if (outputStratifier != null) {
                var groupRecords = aggregateExposures
                    .GroupBy(r => outputStratifier.GetLevel(r.SimulatedIndividual))
                    .SelectMany(r => summarizeExposureRecords(
                        [.. r],
                        substances,
                        relativePotencyFactors,
                        membershipProbabilities,
                        kineticConversionFactors,
                        isPerPerson,
                        percentages,
                        r.Key
                    ));
                ExposureRecords.AddRange(groupRecords);
            }

            ExposureBoxPlotRecords = summarizeBoxPlotRecords(
                aggregateExposures,
                kineticConversionFactors,
                substances,
                targetUnit,
                isPerPerson
            );

            if (outputStratifier != null) {
                StratifiedExposureBoxPlotRecords = summarizeBoxPlotRecords(
                    aggregateExposures,
                    kineticConversionFactors,
                    substances,
                    targetUnit,
                    isPerPerson,
                    outputStratifier
                );
            }
        }

        public List<ExposureBySubstanceRecord> summarizeExposureRecords(
            ICollection<AggregateIndividualExposure> aggregateExposures,
            ICollection<Compound> substances,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            IDictionary<(ExposureRoute, Compound), double> kineticConversionFactors,
            bool isPerPerson,
            double[] percentages,
            IStratificationLevel level = null
        ) {
            var result = new List<ExposureBySubstanceRecord>();
            foreach (var substance in substances) {
                var exposures = aggregateExposures
                    .Select(c => (
                        Exposure: c.GetTotalExternalExposureForSubstance(
                            substance,
                            kineticConversionFactors,
                            isPerPerson
                        ),
                        SamplingWeight: c.SimulatedIndividual.SamplingWeight
                    ))
                    .ToList();

                var rpf = relativePotencyFactors?[substance] ?? double.NaN;
                var membership = membershipProbabilities?[substance] ?? 1D;

                var weightsAll = exposures
                    .Select(c => c.SamplingWeight)
                    .ToList();
                var percentilesAll = exposures
                    .Select(c => c.Exposure)
                    .PercentilesWithSamplingWeights(weightsAll, percentages);
                var weights = exposures
                    .Where(c => c.Exposure > 0)
                    .Select(c => c.SamplingWeight)
                    .ToList();
                var percentiles = exposures
                    .Where(c => c.Exposure > 0)
                    .Select(c => c.Exposure)
                    .PercentilesWithSamplingWeights(weights, percentages);

                var total = exposures.Sum(c => c.Exposure * c.SamplingWeight);

                var record = new ExposureBySubstanceRecord {
                    SubstanceCode = substance.Code,
                    SubstanceName = substance.Name,
                    Stratification = level?.Name,
                    Percentage = weights.Count * 100D / exposures.Count,
                    MeanAll = total / weightsAll.Sum(),
                    Mean = total / weights.Sum(),
                    Percentile25 = percentiles[0],
                    Median = percentiles[1],
                    Percentile75 = percentiles[2],
                    Percentile25All = percentilesAll[0],
                    MedianAll = percentilesAll[1],
                    Percentile75All = percentilesAll[2],
                    RelativePotencyFactor = relativePotencyFactors?[substance] ?? double.NaN,
                    AssessmentGroupMembership = membershipProbabilities?[substance] ?? double.NaN,
                    NumberOfIndividuals = weights.Count,
                };
                result.Add(record);
            }
            return [..result
                .OrderBy(r => r.SubstanceName)
                .ThenBy(r => r.SubstanceCode)
                .ThenBy(r => r.Stratification)];
        }

        /// <summary>
        /// Calculate summary statistics for boxplots target exposures chronic.
        /// </summary>
        private List<ExposureBySubstancePercentileRecord> summarizeBoxPlotRecords(
            ICollection<AggregateIndividualExposure> aggregateExposures,
            IDictionary<(ExposureRoute, Compound), double> kineticConversionFactors,
            ICollection<Compound> substances,
            TargetUnit targetUnit,
            bool isPerPerson,
            PopulationStratifier stratifier = null
        ) {
            var cancelToken = ProgressState?.CancellationToken ?? new();
            var result = substances
                .AsParallel()
                .WithCancellation(cancelToken)
                .WithDegreeOfParallelism(50)
                .SelectMany(substance => {
                    var exposures = aggregateExposures
                        .AsParallel()
                        .WithCancellation(cancelToken)
                        .Select(c => (
                            Stratification: stratifier?.GetLevel(c.SimulatedIndividual),
                            SamplingWeight: c.SimulatedIndividual.SamplingWeight,
                            Exposure: c.GetTotalExternalExposureForSubstance(
                                substance,
                                kineticConversionFactors,
                                isPerPerson
                            )
                        ))
                        .ToList();

                    return getBoxPlotRecord(substance, exposures);
                })
                .ToList();
            return [..result
                .OrderBy(r => r.SubstanceName)
                .ThenBy(r => r.SubstanceCode)
                .ThenBy(r => r.Stratification)];
        }

        private static List<ExposureBySubstancePercentileRecord> getBoxPlotRecord(
            Compound substance,
            List<(IStratificationLevel stratification, double SamplingWeight, double Exposure)> exposureCollections
        ) {
            var exposureGroups = exposureCollections
                .GroupBy(c => c.stratification)
                .ToList();
            var results = new List<ExposureBySubstancePercentileRecord>();
            foreach (var group in exposureGroups) {
                var exposures = group.Select(c => (c.SamplingWeight, c.Exposure)).ToList();
                var weights = exposures
                    .Select(a => a.SamplingWeight)
                    .ToList();
                var allExposures = exposures
                    .Select(a => a.Exposure)
                    .ToList();
                var percentiles = allExposures
                    .PercentilesWithSamplingWeights(weights, _percentages)
                    .ToList();
                var positives = allExposures.Where(r => r > 0).ToList();
                var outliers = allExposures
                    .Where(c => c > percentiles[4] + 3 * (percentiles[4] - percentiles[2])
                        || c < percentiles[2] - 3 * (percentiles[4] - percentiles[2]))
                    .Select(c => c)
                    .ToList();

                var record = new ExposureBySubstancePercentileRecord() {
                    SubstanceName = substance.Name,
                    SubstanceCode = substance.Code,
                    Stratification = group.Key?.Name,
                    MinPositives = positives.Any() ? positives.Min() : 0,
                    MaxPositives = positives.Any() ? positives.Max() : 0,
                    Percentiles = percentiles,
                    NumberOfPositives = positives.Count,
                    Percentage = positives.Count * 100d / exposures.Count,
                    Outliers = outliers,
                    NumberOfOutLiers = outliers.Count,
                };
                results.Add(record);
            }
            return results;
        }
    }
}
