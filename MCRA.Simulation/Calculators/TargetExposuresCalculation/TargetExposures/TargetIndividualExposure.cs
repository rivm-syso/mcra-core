using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.Calculators.TargetExposuresCalculation {
    public class TargetIndividualExposure : ITargetIndividualExposure {

        /// <summary>
        /// Individual record for which the exposure is modelled.
        /// </summary>
        public Individual Individual { get; set; }

        /// <summary>
        /// Sampling weight of the individual in the population.
        /// </summary>
        public double IndividualSamplingWeight { get; set; }

        /// <summary>
        /// The id assigned to the simulated individual.
        /// The sampling weight of the simulated individual.
        /// For ExposureType == acute and numberOfIterations == 0, use samplingweights to determine percentiles (USESAMPLINGWEIGHTS):
        ///   - always correct input,
        ///   - correct output; 
        /// For ExposureType == acute and numberOfIterations > 0, no samplingweights to determine percentiles, weights are already in simulated exposures (DO NOT USESAMPLINGWEIGHTS)
        ///   - always correct input,
        ///   - output is already weighted;
        ///  For ExposureType == chronic (USESAMPLINGWEIGHTS)
        ///   - always correct input,
        ///   - correct output; 
        /// </summary>
        public int SimulatedIndividualId { get; set; }

        /// <summary>
        /// The body weight of the individual as used in calculations, which is most of the times equal to the 
        /// original individual body weight read from the data or an imputed value when the body weight is missing.
        /// </summary>
        public double SimulatedIndividualBodyWeight => Individual.BodyWeight;

        /// <summary>
        /// Relative weight of the compartment to the body weight.
        /// </summary>
        public double RelativeCompartmentWeight { get; set; } = 1D;

        /// <summary>
        /// The target exposures by substance.
        /// </summary>
        public IDictionary<Compound, ISubstanceTargetExposure> TargetExposuresBySubstance { get; set; } = new Dictionary<Compound, ISubstanceTargetExposure>();

        /// <summary>
        /// Gets the substances for which exposures are recorded.
        /// </summary>
        public ICollection<Compound> Substances {
            get {
                return TargetExposuresBySubstance.Keys;
            }
        }

        /// <summary>
        /// Weight of target compartment.
        /// </summary>
        public double CompartmentWeight {
            get {
                return SimulatedIndividualBodyWeight * RelativeCompartmentWeight;
            }
        }

        public double IntraSpeciesDraw { get; set; }

        public double GetExposureForSubstance(Compound compound) {
            return TargetExposuresBySubstance.ContainsKey(compound)
                ? TargetExposuresBySubstance[compound].SubstanceAmount
                : double.NaN;
        }

        /// <summary>
        /// Gets the target exposure value for a substance, corrected for relative
        /// potency and membership probability.
        /// </summary>
        public double GetExposureForSubstance(
            Compound substance,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            bool isPerPerson
        ) {
            return TargetExposuresBySubstance.ContainsKey(substance)
                 ? TargetExposuresBySubstance[substance].EquivalentSubstanceAmount(relativePotencyFactors[substance], membershipProbabilities[substance])
                        / (isPerPerson ? 1 : CompartmentWeight)
                 : 0D;
        }

        public ISubstanceTargetExposureBase GetSubstanceTargetExposure(Compound compound) {
            return TargetExposuresBySubstance.ContainsKey(compound)
                ? TargetExposuresBySubstance[compound]
                : null;
        }

        /// <summary>
        /// Returns the (cumulative) substance conconcentration of the
        /// target. I.e., the total (corrected) amoount divided by the
        /// volume of the target.
        /// </summary>
        public double GetSubstanceConcentrationAtTarget(
            Compound substance,
            bool isPerPerson
        ) {
            if (!TargetExposuresBySubstance.ContainsKey(substance)) {
                return 0D;
            }
            return TargetExposuresBySubstance[substance].SubstanceAmount 
                / (isPerPerson ? 1 : CompartmentWeight);
        }

        /// <summary>
        /// Substance amount at target (i.e., absoulte amount), corrected for relative potency and membership probability.
        /// </summary>
        public double TotalAmountAtTarget(
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities
        ) {
            return TargetExposuresBySubstance.Values.Sum(ipc => ipc.EquivalentSubstanceAmount(relativePotencyFactors[ipc.Substance], membershipProbabilities[ipc.Substance]));
        }

        /// <summary>
        /// Concentration at target (i.e., per kg bodyweight/organ weight) corrected for relative potency and membership probability.
        /// </summary>
        public double TotalConcentrationAtTarget(
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            bool isPerPerson
        ) {
            return TotalAmountAtTarget(relativePotencyFactors, membershipProbabilities) / (isPerPerson ? 1 : CompartmentWeight);
        }

        /// <summary>
        /// Returns whether there is any positive substance amount.
        /// </summary>
        /// <returns></returns>
        public bool IsPositiveExposure() {
            return TargetExposuresBySubstance.Any(r => r.Value.SubstanceAmount > 0);
        }
    }
}
