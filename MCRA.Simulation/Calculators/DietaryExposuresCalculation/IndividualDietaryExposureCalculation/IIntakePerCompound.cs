using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation {

    public interface IIntakePerCompound {

        /// <summary>
        /// The substance for which the exposure is simulated.
        /// </summary>
        Compound Compound { get; }

        /// <summary>
        /// The raw substance concentration (without any RPF/membership correction).
        /// </summary>
        double Exposure { get; }

        /// <summary>
        /// The total (substance) intake, calculated by summing over all portion.Intakes of the Portions property.
        /// </summary>
        double Intake(double rpf, double membershipProbability);

    }
}
