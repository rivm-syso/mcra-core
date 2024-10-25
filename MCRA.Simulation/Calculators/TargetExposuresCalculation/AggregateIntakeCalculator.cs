using MCRA.General;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;
using MCRA.Simulation.Calculators.DustExposureCalculation;
using MCRA.Simulation.Calculators.NonDietaryIntakeCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation;

namespace MCRA.Simulation.Calculators.KineticModelCalculation {
    public class AggregateIntakeCalculator {

        /// <summary>
        /// Merges the dietary and non-dietary individual day exposure collections into a 
        /// collection of aggregate individual day exposures.
        /// Change: aggregate the Oral dietary route with the Oral nondietary route.
        /// </summary>
        public static List<IExternalIndividualDayExposure> CreateCombinedIndividualDayExposures(
            ICollection<DietaryIndividualDayIntake> dietaryIndividualDayIntakes,
            ICollection<NonDietaryIndividualDayIntake> nonDietaryIndividualDayIntakes,
            ICollection<DustIndividualDayExposure> dustIndividualDayExposures,
            ICollection<ExposurePathType> exposureRoutes
        ) {
            var nonDietaryIntakeLookup = nonDietaryIndividualDayIntakes?
                .ToDictionary(item => item.SimulatedIndividualDayId);
            var dustIntakeLookup = dustIndividualDayExposures?
                .ToDictionary(item => item.SimulatedIndividualDayId);
            var result = dietaryIndividualDayIntakes
                .AsParallel()
                .Select(dietaryIndividualDayIntake => {
                    var nonDietaryIntake = nonDietaryIntakeLookup?[dietaryIndividualDayIntake.SimulatedIndividualDayId];
                    var dustIntake = dustIntakeLookup?[dietaryIndividualDayIntake.SimulatedIndividualDayId];
                    var exposuresPerRouteSubstance = collectIndividualDayExposurePerRouteSubstance(
                        dietaryIndividualDayIntake,
                        nonDietaryIntake,
                        dustIntake,
                        exposureRoutes
                    );
                    return new ExternalIndividualDayExposure() {
                        Individual = dietaryIndividualDayIntake.Individual,
                        Day = dietaryIndividualDayIntake.Day,
                        SimulatedIndividualDayId = dietaryIndividualDayIntake.SimulatedIndividualDayId,
                        SimulatedIndividualId = dietaryIndividualDayIntake.SimulatedIndividualId,
                        IndividualSamplingWeight = dietaryIndividualDayIntake.IndividualSamplingWeight,
                        ExposuresPerRouteSubstance = exposuresPerRouteSubstance,
                    };
                })
                .OrderBy(r => r.SimulatedIndividualDayId)
                .Cast<IExternalIndividualDayExposure>()
                .ToList();
            return result;
        }

        /// <summary>
        /// Groups the aggregate individual day exposures by simulated individual to obtain
        /// a collection of aggregate individual exposures.
        /// </summary>
        public static List<IExternalIndividualExposure> CreateCombinedExternalIndividualExposures(
            List<IExternalIndividualDayExposure> externalIndividualDayExposures
        ) {
            // Combine dietary and non-dietary individual exposures
            var result = externalIndividualDayExposures
               .GroupBy(c => c.SimulatedIndividualId)
               .Select(c => {
                   var exposuresPerRouteSubstance = collectIndividualExposurePerRouteSubstance(c.ToList());
                   return new ExternalIndividualExposure() {
                       Individual = c.First().Individual,
                       SimulatedIndividualId = c.Key,
                       IndividualSamplingWeight = c.First().IndividualSamplingWeight,
                       ExposuresPerRouteSubstance = exposuresPerRouteSubstance,
                       ExternalIndividualDayExposures = c.Cast<IExternalIndividualDayExposure>().ToList(),
                   };
               })
               .OrderBy(r => r.SimulatedIndividualId)
               .Cast<IExternalIndividualExposure>()
               .ToList();
            return result;
        }

