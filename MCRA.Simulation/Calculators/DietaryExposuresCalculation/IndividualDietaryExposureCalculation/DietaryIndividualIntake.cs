using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;

namespace MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation {

    /// <summary>
    /// Contains all information for a single individual.
    /// </summary>
    public sealed class DietaryIndividualIntake {

        public int SimulatedIndividualId { get; set; }

        public Individual Individual { get; set; }

        public double IndividualSamplingWeight { get; set; }

        public int NumberOfDays { get; set; }

        public double DietaryIntakePerMassUnit { get; set; }

    }
}
