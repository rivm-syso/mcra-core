using MCRA.Simulation.Calculators.DietaryExposureCalculation.IndividualDietaryExposureCalculation;

namespace MCRA.Simulation.Calculators.DietaryExposureCalculation.IndividualDayPruning {

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
