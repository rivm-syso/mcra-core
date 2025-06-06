using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Simulation.Calculators.DietaryExposureCalculation.IndividualDietaryExposureCalculation {

    public interface IIntakePerCompound {

        /// <summary>
        /// The substance for which the exposure is simulated.
        /// </summary>
        Compound Compound { get; }

        /// <summary>
        /// The raw substance intake amount (without any RPF/membership correction).
        /// </summary>
        double Amount { get; }

        /// <summary>
        /// The total (substance) intake, calculated by summing over all portion.Intakes of the Portions property.
        /// </summary>
        double EquivalentSubstanceAmount(double rpf, double membershipProbability);

        ExposureRoute ExposureRoute { get; }
    }
}
