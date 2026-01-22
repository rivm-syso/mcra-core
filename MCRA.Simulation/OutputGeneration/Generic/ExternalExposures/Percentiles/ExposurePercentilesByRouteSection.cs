using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ExposurePercentilesByRouteSection : SummarySection {
        public override bool SaveTemporaryData => true;
        public List<ExposurePercentileRecord> Records { get; set; }

        public void SummarizeByRoute<T>(
            ICollection<T> externalIndividualExposures,
            ICollection<ExposureRoute> routes,
            ICollection<Compound> substances,
            IDictionary<Compound, double> rpfs,
            IDictionary<Compound, double> memberships,
            List<double> percentages,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound,
            bool isPerPerson,
            ExposureUnitTriple exposureUnit
        ) where T : IExternalIndividualExposure {
            rpfs = rpfs ?? substances.ToDictionary(r => r, r => 1D);
            memberships = memberships ?? substances.ToDictionary(r => r, r => 1D);
            var records = new List<ExposurePercentileRecord>();
            var cancelToken = ProgressState?.CancellationToken ?? new();
            foreach (var route in routes) {
                cancelToken.ThrowIfCancellationRequested();
                var results = getRecords(
                    route,
                    externalIndividualExposures,
                    substances,
                    rpfs,
                    memberships,
                    uncertaintyLowerBound,
                    uncertaintyUpperBound,
                    percentages,
                    isPerPerson
                );
                records.AddRange(results);
            }
            Records = records;
        }

        public void SummarizeByRouteUncertainty<T>(
            ICollection<T> externalIndividualExposures,
            ICollection<ExposureRoute> routes,
            ICollection<Compound> substances,
            IDictionary<Compound, double> rpfs,
            IDictionary<Compound, double> memberships,
            List<double> percentages,
            bool isPerPerson,
            ExposureUnitTriple exposureUnit
        ) where T : IExternalIndividualExposure {
            var cancelToken = ProgressState?.CancellationToken ?? new();
            rpfs = rpfs ?? substances.ToDictionary(r => r, r => 1D);
            memberships = memberships ?? substances.ToDictionary(r => r, r => 1D);
            foreach (var route in routes) {
                cancelToken.ThrowIfCancellationRequested();
                var percentiles = getPercentiles(
                    route,
                    externalIndividualExposures,
                    substances,
                    rpfs,
                    memberships,
                    percentages,
                    isPerPerson
                );
                var records = Records
                    .Where(r => r.Route == route.GetDisplayName());

                var zip = records.Zip(percentiles, (r, v) => new { Record = r, Value = v })
                    .ToList();
                foreach (var item in zip) {
                    item.Record.Values.Add(item.Value);
                }
            }
        }

        private static List<ExposurePercentileRecord> getRecords<T>(
            ExposureRoute route,
            ICollection<T> externalIndividualExposures,
            ICollection<Compound> substances,
            IDictionary<Compound, double> rpfs,
            IDictionary<Compound, double> memberships,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound,
            List<double> percentages,
            bool isPerPerson
       ) where T : IExternalIndividualExposure {
            var percentiles = getPercentiles(
                route,
                externalIndividualExposures,
                substances,
                rpfs,
                memberships,
                percentages,
                isPerPerson
            );

            var zip = percentages.Zip(percentiles, (x, v) => new { X = x, V = v })
                .ToList();

            var records = zip.Select(p => new ExposurePercentileRecord {
                UncertaintyLowerLimit = uncertaintyLowerBound,
                UncertaintyUpperLimit = uncertaintyUpperBound,
                XValue = p.X / 100,
                Value = p.V,
                Values = [],
                Route = route.GetDisplayName()
            }).ToList();
            return records;
        }

        private static double[] getPercentiles<T>(
            ExposureRoute route,
            ICollection<T> externalIndividualExposures,
            ICollection<Compound> substances,
            IDictionary<Compound, double> rpfs,
            IDictionary<Compound, double> memberships,
            List<double> percentages,
            bool isPerPerson
        ) where T : IExternalIndividualExposure {
            var exposures = substances
                .SelectMany(substance => {
                    return externalIndividualExposures.Select(e => new {
                        Exposure = e.GetExposure(route, substance, isPerPerson) * rpfs[substance] * memberships[substance],
                        e.SimulatedIndividual,
                    });
                })
                .GroupBy(c => c.SimulatedIndividual)
                .Select(g => new {
                    SimulatedIndividual = g.Key,
                    Exposure = g.Sum(c => c.Exposure),
                })
                .ToList();

            var samplingWeights = exposures
                .Select(c => c.SimulatedIndividual.SamplingWeight)
                .ToList();

            var percentiles = exposures
                .Select(c => c.Exposure)
                .PercentilesWithSamplingWeights(samplingWeights, percentages);
            return percentiles;
        }
    }
}
