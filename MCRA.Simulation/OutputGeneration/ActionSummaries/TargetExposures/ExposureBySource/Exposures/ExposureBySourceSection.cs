using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.Simulation.Constants;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class ExposureBySourceSection : ExposureBySourceSectionBase {
        public override bool SaveTemporaryData => true;

        private static readonly double _upperWhisker = 95;

        private static readonly double[] _percentages = [5, 10, 25, 50, 75, 90, 95];
        public bool ShowOutliers { get; set; }
        public double? RestrictedUpperPercentile { get; set; }
        public List<ExposureBySourceRecord> Records { get; set; }
        public List<ExposureBySourcePercentileRecord> BoxPlotRecords { get; set; }
        public TargetUnit TargetUnit { get; set; }

        public void Summarize(
            ICollection<IExternalIndividualExposure> externalIndividualExposures,
            ICollection<Compound> activeSubstances,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            IDictionary<(ExposureRoute, Compound), double> kineticConversionFactors,
            double lowerPercentage,
            double upperPercentage,
            TargetUnit targetUnit,
            ExposureUnitTriple externalExposureUnit,
            bool isPerPerson,
            bool skipPrivacySensitiveOutputs
        ) {
            relativePotencyFactors = activeSubstances.Count > 1
                ? relativePotencyFactors : activeSubstances.ToDictionary(r => r, r => 1D);
            membershipProbabilities = activeSubstances.Count > 1
                ? membershipProbabilities : activeSubstances.ToDictionary(r => r, r => 1D);

            var percentages = new double[] { lowerPercentage, 50, upperPercentage };
            if (skipPrivacySensitiveOutputs) {
                var maxUpperPercentile = SimulationConstants.MaxUpperPercentage(externalIndividualExposures.Count);
                if (_upperWhisker > maxUpperPercentile) {
                    RestrictedUpperPercentile = maxUpperPercentile;
                }
            }
            ShowOutliers = !skipPrivacySensitiveOutputs;
            TargetUnit = targetUnit;

            var exposureSourceCollection = CalculateExposures(
                externalIndividualExposures,
                relativePotencyFactors,
                membershipProbabilities,
                kineticConversionFactors,
                isPerPerson
            );

            Records = summarizeExposureRecords(
                exposureSourceCollection,
                percentages
            );

            BoxPlotRecords = summarizeBoxPlotsRecords(
                exposureSourceCollection,
                externalExposureUnit
            );
        }

        public List<ExposureBySourceRecord> summarizeExposureRecords(
            List<(ExposureSource ExposureSource, List<(SimulatedIndividual SimulatedIndividual, double Exposure)> Exposures)> exposureSourceCollection,
            double[] percentages
        ) {
            var records = new List<ExposureBySourceRecord>();
            foreach (var item in exposureSourceCollection) {
                if (item.Exposures.Any(c => c.Exposure > 0)) {
                    var record = getExposureSourceRecord(
                        item.ExposureSource,
                        item.Exposures,
                        percentages
                    );
                    records.Add(record);
                }
            }
            return records;
        }

        private List<ExposureBySourcePercentileRecord> summarizeBoxPlotsRecords(
            List<(ExposureSource ExposureSource, List<(SimulatedIndividual SimulatedIndividual, double Exposure)> Exposures)> exposureSourceCollection,
            ExposureUnitTriple externalExposureUnit
        ) {
            var records = new List<ExposureBySourcePercentileRecord>();

            foreach (var item in exposureSourceCollection) {
                if (item.Exposures.Any(c => c.Exposure > 0)) {
                    var boxPlotRecord = getBoxPlotRecord(
                        item.ExposureSource,
                        item.Exposures,
                        externalExposureUnit
                    );
                    records.Add(boxPlotRecord);
                }
            }
            return records;
        }
        private ExposureBySourceRecord getExposureSourceRecord(
            ExposureSource source,
            List<(SimulatedIndividual SimulatedIndividual, double Exposure)> exposures,
            double[] percentages
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
            var record = new ExposureBySourceRecord {
                ExposureSource = source.GetShortDisplayName(),
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

        private static ExposureBySourcePercentileRecord getBoxPlotRecord(
            ExposureSource source,
            List<(SimulatedIndividual SimulatedIndividual, double Exposure)> exposures,
            ExposureUnitTriple unit
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
            var record = new ExposureBySourcePercentileRecord() {
                ExposureSource = source.GetDisplayName(),
                MinPositives = positives.Any() ? positives.Min() : 0,
                MaxPositives = positives.Any() ? positives.Max() : 0,
                Percentiles = percentiles,
                NumberOfPositives = positives.Count,
                Percentage = positives.Count * 100d / exposures.Count,
                Unit = unit.GetShortDisplayName(),
                Outliers = outliers,
                NumberOfOutLiers = outliers.Count,
            };
            return record;
        }
    }
}