        /// <summary>
        /// Computes the individual day exposures per route compound.
        /// Change: aggregate the Oral dietary route with the Oral nondietary route.
        /// </summary>
        private static Dictionary<ExposurePathType, ICollection<IIntakePerCompound>> collectIndividualDayExposurePerRouteSubstance(
            DietaryIndividualDayIntake dietaryIndividualDayIntake,
            NonDietaryIndividualDayIntake nonDietaryIndividualDayIntake,
            DustIndividualDayExposure dustIndividualDayExposure,
            ICollection<ExposurePathType> exposureRoutes
        ) {
            var intakesPerRoute = new Dictionary<ExposurePathType, ICollection<IIntakePerCompound>>();
            var nonDietaryIntakesPerRouteSubstance = nonDietaryIndividualDayIntake?.GetTotalIntakesPerRouteSubstance();
            foreach (var route in exposureRoutes) {
                var intakesPerSubstance = new List<IIntakePerCompound>();
                if (route == ExposurePathType.Oral) {
                    if (dietaryIndividualDayIntake != null) {
                        var dietaryIntakePerSubstance = dietaryIndividualDayIntake
                            .GetDietaryIntakesPerSubstance();
                        intakesPerSubstance.AddRange(dietaryIntakePerSubstance);
                    }
                }
                if (nonDietaryIntakesPerRouteSubstance != null) {
                    var nonDietaryIntakePerSubstance = nonDietaryIntakesPerRouteSubstance?
                        .Where(c => c.Route == route)
                        .Select(g => new AggregateIntakePerCompound() {
                            Compound = g.Compound,
                            Amount = g.Amount,
                        })
                        .ToList();
                    intakesPerSubstance.AddRange(nonDietaryIntakePerSubstance);
                }
                if (dustIndividualDayExposure != null) {
                    // TO DO: filter before query
                    var dustExposurePerSubstance = dustIndividualDayExposure
                        .ExposurePerSubstanceRoute
                        .Where(r => r.Key.GetExposurePath() == route)
                        .SelectMany(r => r.Value)
                        .Select(g => new AggregateIntakePerCompound() {
                            Compound = g.Compound,
                            Amount = g.Amount
                        })
                        .ToList();
                    intakesPerSubstance.AddRange(dustExposurePerSubstance);
                }
                intakesPerRoute[route] = intakesPerSubstance
                    .GroupBy(c => c.Compound)
                    .Select(c => new AggregateIntakePerCompound() {
                        Compound = c.Key,
                        Amount = c.Sum(s => s.Amount)
                    })
                    .Cast<IIntakePerCompound>()
                    .ToList();
            }            
            return intakesPerRoute;
        }

        /// <summary>
        /// Computes the individual day exposures per route compound.
        /// </summary>
        /// <param name="externalIndividualDayExposures"></param>
        /// <returns></returns>
        private static Dictionary<ExposurePathType, ICollection<IIntakePerCompound>> collectIndividualExposurePerRouteSubstance(
            IEnumerable<IExternalIndividualDayExposure> externalIndividualDayExposures
        ) {
            var intakesPerRoute = new Dictionary<ExposurePathType, ICollection<IIntakePerCompound>>();
            var routes = externalIndividualDayExposures.First().ExposuresPerRouteSubstance.Keys;
            foreach (var route in routes) {
                var routeExposures = externalIndividualDayExposures
                    .SelectMany(r => r.ExposuresPerRouteSubstance[route], (r, i) => (r.Individual, i.Compound, i.Amount))
                    .GroupBy(c => c.Compound)
                    .Select(c => new AggregateIntakePerCompound() {
                        Compound = c.Key,
                        Amount = c.Sum(i => i.Amount) / externalIndividualDayExposures.Count()
                    })
                    .Cast<IIntakePerCompound>()
                    .ToList();
                intakesPerRoute[route] = routeExposures;
            }
            return intakesPerRoute;
        }
    }
}
