using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.Simulation.Constants;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration {
    public abstract class ExternalExposureBySourceSectionBase : SummarySection {
        public override bool SaveTemporaryData => true;

        protected readonly double _upperWhisker = 95;

        protected static double[] _percentages = [5, 10, 25, 50, 75, 90, 95];
        public bool ShowOutliers { get; set; }
        protected double[] Percentages { get; set; }
        public List<ExternalExposureBySourceRecord> ExposureRecords { get; set; }
        public double? RestrictedUpperPercentile { get; set; }
        public List<ExternalExposureBySourcePercentileRecord> ExposureBoxPlotRecords { get; set; } = [];
        public TargetUnit TargetUnit { get; set; }

        protected List<ExternalExposureBySourceRecord> SummarizeExposures(
            ICollection<ExternalExposureCollection> externalExposureCollections,
            ICollection<DietaryIndividualIntake> observedIndividualMeans,
            ICollection<Compound> activeSubstances,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            TargetUnit targetUnit,
            ExposureUnitTriple externalExposureUnit,
            bool isPerPerson
        ) {
            var cancelToken = ProgressState?.CancellationToken ?? new CancellationToken();
            var result = new List<ExternalExposureBySourceRecord>();
            relativePotencyFactors = activeSubstances.Count > 1
                ? relativePotencyFactors : activeSubstances.ToDictionary(r => r, r => 1D);
            membershipProbabilities = activeSubstances.Count > 1
                ? membershipProbabilities : activeSubstances.ToDictionary(r => r, r => 1D);

            foreach (var collection in externalExposureCollections) {
                var exposures = collection.ExternalIndividualDayExposures.Select(id => (
                        Exposure: id.GetTotalExternalExposure(relativePotencyFactors, membershipProbabilities, isPerPerson),
                        SamplingWeight: id.IndividualSamplingWeight
                    ))
                    .ToList();
                var record = getExposureSourceRecord(collection.ExposureSource, exposures);
                result.Add(record);
            };
            if (observedIndividualMeans != null) {
                var oims = observedIndividualMeans.Select(id => (
                        Exposure: id.DietaryIntakePerMassUnit,
                        SamplingWeight: id.IndividualSamplingWeight
                    )).ToList();
                result.Add(getExposureSourceRecord(ExposureSource.DietaryExposures, oims));
            }
            result.TrimExcess();
            return result;
        }
        private ExternalExposureBySourceRecord getExposureSourceRecord(
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
            var record = new ExternalExposureBySourceRecord {
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
