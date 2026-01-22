using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ExposurePercentilesByRouteSubstanceSection : SummarySection {
        public override bool SaveTemporaryData => true;
        public List<ExposurePercentileRecord> Records { get; set; }

        public void SummarizeByRouteSubstance<T>(
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
                foreach (var substance in substances) {
                    cancelToken.ThrowIfCancellationRequested();
                    var results = getRecords(
                        route,
                        externalIndividualExposures,
                        substance,
                        rpfs,
                        memberships,
                        uncertaintyLowerBound,
                        uncertaintyUpperBound,
                        percentages,
                        isPerPerson
                    );
                    records.AddRange(results);
                }
            }
            Records = records;
        }

        public void SummarizeByRouteSubstanceUncertainty<T>(
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
                foreach (var substance in substances) {
                    cancelToken.ThrowIfCancellationRequested();
                    var percentiles = getPercentiles(
                        route,
                        externalIndividualExposures,
                        substance,
                        rpfs,
                        memberships,
                        percentages,
                        isPerPerson
                    );
                    var records = Records
                        .Where(r => r.Route == route.GetDisplayName()
                            && r.SubstanceCode == substance.Code);

                    var zip = records.Zip(percentiles, (r, v) => new { Record = r, Value = v })
                        .ToList();
                    foreach (var item in zip) {
                        item.Record.Values.Add(item.Value);
                    }
                }
            }
        }

        private static List<ExposurePercentileRecord> getRecords<T>(
            ExposureRoute route,
            ICollection<T> externalIndividualExposures,
            Compound substance,
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
                substance,
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
                Route = route.GetShortDisplayName(),
                SubstanceName = substance.Name,
                SubstanceCode = substance.Code
            }).ToList();
            return records;
        }

        private static double[] getPercentiles<T>(
            ExposureRoute route,
            ICollection<T> externalIndividualExposures,
            Compound substance,
            IDictionary<Compound, double> rpfs,
            IDictionary<Compound, double> memberships,
            List<double> percentages,
            bool isPerPerson
        ) where T : IExternalIndividualExposure {
            var exposures = externalIndividualExposures
                .Select(
                    r => (
                        Exposure: r.GetExposure(route, substance, isPerPerson) * rpfs[substance] * memberships[substance],
                        r.SimulatedIndividual
                ))
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
