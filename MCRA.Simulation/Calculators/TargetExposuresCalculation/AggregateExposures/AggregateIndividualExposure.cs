using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;

namespace MCRA.Simulation.Calculators.TargetExposuresCalculation.AggregateExposures {
    public class AggregateIndividualExposure {

        public SimulatedIndividual SimulatedIndividual { get; set; }

        public double IndividualSamplingWeight => SimulatedIndividual.SamplingWeight;

        public List<IExternalIndividualDayExposure> ExternalIndividualDayExposures { get; set; } = [];

        public Dictionary<ExposureTarget, Dictionary<Compound, ISubstanceTargetExposure>> InternalTargetExposures { get; set; } = [];

        /// <summary>
        /// Returns the substance exposure of the specified target or zero
        /// if no exposures for the target/substance combination were found.
        /// </summary>
        public double GetSubstanceExposure(
            ExposureTarget target,
            Compound substance
        ) {
            if (InternalTargetExposures.TryGetValue(target, out var targetExposures)) {
                if (targetExposures.TryGetValue(substance, out var exposure)) {
                    return exposure.Exposure;
                }
                throw new Exception($"No exposures found for substance [{substance.Code}] on target [{target.Code}].");
            }
            throw new Exception($"No exposures found for target [{target.Code}].");
        }

        /// <summary>
        /// Gets the substance target exposure for the specified target, corrected
        /// for relative potency and membership.
        /// </summary>
        public double GetSubstanceExposure(
            ExposureTarget target,
            Compound substance,
            double rpf,
            double membership
        ) {
            if (InternalTargetExposures.TryGetValue(target, out var targetSubstanceExposures)) {
                if (targetSubstanceExposures.TryGetValue(substance, out var substanceTargetExposure)) {
                    return rpf * membership * substanceTargetExposure.Exposure;
                }
                throw new Exception($"No exposures found for substance [{substance.Code}] on target [{target.Code}].");
            }
            throw new Exception($"No exposures found for target [{target.Code}].");
        }

        /// <summary>
        /// Gets the substance target exposure for the specified target.
        /// </summary>
        public ISubstanceTargetExposure GetSubstanceTargetExposure(
            ExposureTarget target,
            Compound substance
        ) {
            if (InternalTargetExposures.TryGetValue(target, out var targetSubstanceExposures)) {
                if (targetSubstanceExposures.TryGetValue(substance, out var substanceTargetExposure)) {
                    return substanceTargetExposure;
                }
                throw new Exception($"No exposures found for substance [{substance.Code}] on target [{target.Code}].");
            }
            throw new Exception($"No exposures found for target [{target.Code}].");
        }

        /// <summary>
        /// Cumulative exposure at target.
        /// </summary>
        public double GetTotalExposureAtTarget(
            ExposureTarget target,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities
        ) {
            if (InternalTargetExposures.TryGetValue(target, out var targetExposures)) {
                var result = targetExposures.Values
                    .Sum(ipc => ipc.EquivalentSubstanceExposure(
                        relativePotencyFactors[ipc.Substance],
                        membershipProbabilities[ipc.Substance]
                    )
                );
                return result;
            }
            throw new Exception($"No exposures found for target [{target.Code}].");
        }

        /// <summary>
        /// Gets the sum of the external exposures expressed in reference
        /// substance equivalents (i.e., corrected for relative potency).
        /// </summary>
        /// <param name="rpfs"></param>
        /// <param name="memberships"></param>
        /// <param name="isPerPerson"></param>
        /// <returns></returns>
        public double GetTotalExternalExposure(
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
        /// <param name="rpfs"></param>
        /// <param name="memberships"></param>
        /// <param name="kineticConversionFactors"></param>
        /// <param name="isPerPerson"></param>
        /// <returns></returns>
        public double GetTotalExternalExposure(
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
        public double GetTotalExternalExposureForSubstance(
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
        public double GetTotalExternalExposureForSubstance(
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
        public double GetTotalRouteExposure(
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
        public double GetTotalRouteExposureForSubstance(
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
        /// Get contributions of external routes to internal exposures. Note that for
        /// this calculation, internal exposures are derived using kinetic conversion factors.
        /// </summary>
        /// <param name="routes"></param>
        /// <param name="relativePotencyFactors"></param>
        /// <param name="membershipProbabilities"></param>
        /// <param name="kineticConversionFactors"></param>
        /// <param name="externalExposureUnit"></param>
        /// <returns></returns>
        public List<double> GetExternalRouteExposureContributions(
            ICollection<ExposureRoute> routes,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            IDictionary<(ExposureRoute, Compound), double> kineticConversionFactors,
            ExposureUnitTriple externalExposureUnit,
            TargetUnit targetUnit
        ) {
            var result = new List<double>();
            // Calculate internal exposure
            var internalExposure = GetTotalExternalExposure(
                relativePotencyFactors,
                membershipProbabilities,
                kineticConversionFactors,
                externalExposureUnit.IsPerUnit()
            );
            // Calculate route exposure and use kinetic conversion factors
            foreach (var route in routes) {
                var routeExposure = GetTotalRouteExposure(
                    route,
                    relativePotencyFactors,
                    membershipProbabilities,
                    kineticConversionFactors,
                    externalExposureUnit.IsPerUnit()
                );
                var contribution = routeExposure / internalExposure;
                result.Add(contribution);
            }
            return result;
        }

        /// <summary>
        /// Returns whether there is (external) exposure of multiple substances.
        /// </summary>
        /// <returns></returns>
        public bool IsCoExposure() {
            var result = ExternalIndividualDayExposures
                .SelectMany(r => r.ExposuresPerPath)
                .SelectMany(r => r.Value.Where(e => e.Amount > 0))
                .Select(r => r.Compound)
                .Distinct();
            return result.Count() > 1;
        }

        /// <summary>
        /// Returns whether there is (external) exposure of multiple substances.
        /// </summary>
        /// <returns></returns>
        public bool IsPositiveTargetExposure(ExposureTarget target) {
            if (InternalTargetExposures.TryGetValue(target, out var targetExposures)) {
                return targetExposures.Values.Any(r => r.Exposure > 0);
            }
            return false;
        }
    }
}
