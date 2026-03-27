using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.Simulation.Calculators.Stratification;
using MCRA.Simulation.OutputGeneration.ActionSummaries.TargetExposures.Generic;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class ExposureByRoutePercentilesSection : InternalExposurePercentileSectionBase<RouteContributorKey, ExposureByRoutePercentileRecord> {

        public override string DescriptorKey => "Route";
        public override string DescriptorName => "route";

        public void Summarize(
            ICollection<IExternalIndividualExposure> externalIndividualExposures,
            ICollection<Compound> substances,
            IDictionary<Compound, double> rpfs,
            IDictionary<Compound, double> memberships,
            IDictionary<(ExposureRoute route, Compound substance), double> kineticConversionFactors,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound,
            List<double> percentages,
            bool isPerPerson,
            PopulationStratifier outputStratifier
        ) {
            var routes = kineticConversionFactors.Select(c => c.Key.route).Distinct().ToList();
            rpfs = substances.Count > 1 ? rpfs : substances.ToDictionary(r => r, r => 1D);
            memberships = substances.Count > 1 ? memberships : substances.ToDictionary(r => r, r => 1D);
            var exposureCollection = ExposureByRouteCalculator.CalculateExposures(
                externalIndividualExposures,
                rpfs,
                memberships,
                kineticConversionFactors,
                isPerPerson
            );
            summarize(uncertaintyLowerBound, uncertaintyUpperBound, percentages, outputStratifier, exposureCollection);
        }

        public void SummarizeUncertainty(
            ICollection<IExternalIndividualExposure> externalIndividualExposures,
            ICollection<Compound> substances,
            IDictionary<Compound, double> rpfs,
            IDictionary<Compound, double> memberships,
            IDictionary<(ExposureRoute route, Compound substance), double> kineticConversionFactors,
            List<double> percentages,
            bool isPerPerson,
            PopulationStratifier outputStratifier
        ) {
            var routes = kineticConversionFactors.Select(c => c.Key.route).Distinct().ToList();
            rpfs = rpfs ?? substances.ToDictionary(r => r, r => 1D);
            memberships = memberships ?? substances.ToDictionary(r => r, r => 1D);

            var exposureCollection = ExposureByRouteCalculator.CalculateExposures(
                externalIndividualExposures,
                rpfs,
                memberships,
                kineticConversionFactors,
                isPerPerson
            );
            summarize(percentages, outputStratifier, exposureCollection);
        }
    }
}
