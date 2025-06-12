using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.DietaryExposureCalculation.IndividualDietaryExposureCalculation;

namespace MCRA.Simulation.Calculators.ConsumerProductExposureCalculation{

    /// <summary>
    /// Summarizes all info for a consumer product.
    /// </summary>
    public interface IIntakePerConsumerProduct {

        /// <summary>
        /// The consumer product of this intake.
        /// </summary>
        ConsumerProduct Product { get; }

        /// <summary>
        /// The total compound exposure per consumer product.
        /// </summary>
        Dictionary<ExposureRoute, List<IIntakePerCompound>> IntakesPerSubstance { get; }

    }
}
