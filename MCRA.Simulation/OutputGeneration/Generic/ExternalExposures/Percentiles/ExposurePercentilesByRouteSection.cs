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
            IDictionary<Compound, double> rpf,
            IDictionary<Compound, double> membership,
            List<double> percentages,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound,
            bool isPerPerson,
            ExposureUnitTriple exposureUnit
        ) where T : IExternalIndividualExposure {
            var records = new List<ExposurePercentileRecord>();
            var cancelToken = ProgressState?.CancellationToken ?? new();
            foreach (var route in routes) {
                cancelToken.ThrowIfCancellationRequested();
                var results = getRecords(
                    route,
                    externalIndividualExposures,
                    substances,
                    rpf,
                    membership,
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
            IDictionary<Compound, double> rpf,
            IDictionary<Compound, double> membership,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound,
            List<double> percentages,
            bool isPerPerson,
            ExposureUnitTriple exposureUnit
        ) where T : IExternalIndividualExposure {
            var cancelToken = ProgressState?.CancellationToken ?? new();
            rpf = rpf ?? substances.ToDictionary(r => r, r => 1D);
            membership = membership ?? substances.ToDictionary(r => r, r => 1D);
            foreach (var route in routes) {
                cancelToken.ThrowIfCancellationRequested();
                var percentiles = getPercentiles(
                    route,
                    externalIndividualExposures,
                    substances,
                    rpf,
                    membership,
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
            IDictionary<Compound, double> rpf,
            IDictionary<Compound, double> membership,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound,
            List<double> percentages,
            bool isPerPerson
       ) where T : IExternalIndividualExposure {
            var percentiles = getPercentiles(
                route,
                externalIndividualExposures,
                substances,
                rpf,
                membership,
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
            IDictionary<Compound, double> rpf,
            IDictionary<Compound, double> membership,
            List<double> percentages,
            bool isPerPerson
        ) where T : IExternalIndividualExposure {
            var exposures = substances
                .SelectMany(substance => {
                    return externalIndividualExposures.Select(e => new {
                        Exposure = e.GetExposure(route, substance, isPerPerson) * rpf[substance] * membership[substance],
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
