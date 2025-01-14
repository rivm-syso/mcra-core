using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;

namespace MCRA.Simulation.Calculators.TargetExposuresCalculation {
    public class TargetIndividualExposure : ITargetIndividualExposure {

        /// <summary>
        /// Individual record for which the exposure is modelled.
        /// </summary>
        public SimulatedIndividual SimulatedIndividual { get; set; }

        /// <summary>
        /// The body weight of the individual as used in calculations, which is
        /// most of the times equal to the original individual body weight read
        /// from the data or an imputed value when the body weight is missing.
        /// </summary>
        public double SimulatedIndividualBodyWeight => SimulatedIndividual.BodyWeight;

        public double IntraSpeciesDraw { get; set; }

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
        /// Returns the (cumulative) substance conconcentration of the
        /// target. I.e., the total (corrected) amoount divided by the
        /// volume of the target.
        /// </summary>
        public double GetSubstanceExposure(Compound substance) {
            if (!TargetExposuresBySubstance.ContainsKey(substance)) {
                return 0D;
            }
            return TargetExposuresBySubstance[substance].Exposure;
        }

        /// <summary>
        /// Gets the target exposure value for a substance, corrected for relative
        /// potency and membership probability.
        /// </summary>
        public double GetSubstanceExposure(
            Compound substance,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities
        ) {
            return TargetExposuresBySubstance.ContainsKey(substance)
                 ? TargetExposuresBySubstance[substance]
                    .EquivalentSubstanceExposure(relativePotencyFactors[substance], membershipProbabilities[substance])
                 : 0D;
        }

        public ISubstanceTargetExposure GetSubstanceTargetExposure(
            Compound compound
        ) {
            return TargetExposuresBySubstance.ContainsKey(compound)
                ? TargetExposuresBySubstance[compound]
                : null;
        }

        /// <summary>
        /// Concentration at target (i.e., per kg bodyweight/organ weight) corrected for relative potency and membership probability.
        /// </summary>
        public double GetCumulativeExposure(
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities
        ) {
            return TargetExposuresBySubstance.Values
                .Sum(ipc => ipc
                    .EquivalentSubstanceExposure(
                        relativePotencyFactors[ipc.Substance],
                        membershipProbabilities[ipc.Substance]
                    )
                );
        }

        /// <summary>
        /// Returns whether there is any positive substance amount.
        /// </summary>
        public bool IsPositiveExposure() {
            return TargetExposuresBySubstance.Any(r => r.Value.Exposure > 0);
        }
    }
}
