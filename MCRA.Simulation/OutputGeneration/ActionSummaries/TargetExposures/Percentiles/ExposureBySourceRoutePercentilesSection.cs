using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class ExposureBySourceRoutePercentilesSection : ExposureBySourceRouteSectionBase {
        public override bool SaveTemporaryData => true;
        public List<TargetExposurePercentileRecord> Records { get; set; } = [];
        public void Summarize(
            ICollection<IExternalIndividualExposure> externalIndividualExposures,
            ICollection<Compound> substances,
            IDictionary<Compound, double> rpfs,
            IDictionary<Compound, double> memberships,
            IDictionary<(ExposureRoute, Compound), double> kineticConversionFactors,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound,
            bool isPerPerson,
            List<double> percentages
        ) {
            rpfs = substances.Count > 1 ? rpfs : substances.ToDictionary(r => r, r => 1D);
            memberships = substances.Count > 1 ? memberships : substances.ToDictionary(r => r, r => 1D);

            var exposureCollection = CalculateExposures(
                externalIndividualExposures,
                rpfs,
                memberships,
                kineticConversionFactors,
                isPerPerson
            );

            foreach (var (path, exposures) in exposureCollection) {
                if (exposures.Any(c => c.Exposure > 0)) {
                    var weights = exposures
                        .Select(c => c.SimulatedIndividual.SamplingWeight)
                        .ToList();
                    var percentiles = exposures
                        .Select(c => c.Exposure)
                        .PercentilesWithSamplingWeights(weights, percentages);

                    var zip = percentages.Zip(percentiles, (x, v) => new { X = x, V = v })
                        .ToList();

                    var records = zip.Select(p => new TargetExposurePercentileRecord {
                        UncertaintyLowerLimit = uncertaintyLowerBound,
                        UncertaintyUpperLimit = uncertaintyUpperBound,
                        XValue = p.X / 100,
                        Value = p.V,
                        Values = [],
                        Source = path.Source.GetDisplayName(),
                        Route = path.Route.GetDisplayName(),
                    })
                    .ToList();
                    Records.AddRange(records);
                }
            }
        }
        public void SummarizeUncertainty(
            ICollection<IExternalIndividualExposure> externalIndividualExposures,
            ICollection<Compound> substances,
            IDictionary<Compound, double> rpfs,
            IDictionary<Compound, double> memberships,
            IDictionary<(ExposureRoute, Compound), double> kineticConversionFactors,
            List<double> percentages,
            bool isPerPerson
        ) {
            rpfs = rpfs ?? substances.ToDictionary(r => r, r => 1D);
            memberships = memberships ?? substances.ToDictionary(r => r, r => 1D);

            var exposureCollection = CalculateExposures(
                externalIndividualExposures,
                rpfs,
                memberships,
                kineticConversionFactors,
                isPerPerson
            );

            foreach (var (path, exposures) in exposureCollection) {
                if (exposures.Any(c => c.Exposure > 0)) {
                    var weights = exposures
                        .Select(c => c.SimulatedIndividual.SamplingWeight)
                        .ToList();
                    var percentiles = exposures
                        .Select(c => c.Exposure)
                        .PercentilesWithSamplingWeights(weights, percentages);
                    var records = Records
                        .Where(r => r.Source == path.Source.GetDisplayName() 
                            && r.Route == path.Route.GetDisplayName());
                    var zip = records.Zip(percentiles, (r, v) => new { Record = r, Value = v })
                        .ToList();
                    foreach (var item in zip) {
                        item.Record.Values.Add(item.Value);
                    }
                }
            }
        }
    }
}
