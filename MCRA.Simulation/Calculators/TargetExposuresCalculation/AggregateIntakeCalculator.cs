using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;
using MCRA.Simulation.Calculators.NonDietaryIntakeCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation;

namespace MCRA.Simulation.Calculators.KineticModelCalculation {
    public class AggregateIntakeCalculator {

        public AggregateIntakeCalculator() {
        }

        /// <summary>
        /// Merges the dietary and non-dietary individual day exposure collections into a collection of
        /// aggregate individual day exposures.
        /// </summary>
        /// <param name="dietaryIndividualDayIntakes"></param>
        /// <param name="nonDietaryIndividualDayIntakes"></param>
        /// <param name="nonDietaryExposureRoutes"></param>
        /// <returns></returns>
        public static List<AggregateIndividualDayExposure> CreateAggregateIndividualDayExposures(
            ICollection<DietaryIndividualDayIntake> dietaryIndividualDayIntakes,
            ICollection<NonDietaryIndividualDayIntake> nonDietaryIndividualDayIntakes,
            ICollection<ExposurePathType> nonDietaryExposureRoutes
        ) {
            var nonDietaryIndividualIntakeDictionary = nonDietaryIndividualDayIntakes?.ToDictionary(item => item.SimulatedIndividualDayId);
            var result = dietaryIndividualDayIntakes
                .AsParallel()
                .Select(c => {
                    var exposuresPerRouteSubstance = CollectIndividualDayExposurePerRouteSubstance(c, nonDietaryIndividualIntakeDictionary?[c.SimulatedIndividualDayId], nonDietaryExposureRoutes);
                    return new AggregateIndividualDayExposure() {
                        Individual = c.Individual,
                        SimulatedIndividualDayId = c.SimulatedIndividualDayId,
                        SimulatedIndividualId = c.SimulatedIndividualId,
                        IndividualSamplingWeight = c.IndividualSamplingWeight,
                        DietaryIndividualDayIntake = c,
                        NonDietaryIndividualDayIntake = nonDietaryIndividualIntakeDictionary?[c.SimulatedIndividualDayId],
                        ExposuresPerRouteSubstance = exposuresPerRouteSubstance,
                        TargetExposuresBySubstance = new Dictionary<Compound, ISubstanceTargetExposure>(),
                        RelativeCompartmentWeight = 1D,
                    };
                })
                .OrderBy(r => r.SimulatedIndividualDayId)
                .ToList();
            return result;
        }

        /// <summary>
        /// Groups the aggregate individual day exposures by simulated individual to obtain
        /// a collection of aggregate individual exposures.
        /// </summary>
        /// <param name="aggregateIndividualDayExposures"></param>
        /// <returns></returns>
        public static List<AggregateIndividualExposure> CreateAggregateIndividualExposures(
            List<AggregateIndividualDayExposure> aggregateIndividualDayExposures
        ) {
            // Combine dietary and non-dietary individual exposures
            var aggregateIndividualExposures = aggregateIndividualDayExposures
               .GroupBy(c => c.SimulatedIndividualId)
               .Select(c => {
                   var exposuresPerRouteSubstance = CollectIndividualExposurePerRouteSubstance(c.ToList());
                   return new AggregateIndividualExposure() {
                       Individual = c.First().Individual,
                       SimulatedIndividualId = c.Key,
                       IndividualSamplingWeight = c.First().IndividualSamplingWeight,
                       TargetExposuresBySubstance = new Dictionary<Compound, ISubstanceTargetExposure>(),
                       ExposuresPerRouteSubstance = exposuresPerRouteSubstance,
                       ExternalIndividualDayExposures = c.Cast<IExternalIndividualDayExposure>().ToList(),
                       RelativeCompartmentWeight = 1D,
                   };
               })
               .OrderBy(r => r.SimulatedIndividualId)
               .ToList();
            return aggregateIndividualExposures;
        }

