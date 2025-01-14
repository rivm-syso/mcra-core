using MCRA.General;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;

namespace MCRA.Simulation.Calculators.KineticModelCalculation {
    public class AggregateIntakeCalculator {

        /// <summary>
        /// Merges the dietary, non-dietary, and dust individual day exposure collections into a
        /// collection of aggregate individual day exposures.
        /// Change: aggregate the Oral dietary route with the Oral nondietary route.
        /// </summary>
        public static List<IExternalIndividualDayExposure> CreateCombinedIndividualDayExposures(
            ICollection<DietaryIndividualDayIntake> dietaryIndividualDayIntakes,
            ICollection<ExternalExposureCollection> externalExposureCollections,
            ICollection<ExposureRoute> routes,
            ExposureUnitTriple targetUnit,
            ExposureType exposureType
        ) {
            var externalExposureLookup = (exposureType == ExposureType.Acute)
                ? externalExposureCollections
                    .Select(r => (
                        ExposureSource: r.ExposureUnit,
                        IndividualDayExposures: r.ExternalIndividualDayExposures
                            .ToDictionary(eidx => eidx.SimulatedIndividualDayId)
                    )).ToList()
                : externalExposureCollections
                    .Select(r => (
                        ExposureSource: r.ExposureUnit,
                        IndividualDayExposures: r.ExternalIndividualDayExposures
                            .ToDictionary(eidx => eidx.SimulatedIndividual.Id)
                    )).ToList();

            var result = dietaryIndividualDayIntakes
                .AsParallel()
                .Select(r => createExternalIndividualDayExposure(
                    externalExposureLookup,
                    routes,
                    exposureType,
                    targetUnit,
                    r
                ))
                .OrderBy(r => r.SimulatedIndividualDayId)
                .Cast<IExternalIndividualDayExposure>()
                .ToList();
            return result;
        }

        private static ExternalIndividualDayExposure createExternalIndividualDayExposure(
            ICollection<(ExposureUnitTriple, Dictionary<int, IExternalIndividualDayExposure>)> externalExposureLookup,
            ICollection<ExposureRoute> routes,
            ExposureType exposureType,
            ExposureUnitTriple targetUnit,
            DietaryIndividualDayIntake dietaryIndividualDayIntake
        ) {
            var externalExposure = (exposureType == ExposureType.Acute)
                ? externalExposureLookup
                    .Select(r => (r.Item1, r.Item2[dietaryIndividualDayIntake.SimulatedIndividualDayId]))
                    .ToList()
                : externalExposureLookup
                    .Select(r => (r.Item1, r.Item2[dietaryIndividualDayIntake.SimulatedIndividual.Id]))
                    .ToList();

            var exposuresPerRouteSubstance = collectIndividualDayExposurePerRouteSubstance(
                dietaryIndividualDayIntake,
                externalExposure,
                routes,
                targetUnit
            );
            return new ExternalIndividualDayExposure() {
                SimulatedIndividual = dietaryIndividualDayIntake.SimulatedIndividual,
                Day = dietaryIndividualDayIntake.Day,
                SimulatedIndividualDayId = dietaryIndividualDayIntake.SimulatedIndividualDayId,
                ExternalExposuresPerPath = exposuresPerRouteSubstance,
            };
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
               .GroupBy(c => c.SimulatedIndividual)
               .Select(c => {
                   var exposuresPerRouteSubstance = collectIndividualExposurePerRouteSubstance([.. c]);
                   return new ExternalIndividualExposure(c.Key) {
                       ExposuresPerRouteSubstance = exposuresPerRouteSubstance,
                       ExternalIndividualDayExposures = c.Cast<IExternalIndividualDayExposure>().ToList(),
                   };
               })
               .OrderBy(r => r.SimulatedIndividual.Id)
               .Cast<IExternalIndividualExposure>()
               .ToList();
            return result;
        }

        /// <summary>
        /// Computes the individual day exposures per route compound.
        /// Change: aggregate the Oral dietary route with the Oral nondietary route.
        /// </summary>
        private static Dictionary<ExposureRoute, List<IIntakePerCompound>> collectIndividualDayExposurePerRouteSubstance(
            DietaryIndividualDayIntake dietaryIndividualDayIntake,
            List<(ExposureUnitTriple, IExternalIndividualDayExposure)> externalIndividualDayExposures,
            ICollection<ExposureRoute> routes,
            ExposureUnitTriple targetUnit
        ) {
            var intakesPerRoute = new Dictionary<ExposureRoute, List<IIntakePerCompound>>();
            foreach (var route in routes) {
                var intakesPerSubstance = new List<IIntakePerCompound>();
                if (route == ExposureRoute.Oral) {
                    if (dietaryIndividualDayIntake != null) {
                        var dietaryIntakePerSubstance = dietaryIndividualDayIntake
                            .GetDietaryIntakesPerSubstance();
                        intakesPerSubstance.AddRange(dietaryIntakePerSubstance);
                    }
                }
                foreach (var externalIndividualDayExposure in externalIndividualDayExposures) {
                    var exposureUnit = externalIndividualDayExposure.Item1;

                    var bodyWeight = externalIndividualDayExposure.Item2.SimulatedIndividual.BodyWeight;
                    var externalExposurePerSubstance = externalIndividualDayExposure.Item2.ExposuresPerRouteSubstance
                        .Where(r => r.Key == route)
                        .SelectMany(r => r.Value)
                        .Select(g => {
                            var alignmentFactor = exposureUnit
                                .GetAlignmentFactor(targetUnit, g.Compound.MolecularMass, bodyWeight);
                            if (targetUnit.IsPerBodyWeight()) {
                                alignmentFactor *= bodyWeight;
                            }
                            var result = new AggregateIntakePerCompound() {
                                Compound = g.Compound,
                                Amount = g.Amount * alignmentFactor
                            };
                            return result;
                        })
                        .ToList();
                    intakesPerSubstance.AddRange(externalExposurePerSubstance);
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
        private static Dictionary<ExposureRoute, ICollection<IIntakePerCompound>> collectIndividualExposurePerRouteSubstance(
            IEnumerable<IExternalIndividualDayExposure> externalIndividualDayExposures
        ) {
            var intakesPerRoute = new Dictionary<ExposureRoute, ICollection<IIntakePerCompound>>();
            var routes = externalIndividualDayExposures.First().ExposuresPerRouteSubstance.Keys;
            foreach (var route in routes) {
                var routeExposures = externalIndividualDayExposures
                    .SelectMany(r => r.ExposuresPerRouteSubstance[route], (r, i) => (r.SimulatedIndividual, i.Compound, i.Amount))
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
