using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration {

    public abstract class ExternalExposureBySourceRouteSectionBase : SummarySection {

        public override bool SaveTemporaryData => true;

        protected readonly double _upperWhisker = 95;

        protected static double[] _percentages = [5, 10, 25, 50, 75, 90, 95];

        public List<ExternalExposuresBySourceRouteRecord> ExposureRecords { get; set; }
        public List<ExternalExposuresBySourceRoutePercentileRecord> ExposureBoxPlotRecords { get; set; } = [];
        public ExposureUnitTriple ExposureUnit { get; set; }
        protected double[] Percentages;
        public bool ShowOutliers { get; set; }
        public double? RestrictedUpperPercentile { get; set; }

        public List<ExternalExposuresBySourceRouteRecord> summarizeExposureRecords(
            ICollection<ExternalExposureCollection> externalExposureCollections,
            ICollection<DietaryIndividualIntake> observedIndividualMeans,
            ICollection<Compound> activeSubstances,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            ICollection<ExposureRoute> routes,
            bool isPerPerson
        ) {
            var results = new List<ExternalExposuresBySourceRouteRecord>();
            foreach (var collection in externalExposureCollections) {
                foreach (var route in routes) {
                    var exposures = collection.ExternalIndividualDayExposures
                        .Select(id => (
                            Exposure: id.GetTotalRouteExposure(route, relativePotencyFactors, membershipProbabilities, isPerPerson),
                            SamplingWeight: id.IndividualSamplingWeight
                        ))
                        .ToList();
                    if (exposures.Any(c => c.Exposure > 0)) {
                        var record = getExposureSourceRouteRecord(
                            route,
                            collection.ExposureSource,
                            exposures
                        );
                        results.Add(record);
                    }
                }
            }

            if (observedIndividualMeans != null) {
                var oims = observedIndividualMeans
                    .Select(id => (
                        Exposure: id.DietaryIntakePerMassUnit,
                        SamplingWeight: id.IndividualSamplingWeight
                    ))
                    .ToList();
                results.Add(getExposureSourceRouteRecord(ExposureRoute.Oral, ExposureSource.DietaryExposures, oims));
            }
            return results;
        }

        private ExternalExposuresBySourceRouteRecord getExposureSourceRouteRecord(
            ExposureRoute route,
            ExposureSource source,
            List<(double Exposure, double SamplingWeight)> exposures
        ) {
            var weights = exposures.Where(c => c.Exposure > 0)
                .Select(idi => idi.SamplingWeight)
                .ToList();
            var percentiles = exposures.Where(c => c.Exposure > 0)
                .Select(c => c.Exposure)
                .PercentilesWithSamplingWeights(weights, Percentages);

            var weightsAll = exposures.Select(idi => idi.SamplingWeight).ToList();
            var percentilesAll = exposures
                .Select(c => c.Exposure)
                .PercentilesWithSamplingWeights(weightsAll, Percentages);

            var record = new ExternalExposuresBySourceRouteRecord {
                ExposureSource = source.GetShortDisplayName(),
                ExposureRoute = route.GetShortDisplayName(),
                Percentage = weights.Count / (double)exposures.Count * 100,
                Mean = exposures.Sum(c => c.Exposure * c.SamplingWeight) / weights.Sum(),
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
    }
}
