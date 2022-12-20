using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDayPruning {

    /// <summary>
    /// Individual day intake pruner for grouping and aggregating all intakes
    /// per food by food-as-eaten.
    /// </summary>
    public sealed class AggregateByFoodAsMeasuredPruner : IIndividualDayIntakePruner {

        public DietaryIndividualDayIntake Prune(
            DietaryIndividualDayIntake dietaryIndividualDayIntake
        ) {
            var intakesPerFood = new List<IIntakePerFood>();
            var bodyweight = dietaryIndividualDayIntake.Individual.BodyWeight;
            var groupedIntakesPerFood = dietaryIndividualDayIntake.DetailedIntakesPerFood
                .GroupBy(r => r.FoodAsMeasured)
                .Select(r => {
                    var allIntakesPerCompound = r
                        .SelectMany(ipc => ipc.IntakesPerCompound)
                        .GroupBy(ipc => ipc.Compound)
                        .Select(ipc => new AggregateIntakePerCompound() {
                            Compound = ipc.Key,
                            Exposure = ipc.Sum(c => c.Exposure)
                        })
                        .Cast<IIntakePerCompound>()
                        .ToList();
                    return new AggregateIntakePerFood() {
                        FoodAsMeasured = r.Key,
                        BodyWeight = bodyweight,
                        GrossAmount = r.Sum(ipf => ipf.Amount),
                        NetAmount = r.Sum(ipf => ipf.Amount),
                        IntakesPerCompound = allIntakesPerCompound
                    };
                })
                .Cast<IIntakePerFood>()
                .ToList();

            groupedIntakesPerFood.TrimExcess();

            var result = new DietaryIndividualDayIntake() {
                SimulatedIndividualId = dietaryIndividualDayIntake.SimulatedIndividualId,
                SimulatedIndividualDayId = dietaryIndividualDayIntake.SimulatedIndividualDayId,
                Individual = dietaryIndividualDayIntake.Individual,
                Day = dietaryIndividualDayIntake.Day,
                IndividualSamplingWeight = dietaryIndividualDayIntake.IndividualSamplingWeight,
                IntakesPerFood = groupedIntakesPerFood,
            };
            return result;
        }
    }
}
