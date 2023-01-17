using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualConcentrationsCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation {
    public class HbmIndividualConcentration : ITargetIndividualExposure {
        public Individual Individual { get; set; }

        /// <summary>
        /// The (survey)day of the exposure.
        /// </summary>
        public int NumberOfDays { get; set; }

        /// <summary>
        /// Not applicable for HBM concentrations, so use default.
        /// </summary>
        public double CompartmentWeight => 1D;

        /// <summary>
        /// Not applicable for HBM concentrations, so use default.
        /// </summary>
        public double RelativeCompartmentWeight => 1D;

        public double IntraSpeciesDraw { get; set; }
        public int SimulatedIndividualId { get; set; }
        public double IndividualSamplingWeight { get; set; } = 1D;

        /// <summary>
        /// The target exposures by substance.
        /// </summary>
        public IDictionary<Compound, IHbmSubstanceTargetExposure> ConcentrationsBySubstance { get; set; } = new Dictionary<Compound, IHbmSubstanceTargetExposure>();

        public double GetExposureForSubstance(Compound compound) {
            return ConcentrationsBySubstance.ContainsKey(compound) ? ConcentrationsBySubstance[compound].Concentration : double.NaN;
        }

        public ISubstanceTargetExposureBase GetSubstanceTargetExposure(Compound compound) {
            return ConcentrationsBySubstance.ContainsKey(compound) ? ConcentrationsBySubstance[compound] : null;
        }

        /// <summary>
        /// Returns the (cumulative) substance conconcentration of the
        /// target. I.e., the total (corrected) amount divided by the
        /// volume of the target.
        /// </summary>
        /// <param name="substance"></param>
        /// <param name="isPerPerson"></param>
        /// <returns></returns>
        public double GetSubstanceConcentrationAtTarget(
            Compound substance,
            bool isPerPerson
            ) {
            if (!ConcentrationsBySubstance.ContainsKey(substance)) {
                return 0D;
            }
            return ConcentrationsBySubstance[substance].Concentration;
        }

        /// <summary>
        /// Substance amount at target (i.e., absolute amount), corrected for relative potency and membership probability.
        /// </summary>
        /// <param name="relativePotencyFactors"></param>
        /// <param name="membershipProbabilities"></param>
        /// <returns></returns>
        public double TotalAmountAtTarget(
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities
        ) {
            return ConcentrationsBySubstance?.Values
                .Sum(i => i.EquivalentSubstanceConcentration(
                    relativePotencyFactors[i.Substance], membershipProbabilities[i.Substance])
                ) ?? double.NaN;
        }

        /// <summary>
        /// Concentration at target (i.e., per kg bodyweight/organ weight) corrected for relative potency and membership probability.
        /// </summary>
        /// <param name="relativePotencyFactors"></param>
        /// <param name="membershipProbabilities"></param>
        /// <returns></returns>
        public double TotalConcentrationAtTarget(
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            bool isPerPerson
        ) {
            return TotalAmountAtTarget(relativePotencyFactors, membershipProbabilities);
        }

        /// <summary>
        /// Returns whether there is any positive substance amount.
        /// </summary>
        /// <returns></returns>
        public bool IsPositiveExposure() {
            return ConcentrationsBySubstance.Any(r => r.Value.Concentration > 0);
        }

    }
}
