using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ConsumerProductExposureCalculation;
using MCRA.Simulation.Calculators.DietaryExposureCalculation.IndividualDietaryExposureCalculation;
using MCRA.Simulation.Objects;

namespace MCRA.Simulation.Calculators.ExternalExposureCalculation {
    public class ExternalIndividualDayExposure(
        Dictionary<ExposurePath, List<IIntakePerCompound>> exposuresPerPath
    ) : IExternalIndividualDayExposure {

        public Dictionary<ExposurePath, List<IIntakePerCompound>> ExposuresPerPath => exposuresPerPath;
        public SimulatedIndividual SimulatedIndividual { get; set; }
        public string Day { get; set; }
        public int SimulatedIndividualDayId { get; set; }

        /// <summary>
        /// Returns true if this individual day exposure contains one or more positive amounts for the specified
        /// route and substance.
        /// </summary>
        public bool HasPositives(ExposureRoute route, Compound substance) {
            return ExposuresPerPath
                .Where(e => e.Key.Route == route)
                .SelectMany(k => k.Value)
                .Where(i => i.Compound == substance)
                .Any(i => i.Amount > 0);
        }

        /// <summary>
        /// Gets the total (cumulative) external exposure expressed
        /// in reference substance equivalents by weighting by relative
        /// potency.
        /// </summary>
        public double GetExposure(
            IDictionary<Compound, double> rpfs,
            IDictionary<Compound, double> memberships,
            bool isPerPerson
        ) {
            var result = ExposuresPerPath.Values
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
        public double GetExposure(
            IDictionary<Compound, double> rpfs,
            IDictionary<Compound, double> memberships,
            IDictionary<(ExposureRoute, Compound), double> kineticConversionFactors,
            bool isPerPerson
        ) {
            var result = ExposuresPerPath
                .Sum(r => r.Value
                    .Sum(ipc => kineticConversionFactors[(r.Key.Route, ipc.Compound)]
                        * ipc.EquivalentSubstanceAmount(rpfs[ipc.Compound], memberships[ipc.Compound])
                ));
            return isPerPerson
                ? result
                : result / SimulatedIndividual.BodyWeight;
        }

        /// <summary>
        /// Gets the total external exposure for the specified substance multiplied by the kinetic conversion factors.
        /// </summary>
        public double GetExposure(
            Compound substance,
            IDictionary<(ExposureRoute, Compound), double> kineticConversionFactors,
            bool isPerPerson
        ) {
            var result = ExposuresPerPath
                .Sum(r => r.Value
                    .Where(r => r.Compound == substance)
                    .Sum(ipc => ipc.Amount > 0
                        ? ipc.Amount * kineticConversionFactors[(r.Key.Route, substance)]
                        : 0
                    )
                );
            return isPerPerson
                ? result
                : result / SimulatedIndividual.BodyWeight;
        }

        /// <summary>
        /// Gets the total substance exposure summed summed over all routes.
        /// </summary>
        public double GetExposure(
            Compound substance,
            bool isPerPerson
        ) {
            var result = ExposuresPerPath
                .Sum(r => r.Value
                    .Where(r => r.Compound == substance)
                    .Sum(ipc => ipc.Amount)
                );
            return isPerPerson ? result : result / SimulatedIndividual.BodyWeight;
        }

        /// <summary>
        /// Gets the total substance exposure summed for the specified route and optionally corrected for the body weight.
        /// </summary>
        public double GetExposure(
            ExposureRoute route,
            Compound substance,
            bool isPerPerson
        ) {
            var totalIntake = ExposuresPerPath
                .Where(e => e.Key.Route == route)
                .SelectMany(k => k.Value)
                .Where(i => i.Compound == substance)
                .Sum(i => i.Amount);
            return isPerPerson ? totalIntake : totalIntake / SimulatedIndividual.BodyWeight;
        }

        /// <summary>
        /// Gets the total substance exposure summed for the specified route.
        /// </summary>
        public double GetExposure(
            ExposureRoute route,
            Compound substance
        ) {
            return GetExposure(route, substance, isPerPerson: true);
        }

        /// <summary>
        /// Get the kinetic-converted total exposure for the specified route, corrected for RPFs and memberships, 
        /// summed for all substances, and optionally corrected for the body weight.
        /// </summary>
        public double GetExposure(
            ExposureRoute route,
            IDictionary<Compound, double> rpfs,
            IDictionary<Compound, double> memberships,
            IDictionary<(ExposureRoute, Compound), double> kineticConversionFactors,
            bool isPerPerson
        ) {
            var totalIntake = ExposuresPerPath
                .Where(e => e.Key.Route == route)
                .SelectMany(k => k.Value)
                .Sum(r => {
                    var exposure = r.EquivalentSubstanceAmount(rpfs[r.Compound], memberships[r.Compound]);
                    return exposure > 0
                        ? exposure * kineticConversionFactors[(route, r.Compound)]
                        : 0;
                });
            return isPerPerson ? totalIntake : totalIntake / SimulatedIndividual.BodyWeight;
        }

        /// <summary>
        /// Gets the total exposure for the specified route, cumulated of all sources and substances.
        /// </summary>
        public double GetExposure(
            ExposureRoute route,
            IDictionary<Compound, double> rpfs,
            IDictionary<Compound, double> memberships,
            bool isPerPerson
        ) {
            var totalIntake = ExposuresPerPath
                .Where(e => e.Key.Route == route)
                .SelectMany(k => k.Value)
                .Sum(r => r.EquivalentSubstanceAmount(rpfs[r.Compound], memberships[r.Compound]));
            return isPerPerson ? totalIntake : totalIntake / SimulatedIndividual.BodyWeight;
        }

        /// <summary>
        /// Get exposures by substance, where the exposure value for a substance is summed from different sources
        /// and routes.
        /// </summary>
        public ICollection<IIntakePerCompound> GetExposuresBySubstance() {
            var exposurePerSubstance = ExposuresPerPath
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

        /// <summary>
        /// Get exposures by substance for a specified route, where the exposure value for a substance is summed 
        /// from different sources.
        /// </summary>
        public ICollection<IIntakePerCompound> GetExposuresBySubstance(
            ExposureRoute exposureRoute
        ) {
            var exposuresPerSubstance = ExposuresPerPath
                .Where(r => r.Key.Route == exposureRoute)
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

        /// <summary>
        /// reverse dosimetry
        /// </summary>
        public static ExternalIndividualDayExposure FromSingleDose(
            ExposureRoute route,
            Compound compound,
            double dose,
            ExposureUnitTriple targetDoseUnit,
            SimulatedIndividual individual
        ) {
            var absoluteDose = targetDoseUnit.IsPerBodyWeight ? dose * individual.BodyWeight : dose;
            var exposuresPerRouteCompound = new AggregateIntakePerCompound() {
                Compound = compound,
                Amount = absoluteDose,
            };
            var exposuresPerPathSubstance = new Dictionary<ExposurePath, List<IIntakePerCompound>> {
                { new ExposurePath(ExposureSource.Undefined, route), [exposuresPerRouteCompound] }
            };

            return new ExternalIndividualDayExposure(exposuresPerPathSubstance) {
                SimulatedIndividual = individual,
            };
        }

        public static ExternalIndividualDayExposure FromDietaryIndividualDayIntake(
            DietaryIndividualDayIntake individualDay
        ) {
            var exposuresPerPath = new Dictionary<ExposurePath, List<IIntakePerCompound>> {
                [new(ExposureSource.Diet, ExposureRoute.Oral)] = [.. individualDay.GetTotalIntakesPerSubstance()]
            };
            var result = new ExternalIndividualDayExposure(exposuresPerPath) {
                SimulatedIndividualDayId = individualDay.SimulatedIndividualDayId,
                SimulatedIndividual = individualDay.SimulatedIndividual,
                Day = individualDay.Day
            };
            return result;
        }

        public static ExternalIndividualDayExposure FromConsumerProductIndividualIntake(
            ConsumerProductIndividualIntake individualDay,
            ICollection<ExposureRoute> routes
        ) {
            var exposuresPerPath = new Dictionary<ExposurePath, List<IIntakePerCompound>>();
            foreach (var route in routes) {
                exposuresPerPath[new(ExposureSource.ConsumerProducts, route)] = [.. individualDay.GetTotalIntakesPerSubstance(route)];
            }
            var result = new ExternalIndividualDayExposure(exposuresPerPath) {
                SimulatedIndividualDayId = individualDay.SimulatedIndividual.Id,
                SimulatedIndividual = individualDay.SimulatedIndividual
            };
            return result;
        }
    }
}
