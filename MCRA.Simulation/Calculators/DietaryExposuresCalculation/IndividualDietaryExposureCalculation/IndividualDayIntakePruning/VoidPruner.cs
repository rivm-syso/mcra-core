using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;

namespace MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDayPruning {

    /// <summary>
    /// Void pruner: prunes nothing, returns the complete unpruned individual day intake object.
    /// </summary>
    public sealed class VoidPruner : IIndividualDayIntakePruner {

        public DietaryIndividualDayIntake Prune(
            DietaryIndividualDayIntake dietaryIndividualDayIntake
        ) {
            return dietaryIndividualDayIntake;
        }
    }
}
