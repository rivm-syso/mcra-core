using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Simulation.Calculators.TargetExposuresCalculation {

    public class AggregateIndividualExposure : ITargetExposure, ITargetIndividualExposure, IExternalIndividualExposure {

        private double _totalConcentrationAtTarget = double.NaN;

        /// <summary>
        /// The id assigned to the simulated individual.
        /// </summary>
        public int SimulatedIndividualId { get; set; }

        /// <summary>
        /// The id assigned to the simulated individual day.
        /// </summary>
        public int SimulatedIndividualDayId { get; set; }

        /// <summary>
        /// The individual used for simulation.
        /// </summary>
        public Individual Individual { get; set; }

        /// <summary>
        /// The individual sampling weight.
        /// </summary>
        public double IndividualSamplingWeight { get; set; }

        /// <summary>
        /// Relative weight of the compartment to the body weight.
        /// </summary>
        public double RelativeCompartmentWeight { get; set; }

        /// <summary>
        /// Weight of target compartment.
        /// </summary>
        public double CompartmentWeight {
            get {
                return Individual.BodyWeight * RelativeCompartmentWeight;
            }
        }

        /// <summary>
        /// Usual external exposure per route and substance.
        /// </summary>
        public IDictionary<ExposureRouteType, ICollection<IIntakePerCompound>> ExposuresPerRouteSubstance { get; set; }

        /// <summary>
        /// Individual day exposures per route.
        /// </summary>
        public List<IExternalIndividualDayExposure> ExternalIndividualDayExposures { get; set; }

        /// <summary>
        /// Target exposures per substance.
        /// </summary>
        public IDictionary<Compound, ISubstanceTargetExposure> TargetExposuresBySubstance { get; set; }
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
            if (double.IsNaN(_totalConcentrationAtTarget)) {
                _totalConcentrationAtTarget = TotalAmountAtTarget(relativePotencyFactors, membershipProbabilities) / (isPerPerson ? 1 : CompartmentWeight);
            }
            return _totalConcentrationAtTarget;
        }

        /// <summary>
        /// Returns the total intake of the substance of the dietary individual day intake.
        /// </summary>
        /// <returns></returns>
        public double GetSubstanceTotalExposure(Compound substance) {
            return (TargetExposuresBySubstance?.ContainsKey(substance) ?? false)
                ? TargetExposuresBySubstance[substance].SubstanceAmount : 0D;
        }

        /// <summary>
        /// Computes the total dietary substance exposures per mass-unit on this individual-day.
        /// </summary>
        /// <param name="substance"></param>
        /// <param name="isPerPerson"></param>
        /// <returns></returns>
        public double GetSubstanceTotalExposurePerMassUnit(
            Compound substance,
            bool isPerPerson
        ) {
            var result = GetSubstanceTotalExposure(substance) / (isPerPerson ? 1D : RelativeCompartmentWeight * Individual.BodyWeight);
            return result;
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
