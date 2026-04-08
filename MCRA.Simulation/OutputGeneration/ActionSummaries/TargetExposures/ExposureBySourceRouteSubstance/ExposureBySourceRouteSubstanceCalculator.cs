using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.Simulation.OutputGeneration.ActionSummaries.TargetExposures.Generic;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ExposureBySourceRouteSubstanceCalculator {
        public static string DescriptorKey => "SourceRouteSubstance";
        public static string DescriptorName => "source, route and substance";

        public static List<InternalExposuresByDescriptor<SourceRouteSubstanceContributorKey>> CalculateExposures(
            ICollection<IExternalIndividualExposure> externalIndividualExposures,
            ICollection<Compound> activeSubstances,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            IDictionary<(ExposureRoute, Compound), double> kineticConversionFactors,
            bool isPerPerson
        ) {
            relativePotencyFactors = activeSubstances.Count > 1
                ? relativePotencyFactors : activeSubstances.ToDictionary(r => r, r => 1D);
            membershipProbabilities = activeSubstances.Count > 1
                ? membershipProbabilities : activeSubstances.ToDictionary(r => r, r => 1D);
            var exposureCollection = new List<InternalExposuresByDescriptor<SourceRouteSubstanceContributorKey>>();
            var paths = externalIndividualExposures
                .SelectMany(c => c.ExposuresPerPath.Keys)
                .ToHashSet();

            foreach (var substance in activeSubstances) {
                foreach (var path in paths) {
                    var kineticConversionFactor = kineticConversionFactors[(path.Route, substance)];
                    var exposures = externalIndividualExposures
                        .Select(c => (
                            SimulatedIndividual: c.SimulatedIndividual,
                            Exposure: c.GetExposure(path, substance, isPerPerson)
                                * kineticConversionFactor * relativePotencyFactors[substance] * membershipProbabilities[substance]
                        ))
                        .ToList();
                    var internalExposures = new InternalExposuresByDescriptor<SourceRouteSubstanceContributorKey>() {
                        Descriptor = new SourceRouteSubstanceContributorKey() {
                            Route = path.Route,
                            Source = path.Source,
                            Substance = substance.Name
                        },
                        Exposures = exposures
                    };
                    exposureCollection.Add(internalExposures);
                }
            }
            return exposureCollection;
        }
    }
}
