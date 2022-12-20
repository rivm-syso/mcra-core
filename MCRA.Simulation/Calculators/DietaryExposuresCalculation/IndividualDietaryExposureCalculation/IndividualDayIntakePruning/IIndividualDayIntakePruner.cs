using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;

namespace MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDayPruning {
    public interface IIndividualDayIntakePruner {
        DietaryIndividualDayIntake Prune(
            DietaryIndividualDayIntake dietaryIndividualDayIntakes
        );
    }
}