        /// <summary>
        /// Individual aggregate (model assisted).
        /// Note that a correction is needed for compartment weight.
        /// </summary>
        /// <param name="aggregateIndividualDayExposures"></param>
        /// <param name="dietaryModelAssistedIntakes"></param>
        /// <param name="reference"></param>
        /// <param name="isPerPerson"></param>
        /// <returns></returns>
        public List<AggregateIndividualExposure> CalculateAggregateIndividualUsualIntakes(
            ICollection<AggregateIndividualDayExposure> aggregateIndividualDayExposures,
            ICollection<DietaryIndividualIntake> dietaryModelAssistedIntakes,
            Compound reference,
            bool isPerPerson
        ) {
            var dietaryIndividualIntakesLookup = dietaryModelAssistedIntakes
                .ToDictionary(r => r.SimulatedIndividualId);
            return aggregateIndividualDayExposures
                .GroupBy(r => r.SimulatedIndividualId)
                .Select(g => {
                    var targetExposurePerSubstance = new Dictionary<Compound, ISubstanceTargetExposure>();
                    var dietaryIntake = dietaryIndividualIntakesLookup[g.Key].DietaryIntakePerMassUnit * (isPerPerson ? 1 : g.First().CompartmentWeight);
                    targetExposurePerSubstance.Add(reference, new SubstanceTargetExposure() { Substance = reference, SubstanceAmount = dietaryIntake });
                    return new AggregateIndividualExposure() {
                        SimulatedIndividualId = g.Key,
                        Individual = g.First().Individual,
                        IndividualSamplingWeight = g.First().IndividualSamplingWeight,
                        TargetExposuresBySubstance = targetExposurePerSubstance,
                        RelativeCompartmentWeight = g.First().RelativeCompartmentWeight
                    };
                })
                .ToList();
        }

        /// <summary>
        /// Individual aggregate (ISUF). Note that a correction is needed for compartment weight
        /// </summary>
        /// <param name="aggregateIndividualDayExposures"></param>
        /// <returns></returns>
        public List<AggregateIndividualExposure> CalculateAggregateIndividualUsualIntakes(ICollection<AggregateIndividualDayExposure> aggregateIndividualDayExposures) {
            return aggregateIndividualDayExposures
                .GroupBy(r => r.SimulatedIndividualId)
                .Select(g => {
                    return new AggregateIndividualExposure() {
                        SimulatedIndividualId = g.Key,
                        Individual = g.First().Individual,
                        IndividualSamplingWeight = g.First().IndividualSamplingWeight,
                        RelativeCompartmentWeight = g.First().RelativeCompartmentWeight
                    };
                })
                .ToList();
        }
        /// <summary>
        /// Computes the individual day exposures per route compound.
        /// </summary>
        /// <param name="dietaryIndividualDayIntake"></param>
        /// <param name="nonDietaryIndividualDayIntake"></param>
        /// <param name="nonDietaryExposureRoutes"></param>
        /// <returns></returns>
        public static IDictionary<ExposurePathType, ICollection<IIntakePerCompound>> CollectIndividualDayExposurePerRouteSubstance(
            DietaryIndividualDayIntake dietaryIndividualDayIntake,
            NonDietaryIndividualDayIntake nonDietaryIndividualDayIntake,
            ICollection<ExposurePathType> nonDietaryExposureRoutes
        ) {
            var intakesPerRoute = new Dictionary<ExposurePathType, ICollection<IIntakePerCompound>>();
            var nonDietaryIntakesPerRouteSubstance = nonDietaryIndividualDayIntake?.GetTotalIntakesPerRouteSubstance();
            foreach (var route in nonDietaryExposureRoutes) {
                if (route == ExposurePathType.Dietary) {
                    intakesPerRoute[ExposurePathType.Dietary] = dietaryIndividualDayIntake.GetTotalIntakesPerSubstance();
                } else if (nonDietaryIntakesPerRouteSubstance != null) {
                    var intakesPerCompound = nonDietaryIntakesPerRouteSubstance?
                        .Where(c => c.Route == route)
                        .Select(g => new AggregateIntakePerCompound() {
                            Compound = g.Compound,
                            Exposure = g.Exposure,
                        })
                        .Cast<IIntakePerCompound>()
                        .ToList();
                    intakesPerRoute[route] = intakesPerCompound;
                }
            }
            return intakesPerRoute;
        }

        /// <summary>
        /// Computes the individual day exposures per route compound.
        /// </summary>
        /// <param name="externalIndividualDayExposures"></param>
        /// <returns></returns>
        public static IDictionary<ExposurePathType, ICollection<IIntakePerCompound>> CollectIndividualExposurePerRouteSubstance(
            IEnumerable<IExternalIndividualDayExposure> externalIndividualDayExposures
        ) {
            var intakesPerRoute = new Dictionary<ExposurePathType, ICollection<IIntakePerCompound>>();
            var routes = externalIndividualDayExposures.First().ExposuresPerRouteSubstance.Keys;
            foreach (var route in routes) {
                var routeExposures = externalIndividualDayExposures
                    .SelectMany(r => r.ExposuresPerRouteSubstance[route], (r, i) => (r.Individual, i.Compound, i.Exposure))
                    .GroupBy(c => c.Compound)
                    .Select(c => new AggregateIntakePerCompound() {
                        Compound = c.Key,
                        Exposure = c.Sum(i => i.Exposure) / externalIndividualDayExposures.Count()
                    })
                    .Cast<IIntakePerCompound>()
                    .ToList();
                intakesPerRoute[route] = routeExposures;
            }
            return intakesPerRoute;
        }
    }
}
