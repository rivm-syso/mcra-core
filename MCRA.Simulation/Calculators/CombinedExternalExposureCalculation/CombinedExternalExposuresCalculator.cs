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
                        exposureUnit: c.ExposureUnit,
                        externalIndividualDayExposures: r
                    ))
                .GroupBy(c => c.externalIndividualDayExposures.SimulatedIndividualDayId)
                .Select(r => {
                    var externalExposures = r.Select(s => (s.exposureUnit, s.externalIndividualDayExposures)).ToList();
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
            List<IExternalIndividualDayExposure> externalIndividualDayExposures
        ) {
            var result = externalIndividualDayExposures
               .GroupBy(c => c.SimulatedIndividual)
               .Select(c => {
                   var exposuresPerRouteSubstance = collectIndividualExposurePerRouteSubstance([.. c]);
                   return new ExternalIndividualExposure(c.Key) {
                       ExposuresPerPath = exposuresPerRouteSubstance,
                       ExternalIndividualDayExposures = [.. c.Cast<IExternalIndividualDayExposure>()],
                   };
               })
               .OrderBy(r => r.SimulatedIndividual.Id)
               .Cast<IExternalIndividualExposure>()
               .ToList();
            return result;
        }

        private static ExternalIndividualDayExposure createExternalIndividualDayExposure(
            List<(ExposureUnitTriple, IExternalIndividualDayExposure)> externalExposures,
            ICollection<ExposurePath> paths,
            ExposureUnitTriple targetUnit
        ) {
            var exposuresPerPath = collectIndividualDayExposurePerRouteSubstance(
                externalExposures,
                paths,
                targetUnit
            );
            return new ExternalIndividualDayExposure(exposuresPerPath) {
                SimulatedIndividual = externalExposures.First().Item2.SimulatedIndividual,
                Day = externalExposures.First().Item2.Day,
                SimulatedIndividualDayId = externalExposures.First().Item2.SimulatedIndividualDayId
            };
        }

        /// <summary>
        /// Computes the individual day exposures per route compound.
        /// Change: aggregate the Oral dietary route with the Oral nondietary route.
        /// </summary>
        private static Dictionary<ExposurePath, List<IIntakePerCompound>> collectIndividualDayExposurePerRouteSubstance(
            List<(ExposureUnitTriple, IExternalIndividualDayExposure)> externalIndividualDayExposures,
            ICollection<ExposurePath> paths,
            ExposureUnitTriple targetUnit
        ) {
            var intakesPerPath = new Dictionary<ExposurePath, List<IIntakePerCompound>>();
            foreach (var path in paths) {
                var intakesPerSubstance = new List<IIntakePerCompound>();
                foreach (var externalIndividualDayExposure in externalIndividualDayExposures) {
                    var exposureUnit = externalIndividualDayExposure.Item1;
                    var bodyWeight = externalIndividualDayExposure.Item2.SimulatedIndividual.BodyWeight;
                    var externalExposurePerSubstance = externalIndividualDayExposure.Item2.ExposuresPerPath
                        .Where(r => r.Key == path)
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
            var routes = externalIndividualDayExposures.First().ExposuresPerPath.Keys;
            foreach (var route in routes) {
                var routeExposures = externalIndividualDayExposures
                    .SelectMany(r => r.ExposuresPerPath[route], (r, i) => (r.SimulatedIndividual, i.Compound, i.Amount))
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
