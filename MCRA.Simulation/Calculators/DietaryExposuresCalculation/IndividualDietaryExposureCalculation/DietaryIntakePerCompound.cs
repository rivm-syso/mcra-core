using System.Collections.Generic;
using System.Linq;
using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation {

    public sealed class DietaryIntakePerCompound : IIntakePerCompound {

        /// <summary>
        /// Gets/sets the (monitoring) intake portions (concentration + amount).
        /// </summary>
        public IntakePortion IntakePortion { get; set; }

        /// <summary>
        /// Gets/sets the unit intake portions (concentration + amount) (for unit variability).
        /// </summary>
        public List<IntakePortion> UnitIntakePortions { get; set; }

        /// <summary>
        /// The processing type of this intake per substance.
        /// </summary>
        public ProcessingType ProcessingType { get; set; }

        /// <summary>
        /// Gets/sets the correction factor for e.g grapes
        /// Example: consumed 100 g raisons, is translated 300 g grapes. Correction factor is 3.
        /// </summary>
        public float ProcessingCorrectionFactor { get; set; } = 1f;

        /// <summary>
        /// Gets/sets the processing factor applied to this intake.
        /// </summary>
        public float ProcessingFactor { get; set; } = 1f;

        /// <summary>
        /// The substance for which the exposure is simulated.
        /// </summary>
        public Compound Compound { get; set; }

        /// <summary>
        /// The total (substance) intake, calculated by summing over all portion.Intakes of the Portions property.
        /// </summary>
        public double Intake(double rpf, double membershipProbability) {
            var intake = Exposure;
            intake *= rpf * membershipProbability;
            return intake;
        }

        /// <summary>
        /// The total amount consumed; for unit variability this is calculated by summing over the portions,
        /// otherwise this amount is equal to the consumed amount of the normal intake portion.
        /// </summary>
        public double TotalAmountConsumed {
            get {
                return UnitIntakePortions?.Sum(p => p.Amount) ?? IntakePortion.Amount;
            }
        }

        /// <summary>
        /// The total intake, summed over all portions (in case unit variability is applied) and including
        /// a correction for the processing factor (if applicable).
        /// </summary>
        public double Exposure {
            get {
                var intake = UnitIntakePortions != null
                    ? (UnitIntakePortions.Aggregate(0.0, (total, next) => total += next.Concentration * next.Amount))
                    : IntakePortion.Amount * IntakePortion.Concentration;
                intake *= ProcessingFactor / ProcessingCorrectionFactor;
                return intake;
            }
        }

        /// <summary>
        /// The mean concentration per unit of this intake; for unit variability this is calculated by taking
        /// the average over the unit variability portions, otherwise this concentration is equal to that of the
        /// normal intake portion.
        /// </summary>
        public double MeanConcentration {
            get {
                return UnitIntakePortions?.Average(c => c.Concentration) ?? IntakePortion.Concentration;
            }
        }
    }
}
