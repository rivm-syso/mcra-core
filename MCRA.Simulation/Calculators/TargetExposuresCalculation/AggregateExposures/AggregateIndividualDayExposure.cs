using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;

namespace MCRA.Simulation.Calculators.TargetExposuresCalculation.AggregateExposures {
    public class AggregateIndividualDayExposure : AggregateIndividualExposureBase {

        public int SimulatedIndividualDayId { get; set; }

        public string Day { get; set; }

        public IExternalIndividualDayExposure ExternalIndividualDayExposure { get; set; }

        /// <summary>
        /// Gets the sum of the external exposures expressed in reference
        /// substance equivalents (i.e., corrected for relative potency).
        /// </summary>
        public override double GetTotalExternalExposure(
            IDictionary<Compound, double> rpfs,
            IDictionary<Compound, double> memberships,
            bool isPerPerson
        ) {
            return ExternalIndividualDayExposure.GetExposure(rpfs, memberships, isPerPerson);
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
            return ExternalIndividualDayExposure.GetExposure(rpfs, memberships, kineticConversionFactors, isPerPerson);
        }

        /// <summary>
        /// Gets the sum of the external exposures expressed in reference
        /// substance equivalents (i.e., corrected for relative potency).
        /// </summary>
        public override double GetTotalExternalExposureForSubstance(
            Compound substance,
            bool isPerPerson
        ) {
            return ExternalIndividualDayExposure.GetExposure(substance, isPerPerson);
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
            return ExternalIndividualDayExposure.GetExposure(substance, kineticConversionFactors, isPerPerson);
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
            return ExternalIndividualDayExposure.GetExposure(route, rpfs, memberships, kineticConversionFactors, isPerPerson);
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
            return ExternalIndividualDayExposure.GetExposure(route, substance, isPerPerson);
        }

        /// <summary>
        /// Returns whether there is (external) exposure of multiple substances.
        /// </summary>
        public override bool IsCoExposure() {
            var result = ExternalIndividualDayExposure.ExposuresPerPath
                .SelectMany(r => r.Value.Where(e => e.Amount > 0))
                .Select(r => r.Compound)
                .Distinct();
            return result.Count() > 1;
        }
    }
}
