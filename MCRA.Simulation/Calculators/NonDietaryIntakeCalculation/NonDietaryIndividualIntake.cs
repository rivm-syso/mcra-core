using MCRA.Data.Compiled.Objects;
using System.Collections.Generic;

namespace MCRA.Simulation.Calculators.NonDietaryIntakeCalculation
{

    /// <summary>
    /// Contains all information for a single individual.
    /// </summary>
    public sealed class NonDietaryIndividualIntake {

        public int SimulatedIndividualId { get; set; }

        public Individual Individual { get; set; }

        public double IndividualSamplingWeight { get; set; }

        public List<NonDietaryIndividualDayIntake> NonDietaryIndividualDayIntakes { get; set; }

        public int NumberOfDays { get; set; }

        public double NonDietaryIntakePerBodyWeight { get; set; }
    }
}
