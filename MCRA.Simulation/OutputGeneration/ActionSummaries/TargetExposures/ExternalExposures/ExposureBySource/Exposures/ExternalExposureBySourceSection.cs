using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;
using MCRA.General;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.Simulation.Constants;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class ExternalExposureBySourceSection : ExternalExposureBySourceSectionBase {
        public override bool SaveTemporaryData => true;

        private static readonly double _upperWhisker = 95;

        private static readonly double[] _percentages = [5, 10, 25, 50, 75, 90, 95];
        public bool ShowOutliers { get; set; }
        public double? RestrictedUpperPercentile { get; set; }

        public List<ExternalExposureBySourceRecord> Records { get; set; }
        public List<ExternalExposureBySourcePercentileRecord> BoxPlotRecords { get; set; }
        public ExposureUnitTriple ExposureUnit { get; set; }

        public void Summarize(
            ICollection<IExternalIndividualExposure> externalIndividualExposures,
            ICollection<Compound> activeSubstances,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            double lowerPercentage,
            double upperPercentage,
            ExposureUnitTriple externalExposureUnit,
            bool isPerPerson,
            bool skipPrivacySensitiveOutputs
        ) {
            var percentages = new double[] { lowerPercentage, 50, upperPercentage };
            relativePotencyFactors = activeSubstances.Count > 1
                ? relativePotencyFactors : activeSubstances.ToDictionary(r => r, r => 1D);
            membershipProbabilities = activeSubstances.Count > 1
                ? membershipProbabilities : activeSubstances.ToDictionary(r => r, r => 1D);

            if (skipPrivacySensitiveOutputs) {
                var maxUpperPercentile = SimulationConstants.MaxUpperPercentage(externalIndividualExposures.Count);
                if (_upperWhisker > maxUpperPercentile) {
                    RestrictedUpperPercentile = maxUpperPercentile;
                }
            }
            ShowOutliers = !skipPrivacySensitiveOutputs;
            ExposureUnit = externalExposureUnit;
            var exposureSourceCollection = CalculateExposures(
                externalIndividualExposures,
                relativePotencyFactors,
                membershipProbabilities,
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

        public List<ExternalExposureBySourceRecord> summarizeExposureRecords(
            List<(ExposureSource ExposureSource, List<(SimulatedIndividual SimulatedIndividual, double Exposure)> Exposures)> exposureSourceCollection,
            double[] percentages
        ) {
            var records = new List<ExternalExposureBySourceRecord>();
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

        private List<ExternalExposureBySourcePercentileRecord> summarizeBoxPlotsRecords(
            List<(ExposureSource ExposureSource, List<(SimulatedIndividual SimulatedIndividual, double Exposure)> Exposures)> exposureSourceCollection,
            ExposureUnitTriple externalExposureUnit
        ) {
            var records = new List<ExternalExposureBySourcePercentileRecord>();

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

        private static ExternalExposureBySourceRecord getExposureSourceRecord(
            ExposureSource source,
            List<(SimulatedIndividual SimulatedIndividual, double Exposure)> exposures,
            double[] percentages
        ) {
            var weights = exposures.Where(r => r.Exposure > 0)
                .Select(c => c.SimulatedIndividual.SamplingWeight)
                .ToList();
            var weightsAll = exposures
                .Select(c => c.SimulatedIndividual.SamplingWeight)
                .ToList();
            var percentiles = exposures
                .Where(r => r.Exposure > 0)
                .Select(c => c.Exposure)
                .PercentilesWithSamplingWeights(weights, percentages);
            var percentilesAll = exposures
                .Select(c => c.Exposure)
                .PercentilesWithSamplingWeights(weightsAll, percentages);
            var total = exposures.Sum(c => c.Exposure * c.SimulatedIndividual.SamplingWeight);
            var record = new ExternalExposureBySourceRecord {
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
                NumberOfDays = weightsAll.Count
            };
            return record;
        }

        private static ExternalExposureBySourcePercentileRecord getBoxPlotRecord(
            ExposureSource source,
            List<(SimulatedIndividual SimulatedIndividual, double Exposure)> exposures,
            ExposureUnitTriple externalExposureUnit
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
            var record = new ExternalExposureBySourcePercentileRecord() {
                ExposureSource = source.GetDisplayName(),
                MinPositives = positives.Any() ? positives.Min() : 0,
                MaxPositives = positives.Any() ? positives.Max() : 0,
                Percentiles = percentiles,
                NumberOfPositives = positives.Count,
                Percentage = positives.Count * 100d / exposures.Count,
                Unit = externalExposureUnit.GetShortDisplayName(),
                Outliers = outliers,
                NumberOfOutLiers = outliers.Count,
            };
            return record;
        }
    }
}
