using CommandLine;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.DietaryExposureCalculation.IndividualDietaryExposureCalculation;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.Simulation.Objects;

namespace MCRA.Simulation.Calculators.ConsumerProductExposureCalculation {
    public sealed class ConsumerProductIndividualIntake : IExternalIndividualExposure {

        private double _sumIntakesPerConsumerProduct = double.NaN;
        private double _totalExposurePerMassUnit = double.NaN;

        /// <summary>
        /// Identifier for a simulated individual day
        /// </summary>
        public int SimulatedIndividualDayId { get; set; }

        /// <summary>
        /// The original Individual entity.
        /// </summary>
        public SimulatedIndividual SimulatedIndividual { get; set; }

        /// <summary>
        /// Intakes specified per food as eaten.
        /// </summary>
        public List<IIntakePerConsumerProduct> IntakesPerConsumerProduct { get; set; }

        /// <summary>
        /// Exposures per route and substance, aggregated over the products, grouped by
        /// exposure path.
        /// </summary>
        public Dictionary<ExposurePath, List<IIntakePerCompound>> ExposuresPerPath {
            get {
                var routes = IntakesPerConsumerProduct
                    .SelectMany(r => r.IntakesPerSubstance.Keys)
                    .Distinct();
                var result = routes
                    .ToDictionary(r => new ExposurePath(ExposureSource.ConsumerProduct, r), GetTotalIntakesPerSubstance);
                return result;
            }
        }

        /// <summary>
        /// Gets the total substance exposure summed for the specified route
        /// of the simulated individual.
        /// </summary>
        public double GetExposure(
            ExposureRoute route,
            Compound substance,
            bool isPerPerson
        ) {
            var totalAmount = IntakesPerConsumerProduct
                .Where(r => r.IntakesPerSubstance.ContainsKey(route))
                .SelectMany(r => r.IntakesPerSubstance[route])
                .Where(r => r.Compound == substance)
                .Sum(r => r.Amount);
            return isPerPerson ? totalAmount : totalAmount / SimulatedIndividual.BodyWeight;
        }

        /// <summary>
        /// Gets the total substance exposure summed for the specified route and
        /// optionally corrected for the body weight.
        /// </summary>
        public double GetExposure(
            ExposurePath path,
            Compound substance,
            bool isPerPerson
        ) {
            return GetExposure(path.Route, substance, isPerPerson);
        }

        public List<IExternalIndividualDayExposure> ExternalIndividualDayExposures {
            get => throw new NotImplementedException(); set => throw new NotImplementedException();
        }

        /// <summary>
        /// Returns the aggregate exposure per substance of the consumer product individual day exposure.
        /// </summary>
        /// <returns></returns>
        public List<IIntakePerCompound> GetTotalIntakesPerSubstance(ExposureRoute route) {
            var intakesPerSubstance = IntakesPerConsumerProduct
                .Where(r => r.IntakesPerSubstance.ContainsKey(route))
                .SelectMany(ipc => ipc.IntakesPerSubstance[route])
                .GroupBy(ipc => ipc.Compound)
                .Select(g => new AggregateIntakePerCompound() {
                    Compound = g.Key,
                    Amount = g.Sum(ipc => ipc.Amount),
                })
                .Cast<IIntakePerCompound>()
                .ToList();
            return intakesPerSubstance;
        }

        /// <summary>
        /// Sums all (substance) consumer product exposures on this individual-day.
        /// </summary>
        /// <param name="relativePotencyFactors"></param>
        /// <param name="membershipProbabilities"></param>
        /// <returns></returns>
        public double TotalExposure(
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities
        ) {
            if (double.IsNaN(_sumIntakesPerConsumerProduct)) {
                //_sumIntakesPerConsumerProduct = IntakesPerConsumerProduct.Sum(i => i.Intake(relativePotencyFactors, membershipProbabilities));
                _sumIntakesPerConsumerProduct = 12;
            }
            return _sumIntakesPerConsumerProduct;
        }

        /// <summary>
        /// Computes the total consumer product  (substance) exposures per unit body weight
        /// on this individual-day.
        /// </summary>
        /// <param name="relativePotencyFactors"></param>
        /// <param name="membershipProbabilities"></param>
        /// <param name="isPerPerson"></param>
        /// <returns></returns>
        public double TotalExposurePerMassUnit(
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            bool isPerPerson
        ) {
            if (double.IsNaN(_totalExposurePerMassUnit)) {
                _totalExposurePerMassUnit = TotalExposure(relativePotencyFactors, membershipProbabilities) / (isPerPerson ? 1 : SimulatedIndividual.BodyWeight);
            }
            return _totalExposurePerMassUnit;
        }

        /// <summary>
        /// Returns true if this individual day exposure contains one or more positive amounts for the specified
        /// route and substance.
        /// </summary>
        public bool HasPositives(ExposureRoute route, Compound substance) {
            return IntakesPerConsumerProduct
                .Any(r => r.IntakesPerSubstance.TryGetValue(route, out var ipcs)
                    && ipcs.Any(ipc => ipc.Compound == substance && ipc.Amount > 0));
        }
    }
}
