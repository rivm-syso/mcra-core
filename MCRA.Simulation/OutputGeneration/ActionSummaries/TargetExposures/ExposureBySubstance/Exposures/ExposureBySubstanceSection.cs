using MCRA.Data.Compiled.Objects;
using MCRA.General;
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
            ExposureUnitTriple externalExposureUnit,
            bool skipPrivacySensitiveOutputs
        ) {
            var percentages = new double[] { lowerPercentage, 50, upperPercentage };
            var aggregateExposures = aggregateIndividualExposures != null
                ? aggregateIndividualExposures
                : aggregateIndividualDayExposures.Cast<AggregateIndividualExposure>().ToList();

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
                externalExposureUnit,
                percentages
            );

            ExposureBoxPlotRecords = summarizeBoxPlotRecords(
                aggregateExposures,
                kineticConversionFactors,
                substances,
                externalExposureUnit
            );
        }
        public List<ExposureBySubstanceRecord> summarizeExposureRecords(
            ICollection<AggregateIndividualExposure> aggregateExposures,
            ICollection<Compound> substances,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            IDictionary<(ExposureRoute, Compound), double> kineticConversionFactors,
            ExposureUnitTriple externalExposureUnit,
            double[] percentages
        ) {
            var result = new List<ExposureBySubstanceRecord>();
            foreach (var substance in substances) {
                var exposures = aggregateExposures
                    .Select(c => (
                        SamplingWeight: c.IndividualSamplingWeight,
                        Exposure: c.GetTotalExternalExposureForSubstance(
                            substance,
                            kineticConversionFactors,
                            externalExposureUnit.IsPerUnit()
                        )
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
                    Percentage = weights.Count / exposures.Count * 100D,
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
            return result;
        }

        /// <summary>
        /// Calculate summary statistics for boxplots target exposures chronic.
        /// </summary>
        private List<ExposureBySubstancePercentileRecord> summarizeBoxPlotRecords(
            ICollection<AggregateIndividualExposure> aggregateExposures,
            IDictionary<(ExposureRoute, Compound), double> kineticConversionFactors,
            ICollection<Compound> substances,
            ExposureUnitTriple externalExposureUnit
        ) {
            var cancelToken = ProgressState?.CancellationToken ?? new CancellationToken();
            var result = substances
                .AsParallel()
                .WithCancellation(cancelToken)
                .WithDegreeOfParallelism(50)
                .Select(substance => {
                    var exposures = aggregateExposures
                        .AsParallel()
                        .WithCancellation(cancelToken)
                        .Select(c => (
                            SamplingWeight: c.IndividualSamplingWeight,
                            Exposure: c.GetTotalExternalExposureForSubstance(
                                substance,
                                kineticConversionFactors,
                                externalExposureUnit.IsPerUnit()
                            )
                        ))
                        .ToList();
                    return getBoxPlotRecord(substance, exposures);
                })
                .ToList();
            return result;
        }

        private static ExposureBySubstancePercentileRecord getBoxPlotRecord(
            Compound substance,
            List<(double SamplingWeight, double Exposure)> exposures
        ) {
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
            return new ExposureBySubstancePercentileRecord() {
                SubstanceName = substance.Name,
                SubstanceCode = substance.Code,
                MinPositives = positives.Any() ? positives.Min() : 0,
                MaxPositives = positives.Any() ? positives.Max() : 0,
                Percentiles = percentiles,
                NumberOfPositives = positives.Count,
                Percentage = positives.Count * 100d / exposures.Count,
                Outliers = outliers,
                NumberOfOutLiers = outliers.Count,
            };
        }
    }
}
