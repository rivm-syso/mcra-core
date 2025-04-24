using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.DietaryExposureCalculation.IndividualDietaryExposureCalculation;

namespace MCRA.Simulation.Calculators.DietaryExposureCalculation {
    public sealed class IntakePerModelledFood : IIntakePerModelledFood {
        public Food ModelledFood { get; set; }

        public double Exposure { get; set; }
    }
}
