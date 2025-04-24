using MCRA.Simulation.Calculators.DietaryExposureCalculation.IndividualDietaryExposureCalculation;

namespace MCRA.Simulation.Calculators.DietaryExposureCalculation.IndividualDayPruning {
    public interface IIndividualDayIntakePruner {
        DietaryIndividualDayIntake Prune(
            DietaryIndividualDayIntake dietaryIndividualDayIntakes
        );
    }
}
