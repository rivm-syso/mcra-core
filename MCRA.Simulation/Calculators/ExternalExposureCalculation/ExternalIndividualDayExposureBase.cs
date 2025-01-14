using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;
using MCRA.General;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;

namespace MCRA.Simulation.Calculators.ExternalExposureCalculation {
    public abstract class ExternalIndividualDayExposureBase : IExternalIndividualDayExposure {

        public SimulatedIndividual SimulatedIndividual { get; set; }
        public string Day { get; set; }
        public int SimulatedIndividualDayId { get; set; }

        public abstract Dictionary<ExposureRoute, ICollection<IIntakePerCompound>> ExposuresPerRouteSubstance { get; }

        /// <summary>
        /// Gets the total (cumulative) external exposure expressed
        /// in reference substance equivalents by weighting by relative
        /// potency.
        /// </summary>
        public double GetTotalExternalExposure(
            IDictionary<Compound, double> rpfs,
            IDictionary<Compound, double> memberships,
            bool isPerPerson
        ) {
            var result = ExposuresPerRouteSubstance.Values
                .Sum(r => r.Sum(ipc => ipc.EquivalentSubstanceAmount(rpfs[ipc.Compound], memberships[ipc.Compound])));
            return isPerPerson
                ? result
                : result / SimulatedIndividual.BodyWeight;
        }

        /// <summary>
        /// Gets the total (cumulative) external exposure expressed
        /// in reference substance equivalents by weighting by relative
        /// potency.
        /// </summary>
        public double GetTotalExternalExposure(
            IDictionary<Compound, double> rpfs,
            IDictionary<Compound, double> memberships,
            IDictionary<(ExposureRoute, Compound), double> kineticConversionFactors,
            bool isPerPerson
        ) {
            var result = ExposuresPerRouteSubstance
                .Sum(r => r.Value
                    .Sum(ipc => kineticConversionFactors[(r.Key, ipc.Compound)]
                        * ipc.EquivalentSubstanceAmount(rpfs[ipc.Compound], memberships[ipc.Compound])
                ));
            return isPerPerson
                ? result
                : result / SimulatedIndividual.BodyWeight;
        }

        /// <summary>
        /// Gets the total external exposure of the substance summed over the different routes.
        /// </summary>
        public double GetTotalExternalExposureForSubstance(
            Compound substance,
            bool isPerPerson
        ) {
            var result = ExposuresPerRouteSubstance
                .Sum(r => r.Value
                    .Where(r => r.Compound == substance)
                    .Sum(ipc => ipc.Amount)
                );
            return isPerPerson
                ? result
                : result / SimulatedIndividual.BodyWeight;
        }

        /// <summary>
        /// Gets the total external exposure for the specified substance multiplied by the kinetic conversion factors.
        /// </summary>
        public double GetTotalExternalExposureForSubstance(
            Compound substance,
            IDictionary<(ExposureRoute, Compound), double> kineticConversionFactors,
            bool isPerPerson
        ) {
            var result = ExposuresPerRouteSubstance
                .Sum(r => r.Value
                    .Where(r => r.Compound == substance)
                    .Sum(ipc => ipc.Amount > 0
                        ? ipc.Amount * kineticConversionFactors[(r.Key, substance)]
                        : 0
                    )
                );
            return isPerPerson
                ? result
                : result / SimulatedIndividual.BodyWeight;
        }

        /// <summary>
        /// Gets the total substance exposure for the specified route.
        /// </summary>
        public double GetSubstanceExposureForRoute(
            ExposureRoute route,
            Compound substance,
            bool isPerPerson
        ) {
            if (ExposuresPerRouteSubstance.TryGetValue(route, out var exposures)) {
                var totalIntake = exposures
                    .Where(r => r.Compound == substance)
                    .Sum(r => r.Amount);
                var result = isPerPerson
                    ? totalIntake
                    : totalIntake / SimulatedIndividual.BodyWeight;
                return result;
            }
            return 0;
        }

        /// <summary>
        /// Gets the total (cumulative) exposure for the specified route and use kinetic absorption factors.
        /// </summary>
        public double GetTotalRouteExposure(
            ExposureRoute route,
            IDictionary<Compound, double> rpfs,
            IDictionary<Compound, double> memberships,
            IDictionary<(ExposureRoute, Compound), double> kineticConversionFactors,
            bool isPerPerson
        ) {
            if (ExposuresPerRouteSubstance.TryGetValue(route, out var exposures)) {
                var totalIntake = exposures
                    .Sum(r => {
                        var exposure = r.EquivalentSubstanceAmount(rpfs[r.Compound], memberships[r.Compound]);
                        return exposure > 0
                            ? exposure * kineticConversionFactors[(route, r.Compound)]
                            : 0;
                    });
                var exposure = isPerPerson
                    ? totalIntake
                    : totalIntake / SimulatedIndividual.BodyWeight;
                return exposure;
            }
            return 0;
        }

        /// <summary>
        /// Gets the total (cumulative) exposure for the specified route.
        /// </summary>
        /// <returns></returns>
        public double GetTotalRouteExposure(
            ExposureRoute route,
            IDictionary<Compound, double> rpfs,
            IDictionary<Compound, double> memberships,
            bool isPerPerson
        ) {
            if (ExposuresPerRouteSubstance.TryGetValue(route, out var exposures)) {
                var totalIntake = exposures
                    .Sum(r => r.EquivalentSubstanceAmount(rpfs[r.Compound], memberships[r.Compound]));
                var exposure = isPerPerson ? totalIntake : totalIntake / SimulatedIndividual.BodyWeight;
                return exposure;
            }
            return 0;
        }

        public ICollection<IIntakePerCompound> GetTotalExposurePerCompound() {
            var exposurePerSubstance = ExposuresPerRouteSubstance
                .SelectMany(r => r.Value)
                .GroupBy(dipc => dipc.Compound)
                .Select(g => new AggregateIntakePerCompound() {
                    Amount = g.Sum(dipc => dipc.Amount),
                    Compound = g.Key,
                })
                .Cast<IIntakePerCompound>()
                .ToList();
            return exposurePerSubstance;
        }

        public ICollection<IIntakePerCompound> GetTotalExposurePerRouteSubstance(
            ExposureRoute exposureRoute
        ) {
            var exposuresPerSubstance = ExposuresPerRouteSubstance
                .Where(r => r.Key == exposureRoute)
                .SelectMany(r => r.Value)
                .GroupBy(ipc => ipc.Compound)
                .Select(g => new AggregateIntakePerCompound {
                    Compound = g.Key,
                    Amount = g.Sum(c => c.Amount),
                })
                .Cast<IIntakePerCompound>()
                .ToList();
            return exposuresPerSubstance;
        }
    }
}
