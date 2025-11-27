using MCRA.General;
using MCRA.Simulation.Calculators.DietaryExposureCalculation.IndividualDietaryExposureCalculation;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.Simulation.Objects;

namespace MCRA.Simulation.Calculators.CombinedExternalExposureCalculation {
    public class CombinedExternalExposuresCalculator {

        /// <summary>
        /// Merges the dietary, non-dietary, dust, soil and air individual day exposure collections into a
        /// collection of aggregate individual day exposures.
        /// </summary>
        public static List<IExternalIndividualDayExposure> CreateCombinedIndividualDayExposures(
            ICollection<ExternalExposureCollection> externalExposureCollections,
            ExposureUnitTriple targetUnit,
            ExposureType exposureType,
            CancellationToken cancelToken
        ) {
            var paths = externalExposureCollections
                .SelectMany(c => c.ExternalIndividualDayExposures.SelectMany(e => e.ExposuresPerPath.Keys))
                .ToHashSet();

            var result = externalExposureCollections
                .AsParallel()
                .WithCancellation(cancelToken)
                .SelectMany(c => c.ExternalIndividualDayExposures,
                    (c, r) => (
                        substanceAmountUnit: c.SubstanceAmountUnit,
                        externalIndividualDayExposures: r
                    ))
                .GroupBy(c => c.externalIndividualDayExposures.SimulatedIndividualDayId)
                .Select(r => {
                    var externalExposures = r.Select(s => (s.substanceAmountUnit, s.externalIndividualDayExposures)).ToList();
                    return createExternalIndividualDayExposure(
                        externalExposures,
                        paths,
                        targetUnit
                    );
                })
                .OrderBy(r => r.SimulatedIndividualDayId)
                .ThenBy(r => r.SimulatedIndividual.Id)
                .Cast<IExternalIndividualDayExposure>()
                .ToList();
            return result;
        }

        /// <summary>
        /// Groups the aggregate individual day exposures by simulated individual to obtain
        /// a collection of aggregate individual exposures.
        /// </summary>
        public static List<IExternalIndividualExposure> CreateCombinedExternalIndividualExposures(
            List<IExternalIndividualDayExposure> externalIndividualDayExposures,
            ICollection<ExposureRoute> routes
        ) {
            var result = externalIndividualDayExposures
               .GroupBy(c => c.SimulatedIndividual)
               .Select(c => {
                   var exposuresPerRouteSubstance = collectIndividualExposurePerRouteSubstance([.. c])
                       .Where(c => routes.Contains(c.Key.Route))
                       .ToDictionary((c => c.Key), c => c.Value);

                   return new ExternalIndividualExposure(c.Key, exposuresPerRouteSubstance) {
                       ExternalIndividualDayExposures = [.. c
                           .Select(r => new ExternalIndividualDayExposure(
                               r.ExposuresPerPath
                                   .Where(c => routes.Contains(c.Key.Route))
                                   .ToDictionary((c => c.Key), c => c.Value)
                            ) {
                               SimulatedIndividualDayId = r.SimulatedIndividualDayId,
                               SimulatedIndividual = r.SimulatedIndividual,
                               Day = r.Day,
                           })
                           .Cast<IExternalIndividualDayExposure>()],
                   };
               })
               .OrderBy(r => r.SimulatedIndividual.Id)
               .Cast<IExternalIndividualExposure>()
               .ToList();
            return result;
        }

        private static ExternalIndividualDayExposure createExternalIndividualDayExposure(
            List<(SubstanceAmountUnit AmountUnit, IExternalIndividualDayExposure IndividualDayExposure)> externalExposures,
            ICollection<ExposurePath> paths,
            ExposureUnitTriple targetUnit
        ) {
            var exposuresPerPath = collectIndividualDayExposurePerRouteSubstance(
                externalExposures,
                paths,
                targetUnit
            );
            return new ExternalIndividualDayExposure(exposuresPerPath) {
                SimulatedIndividual = externalExposures.First().IndividualDayExposure.SimulatedIndividual,
                Day = externalExposures.First().IndividualDayExposure.Day,
                SimulatedIndividualDayId = externalExposures.First().IndividualDayExposure.SimulatedIndividualDayId
            };
        }

        /// <summary>
        /// Computes the individual day exposures per route compound.
        /// Change: aggregate the Oral dietary route with the Oral nondietary route.
        /// </summary>
        private static Dictionary<ExposurePath, List<IIntakePerCompound>> collectIndividualDayExposurePerRouteSubstance(
            List<(SubstanceAmountUnit AmountUnit, IExternalIndividualDayExposure IndividualDayExposure)> externalIndividualDayExposures,
            ICollection<ExposurePath> paths,
            ExposureUnitTriple targetUnit
        ) {
            var intakesPerPath = new Dictionary<ExposurePath, List<IIntakePerCompound>>();
            foreach (var path in paths) {
                var intakesPerSubstance = new List<IIntakePerCompound>();
                foreach (var externalIndividualDayExposure in externalIndividualDayExposures) {
                    var exposureAmountUnit = externalIndividualDayExposure.AmountUnit;
                    var bodyWeight = externalIndividualDayExposure.IndividualDayExposure.SimulatedIndividual.BodyWeight;
                    var externalExposurePerSubstance = externalIndividualDayExposure.IndividualDayExposure.ExposuresPerPath
                        .Where(r => r.Key == path)
                        .SelectMany(r => r.Value)
                        .Select(g => {
                            var alignmentFactor = exposureAmountUnit
                                .GetMultiplicationFactor(targetUnit.SubstanceAmountUnit, g.Compound.MolecularMass);
                            var result = new AggregateIntakePerCompound() {
                                Compound = g.Compound,
                                Amount = g.Amount * alignmentFactor
                            };
                            return result;
                        })
                        .ToList();
                    intakesPerSubstance.AddRange(externalExposurePerSubstance);
                }

                intakesPerPath[path] = [.. intakesPerSubstance
                    .GroupBy(c => c.Compound)
                    .Select(c => new AggregateIntakePerCompound() {
                        Compound = c.Key,
                        Amount = c.Sum(s => s.Amount)
                    })
                    .Cast<IIntakePerCompound>()];
            }
            return intakesPerPath;
        }

        /// <summary>
        /// Computes the individual day exposures per route compound.
        /// </summary>
        private static Dictionary<ExposurePath, List<IIntakePerCompound>> collectIndividualExposurePerRouteSubstance(
            IEnumerable<IExternalIndividualDayExposure> externalIndividualDayExposures
        ) {
            var intakesPerRoute = new Dictionary<ExposurePath, List<IIntakePerCompound>>();
            var exposurePaths = externalIndividualDayExposures.First().ExposuresPerPath.Keys;
            foreach (var path in exposurePaths) {
                var routeExposures = externalIndividualDayExposures
                    .SelectMany(r => r.ExposuresPerPath[path], (r, i) => (r.SimulatedIndividual, i.Compound, i.Amount))
                    .GroupBy(c => c.Compound)
                    .Select(c => new AggregateIntakePerCompound() {
                        Compound = c.Key,
                        Amount = c.Sum(i => i.Amount) / externalIndividualDayExposures.Count()
                    })
                    .Cast<IIntakePerCompound>()
                    .ToList();
                intakesPerRoute[path] = routeExposures;
            }
            return intakesPerRoute;
        }
    }
}
