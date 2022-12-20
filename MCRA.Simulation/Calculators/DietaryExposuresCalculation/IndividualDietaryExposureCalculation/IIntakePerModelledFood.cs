using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation {
    public interface IIntakePerModelledFood {
        /// <summary>
        /// The food for which the exposure is simulated.
        /// </summary>
        Food ModelledFood { get; }

        /// <summary>
        /// The exposure (with RPF/membership correction)
        /// </summary>
        double Exposure { get; }
    }
}
