using MCRA.Simulation.Objects;

namespace MCRA.Simulation.Calculators.DietaryExposureCalculation.IndividualDietaryExposureCalculation {

    /// <summary>
    /// Contains all information for a single individual.
    /// </summary>
    public sealed class DietaryIndividualIntake {

        public SimulatedIndividual SimulatedIndividual { get; set; }

        public int NumberOfDays { get; set; }

        public double DietaryIntakePerMassUnit { get; set; }
    }
}
