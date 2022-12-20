using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;

namespace MCRA.Simulation.Calculators.DietaryExposuresCalculation {
    public sealed class IntakePerModelledFoodSubstance : IIntakePerModelledFoodSubstance {
        public Food ModelledFood { get; set; }
        public Compound Substance { get; set; }
        public double Exposure { get; set; }
    }
}
