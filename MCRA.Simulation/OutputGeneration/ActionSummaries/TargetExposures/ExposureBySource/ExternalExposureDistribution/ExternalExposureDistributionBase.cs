using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.Simulation.Constants;
using MCRA.Utils;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;
using MCRA.Utils.Statistics.Histograms;

namespace MCRA.Simulation.OutputGeneration {
    public class ExternalExposureDistributionBase : SummarySection, IIntakeDistributionSection {

        public override bool SaveTemporaryData => true;

        protected readonly double _upperWhisker = 95;
        protected static double[] _percentages = [5, 10, 25, 50, 75, 90, 95];

        public List<HistogramBin> IntakeDistributionBins { get; set; }
        public List<HistogramBin> IntakeDistributionBinsCoExposure { get; set; }
        public UncertainDataPointCollection<double> Percentiles { get; set; }
        public int TotalNumberOfExposures { get; set; }
        public double PercentageZeroIntake { get; set; }
        public double UncertaintyLowerLimit { get; set; }
        public double UncertaintyUpperLimit { get; set; }

        public bool ShowOutliers { get; set; }
        protected double[] Percentages { get; set; }
        public List<ExposureBySourceRecord> ExposureRecords { get; set; }
        public double? RestrictedUpperPercentile { get; set; }
        public List<ExposuresBySourcePercentileRecord> ExposureBoxPlotRecords { get; set; } = [];

        public TargetUnit TargetUnit { get; set; }

        public void Summarize(
            HashSet<int> coExposureIds,
            ICollection<IExternalIndividualDayExposure> externalIndividualDayExposures,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            double[] percentages,
            double uncertaintyLowerLimit,
            double uncertaintyUpperLimit,
            bool isPerPerson
        ) {
            UncertaintyLowerLimit = uncertaintyLowerLimit;
            UncertaintyUpperLimit = uncertaintyUpperLimit;
            Percentiles = [];
            var min = 0d;
            var max = 0d;
            var numberOfBins = 100;

            var externalExposures = externalIndividualDayExposures
                .Select(id => (
                    TotalExternalexposure: id.GetTotalExternalExposure(relativePotencyFactors, membershipProbabilities, isPerPerson),
                    SamplingWeight: id.IndividualSamplingWeight,
                    id.SimulatedIndividualDayId
                ))
                .ToList();

            var positives = externalExposures
                .Where(r => r.TotalExternalexposure > 0)
                .ToList();

            if (positives.Count != 0) {
                var logData = positives.Select(c => Math.Log10(c.TotalExternalexposure)).ToList();
                var weights = positives.Select(id => id.SamplingWeight).ToList();
                min = logData.Min();
                max = logData.Max();
                TotalNumberOfExposures = externalIndividualDayExposures.Count;
                numberOfBins = Math.Sqrt(logData.Count) < 100 ? BMath.Ceiling(Math.Sqrt(logData.Count)) : 100;
                IntakeDistributionBins = logData.MakeHistogramBins(weights, numberOfBins, min, max);
                PercentageZeroIntake = 100 - logData.Count / (double)TotalNumberOfExposures * 100;
            } else {
                IntakeDistributionBins = null;
                PercentageZeroIntake = 100D;
            }

            // Summarize the exposures based on a grid defined by the percentages array
            if (percentages != null && percentages.Length > 0) {
                Percentiles.XValues = percentages;
                var weights = externalExposures
                    .Select(c => c.SamplingWeight)
                    .ToList();
                Percentiles.ReferenceValues = externalExposures
                    .Select(i => i.TotalExternalexposure)
                    .PercentilesWithSamplingWeights(weights, percentages);
            }

            if (coExposureIds != null && coExposureIds.Count > 0) {
                var logCoExposureIntakes = externalExposures
                   .Where(i => coExposureIds.Contains(i.SimulatedIndividualDayId))
                   .Select(i => Math.Log10(i.TotalExternalexposure))
                   .ToList();

                if (logCoExposureIntakes.Count != 0) {
                    var avg = logCoExposureIntakes.Average();
                    var weights = externalExposures
                        .Where(i => coExposureIds.Contains(i.SimulatedIndividualDayId))
                        .Select(i => i.SamplingWeight)
                        .ToList();

                    IntakeDistributionBinsCoExposure = logCoExposureIntakes.MakeHistogramBins(weights, numberOfBins, min, max);
                }
            }
        }

        protected List<ExposureBySourceRecord> SummarizeExposures(
            ICollection<ExternalExposureCollection> externalExposureCollections,
            ICollection<DietaryIndividualIntake> observedIndividualMeans,
            ICollection<Compound> activeSubstances,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            double lowerPercentage,
            double upperPercentage,
            TargetUnit targetUnit,
            ExposureUnitTriple externalExposureUnit,
            bool skipPrivacySensitiveOutputs,
            bool isPerPerson
        ) {
            var cancelToken = ProgressState?.CancellationToken ?? new CancellationToken();
            var result = new List<ExposureBySourceRecord>();
            relativePotencyFactors = activeSubstances.Count > 1
                ? relativePotencyFactors : activeSubstances.ToDictionary(r => r, r => 1D);
            membershipProbabilities = activeSubstances.Count > 1
                ? membershipProbabilities : activeSubstances.ToDictionary(r => r, r => 1D);

            Percentages = [lowerPercentage, 50, upperPercentage];
            if (skipPrivacySensitiveOutputs) {
                var maxUpperPercentile = SimulationConstants.MaxUpperPercentage(externalExposureCollections.First().ExternalIndividualDayExposures.Count);
                if (_upperWhisker > maxUpperPercentile) {
                    RestrictedUpperPercentile = maxUpperPercentile;
                }
            }
            ShowOutliers = !skipPrivacySensitiveOutputs;

            foreach (var collection in externalExposureCollections) {
                var exposures = collection.ExternalIndividualDayExposures.Select(id => (
                        Exposure: id.GetTotalExternalExposure(relativePotencyFactors, membershipProbabilities, isPerPerson),
                        SamplingWeight: id.IndividualSamplingWeight
                    ))
                    .ToList();
                var record = getExposureSourceRecord(collection.ExposureSource, exposures);
                result.Add(record);
            };
            var oims = observedIndividualMeans.Select(id => (
                    Exposure: id.DietaryIntakePerMassUnit,
                    SamplingWeight: id.IndividualSamplingWeight
                )).ToList();
            result.Add(getExposureSourceRecord(ExposureSource.DietaryExposures, oims));
            result.TrimExcess();
            return result;
        }

        private ExposureBySourceRecord getExposureSourceRecord(
            ExposureSource source,
            List<(double Exposure, double SamplingWeight)> exposures
        ) {
            var weights = exposures.Where(r => r.Exposure > 0)
                .Select(c => c.SamplingWeight)
                .ToList();
            var weightsAll = exposures
                .Select(c => c.SamplingWeight)
                .ToList();
            var percentiles = exposures
                .Select(c => c.Exposure)
                .PercentilesWithSamplingWeights(weights, Percentages);
            var total = exposures.Sum(c => c.Exposure * c.SamplingWeight);
            var percentilesAll = exposures
                .Select(c => c.Exposure)
                .PercentilesWithSamplingWeights(weightsAll, Percentages);
            var record = new ExposureBySourceRecord {
                ExposureSource = source.GetShortDisplayName(),
                Percentage = weights.Count / (double)exposures.Count * 100,
                Mean = total / weightsAll.Sum(),
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
    }
}
