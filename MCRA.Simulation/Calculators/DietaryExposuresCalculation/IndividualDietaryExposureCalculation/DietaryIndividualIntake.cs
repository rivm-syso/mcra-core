using MCRA.Data.Compiled.Wrappers;

namespace MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation {

    /// <summary>
    /// Contains all information for a single individual.
    /// </summary>
    public sealed class DietaryIndividualIntake {

        public SimulatedIndividual SimulatedIndividual { get; set; }

        public int NumberOfDays { get; set; }

        public double DietaryIntakePerMassUnit { get; set; }
    }
}
