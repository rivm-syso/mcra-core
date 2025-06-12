using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.DietaryExposureCalculation.IndividualDietaryExposureCalculation;

namespace MCRA.Simulation.Calculators.ConsumerProductExposureCalculation {

    /// <summary>
    /// Summarizes all info for a consumer product.
    /// </summary>
    public sealed class IntakePerConsumerProduct : IIntakePerConsumerProduct {

        /// <summary>
        /// The consumer product of this exposure.
        /// </summary>
        public ConsumerProduct Product { get; set; }

        /// <summary>
        /// The total substance exposure of consumer product.
        /// </summary>
        public Dictionary<ExposureRoute, List<IIntakePerCompound>> IntakesPerSubstance { get; set; }
    }
}
