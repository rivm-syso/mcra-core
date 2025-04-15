using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;

namespace MCRA.Simulation.Calculators.DietaryExposuresCalculation {
    public sealed class IntakePerModelledFood : IIntakePerModelledFood {
        public Food ModelledFood { get; set; }

        public double Exposure { get; set; }
    }
}
