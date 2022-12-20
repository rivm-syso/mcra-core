using System.Collections.Generic;
using System.Linq;
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;

namespace MCRA.Simulation.Calculators.TargetExposuresCalculation {
    public class TargetIndividualExposure : ITargetIndividualExposure {

        public TargetIndividualExposure() { }

        public Individual Individual { get; set; }
        public double IndividualSamplingWeight { get; set; }
        public int SimulatedIndividualId { get; set; }

        /// <summary>
        /// Relative weight of the compartment to the body weight.
        /// </summary>
        public double RelativeCompartmentWeight { get; set; }

        /// <summary>
        /// The target exposures by substance.
        /// </summary>
        public IDictionary<Compound, ISubstanceTargetExposure> TargetExposuresBySubstance { get; set; }

        /// <summary>
        /// Weight of target compartment.
        /// </summary>
        public double CompartmentWeight {
            get {
                return Individual.BodyWeight * RelativeCompartmentWeight;
            }
        }

        public double IntraSpeciesDraw { get; set; }

        /// <summary>
        /// Substance amount at target (i.e., absoulte amount), corrected for relative potency and membership probability.
        /// </summary>
        /// <param name="relativePotencyFactors"></param>
        /// <param name="membershipProbabilities"></param>
        /// <returns></returns>
        public double TotalAmountAtTarget(IDictionary<Compound, double> relativePotencyFactors, IDictionary<Compound, double> membershipProbabilities) {
            return TargetExposuresBySubstance.Values.Sum(ipc => ipc.EquivalentSubstanceAmount(relativePotencyFactors[ipc.Substance], membershipProbabilities[ipc.Substance]));
        }

        /// <summary>
        /// Concentration at target (i.e., per kg bodyweight/organ weight) corrected for relative potency and membership probability.
        /// </summary>
        /// <param name="relativePotencyFactors"></param>
        /// <param name="membershipProbabilities"></param>
        /// <returns></returns>
        public double TotalConcentrationAtTarget(IDictionary<Compound, double> relativePotencyFactors, IDictionary<Compound, double> membershipProbabilities, bool isPerPerson) {
            return TotalAmountAtTarget(relativePotencyFactors, membershipProbabilities) / (isPerPerson ? 1 : CompartmentWeight);
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
