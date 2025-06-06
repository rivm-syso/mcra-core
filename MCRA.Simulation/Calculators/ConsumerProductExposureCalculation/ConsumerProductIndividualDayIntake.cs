using CommandLine;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.DietaryExposureCalculation.IndividualDietaryExposureCalculation;
using MCRA.Simulation.Objects;

namespace MCRA.Simulation.Calculators.ConsumerProductExposureCalculation {
    public sealed class ConsumerProductIndividualDayIntake : IIndividualDay {

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
        /// The exposure day.
        /// </summary>
        public string Day { get; set; }

        /// <summary>
        /// Intakes specified per food as eaten.
        /// </summary>
        public List<IIntakePerConsumerProduct> IntakesPerConsumerProduct { get; set; }

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
                _sumIntakesPerConsumerProduct = IntakesPerConsumerProduct.Sum(i => i.Intake(relativePotencyFactors, membershipProbabilities));
            }
            return _sumIntakesPerConsumerProduct;
        }

        /// <summary>
        /// Computes the total consumer product (substance)exposures per unit body weight
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
        /// Returns the aggregate exposure per substance of the consumer product individual day exposure.
        /// </summary>
        /// <returns></returns>
        //public ICollection<IIntakePerCompound> GetTotalIntakesPerSubstance(ExposureRoute route) {
        public ICollection<IIntakePerCompound> GetTotalIntakesPerSubstance(ExposureRoute route) {
            var intakesPerSubstance = IntakesPerConsumerProduct
                .SelectMany(ipc => ipc.IntakesPerSubstance)
                .Where(c => c.ExposureRoute == route)
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
        /// Returns the total exposure of the substance of the consumer product individual day exposure.
        /// </summary>
        /// <returns></returns>
        public double GetSubstanceTotalExposure(Compound substance) {
            var totalIntake = IntakesPerConsumerProduct
                .SelectMany(ipf => ipf.IntakesPerSubstance.Where(ipc => ipc.Compound == substance))
                .Sum(r => r.Amount);
            return totalIntake;
        }

        /// <summary>
        /// Computes the total consumer product exposures per mass-unit on this individual-day.
        /// </summary>
        /// <param name="substance"></param>
        /// <param name="isPerPerson"></param>
        /// <returns></returns>
        public double GetSubstanceTotalExposurePerMassUnit(
            Compound substance,
            bool isPerPerson
        ) {
            var result = GetSubstanceTotalExposure(substance) / (isPerPerson ? 1D : SimulatedIndividual.BodyWeight);
            return result;
        }
    }
}
