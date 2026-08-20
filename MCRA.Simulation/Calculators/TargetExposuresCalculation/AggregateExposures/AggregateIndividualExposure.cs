using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;

namespace MCRA.Simulation.Calculators.TargetExposuresCalculation.AggregateExposures {
    public class AggregateIndividualExposure : AggregateIndividualExposureBase {

        public IExternalIndividualExposure ExternalIndividualExposure { get; set; }

        public List<IExternalIndividualDayExposure> ExternalIndividualDayExposures => ExternalIndividualExposure.ExternalIndividualDayExposures;

        /// <summary>
        /// Gets the sum of the external exposures expressed in reference
        /// substance equivalents (i.e., corrected for relative potency).
        /// </summary>
        public override double GetTotalExternalExposure(
            IDictionary<Compound, double> rpfs,
            IDictionary<Compound, double> memberships,
            bool isPerPerson
        ) {
            var result = ExternalIndividualDayExposures
                .Sum(r => r.GetExposure(rpfs, memberships, isPerPerson))
                    / ExternalIndividualDayExposures.Count;
            return result;
        }

        /// <summary>
        /// Gets the total cumulative exposure, with substance exposures scaled for relative
        /// potency, membership, and kinetic conversion factors.
        /// </summary>
        public override double GetTotalExternalExposure(
            IDictionary<Compound, double> rpfs,
            IDictionary<Compound, double> memberships,
            IDictionary<(ExposureRoute, Compound), double> kineticConversionFactors,
            bool isPerPerson
        ) {
            var exposure = ExternalIndividualDayExposures
                .Sum(r => r.GetExposure(
                    rpfs,
                    memberships,
                    kineticConversionFactors,
                    isPerPerson
                )) / ExternalIndividualDayExposures.Count;

            return exposure;
        }

        /// <summary>
        /// Gets the sum of the external exposures expressed in reference
        /// substance equivalents (i.e., corrected for relative potency).
        /// </summary>
        public override double GetTotalExternalExposureForSubstance(
            Compound substance,
            bool isPerPerson
        ) {
            var result = ExternalIndividualDayExposures
                .Sum(r => r.GetExposure(substance, isPerPerson))
                    / ExternalIndividualDayExposures.Count;
            return result;
        }

        /// <summary>
        /// Gets the sum of the external exposures expressed in reference
        /// substance equivalents (i.e., corrected for relative potency).
        /// </summary>
        public override double GetTotalExternalExposureForSubstance(
            Compound substance,
            IDictionary<(ExposureRoute, Compound), double> kineticConversionFactors,
            bool isPerPerson
        ) {
            var result = ExternalIndividualDayExposures
                .Sum(r => r.GetExposure(substance, kineticConversionFactors, isPerPerson))
                    / ExternalIndividualDayExposures.Count;
            return result;
        }

        /// <summary>
        /// Gets the total (cumulative) exposure for the specified route
        /// expressed in reference substance equivalents (i.e., corrected
        /// for relative potency) and absorptionfactors
        /// </summary>
        public override double GetTotalRouteExposure(
            ExposureRoute route,
            IDictionary<Compound, double> rpfs,
            IDictionary<Compound, double> memberships,
            IDictionary<(ExposureRoute, Compound), double> kineticConversionFactors,
            bool isPerPerson
        ) {
            var result = ExternalIndividualDayExposures
                .Sum(r => r.GetExposure(route, rpfs, memberships, kineticConversionFactors, isPerPerson))
                    / ExternalIndividualDayExposures.Count;
            return result;
        }

        /// <summary>
        /// Gets the total (cumulative) exposure for the specified route
        /// expressed in reference substance equivalents (i.e., corrected
        /// for relative potency).
        /// </summary>
        public override double GetTotalRouteExposureForSubstance(
            ExposureRoute route,
            Compound substance,
            bool isPerPerson
        ) {
            var result = ExternalIndividualDayExposures
                .Sum(r => r.GetExposure(route, substance, isPerPerson))
                    / ExternalIndividualDayExposures.Count;
            return result;
        }

        /// <summary>
        /// Returns whether there is (external) exposure of multiple substances.
        /// </summary>
        public override bool IsCoExposure() {
            var result = ExternalIndividualDayExposures
                .SelectMany(r => r.ExposuresPerPath)
                .SelectMany(r => r.Value.Where(e => e.Amount > 0))
                .Select(r => r.Compound)
                .Distinct();
            return result.Count() > 1;
        }
    }
}
