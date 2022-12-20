using System;
using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation {

    /// <summary>
    /// Represents an intake for a substance aggregated over multiple sources (e.g., modelled foods).
    /// This class can be used to summarize/compress multiple intakes per substance in a single object.
    /// </summary>
    public sealed class AggregateIntakePerCompound : IIntakePerCompound {

        /// <summary>
        /// The total (substance) intake, calculated by summing over all portions.
        /// Intakes of the Portions property.
        /// </summary>
        public double Exposure { get; set; }

        /// <summary>
        /// The substance for which the exposure is simulated.
        /// </summary>
        public Compound Compound { get; set; }

        /// <summary>
        /// The total (substance) intake corrected by the specified rpf/membership probability.
        /// </summary>
        /// <param name="rpf"></param>
        /// <param name="membershipProbability"></param>
        /// <returns></returns>
        public double Intake(double rpf, double membershipProbability) {
            return Exposure * rpf * membershipProbability;
        }
    }
}
