using System.Collections.Generic;
using System.Linq;
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;

namespace MCRA.Simulation.Calculators.TargetExposuresCalculation {
    public class TargetIndividualDayExposure : ITargetIndividualDayExposure {

        public int SimulatedIndividualId { get; set; }
        public int SimulatedIndividualDayId { get; set; }

        public double IndividualSamplingWeight { get; set; }

        public Individual Individual { get; set; }

        public string Day { get; set; }

        /// <summary>
        /// Relative weight of the compartment to the body weight.
        /// </summary>
        public double RelativeCompartmentWeight { get; set; } = 1D;

        /// <summary>
        /// Weight of target compartment.
        /// </summary>
        public double CompartmentWeight {
            get {
                return Individual.BodyWeight * RelativeCompartmentWeight;
            }
        }

        /// <summary>
        /// The target exposures by substance.
        /// </summary>
        public IDictionary<Compound, ISubstanceTargetExposure> TargetExposuresBySubstance { get; set; }
        public double IntraSpeciesDraw { get; set; }

        /// <summary>
        /// Target exposure (total)
        /// </summary>
        /// <param name="relativePotencyFactors"></param>
        /// <param name="membershipProbabilities"></param>
        /// <returns></returns>
        public double TotalAmountAtTarget(IDictionary<Compound, double> relativePotencyFactors, IDictionary<Compound, double> membershipProbabilities) {
            return TargetExposuresBySubstance?
                    .Values
                    .Sum(ipc => ipc.EquivalentSubstanceAmount(
                        relativePotencyFactors[ipc.Substance], membershipProbabilities[ipc.Substance])
                    ) ?? double.NaN;
        }

        /// <summary>
        /// Target exposure per bodyweight
        /// </summary>
        /// <param name="relativePotencyFactors"></param>
        /// <param name="membershipProbabilities"></param>
        /// <returns></returns>
        public double TotalConcentrationAtTarget(IDictionary<Compound, double> relativePotencyFactors, IDictionary<Compound, double> membershipProbabilities, bool isPerPerson) {
            return TotalAmountAtTarget(relativePotencyFactors, membershipProbabilities) / (isPerPerson ? 1 :RelativeCompartmentWeight * Individual.BodyWeight);
        }

        /// <summary>
        /// Returns whether there is any positive substance amount.
        /// </summary>
        /// <returns></returns>
        public bool IsPositiveExposure() {
            return TargetExposuresBySubstance.Any(r => r.Value.SubstanceAmount > 0);
        }

        public IDictionary<Food, IIntakePerModelledFood> IntakesPerModelledFood(IDictionary<Compound, double> relativePotencyFactors, IDictionary<Compound, double> membershipProbabilities, bool isPerPerson) {
            throw new System.NotImplementedException();
        }
    }
}
