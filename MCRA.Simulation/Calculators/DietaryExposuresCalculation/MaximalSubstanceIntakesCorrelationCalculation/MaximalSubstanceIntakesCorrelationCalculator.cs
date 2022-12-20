using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.ProgressReporting;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Simulation.Calculators.DietaryExposuresCalculation.MaximalSubstanceIntakesCorrelationCalculation {

    /// <summary>
    /// Calculator to maximise co-occurrence of high values in simulated intakes.
    /// </summary>
    public class MaximalSubstanceIntakesCorrelationCalculator {

        /// <summary>
        /// Order intakes per presence/absent pattern for multiple substances.
        /// </summary>
        /// <param name="dietaryIndividualDayIntakes"></param>
        /// <param name="progressState"></param>
        public static void Compute(
            ICollection<DietaryIndividualDayIntake> dietaryIndividualDayIntakes,
            ProgressState progressState
        ) {
            var cancelToken = progressState?.CancellationToken ?? new System.Threading.CancellationToken();
            var groupedIntakesPerFood = dietaryIndividualDayIntakes
                .SelectMany(idi => idi.DetailedIntakesPerFood)
                .GroupBy(r => r, new PatternComparer())
                .ToList();

            foreach (var intakesPerFood in groupedIntakesPerFood) {
                var numSubstances = intakesPerFood.First().IntakesPerCompound.Count;
                var result = new List<List<float>>();
                for (int i = 0; i < numSubstances; i++) {
                    result.Add(intakesPerFood
                        .Select(ipf => (float)ipf.DetailedIntakesPerCompound
                            .OrderBy(r => r.Compound.Code)
                            .ElementAt(i).IntakePortion.Concentration
                        )
                        .OrderBy(v => v)
                        .ToList());
                }
                var intakesPerFoodList = intakesPerFood.ToList();
                for (int i = 0; i < intakesPerFoodList.Count; i++) {
                    for (int j = 0; j < numSubstances; j++) {
                        intakesPerFoodList[i].DetailedIntakesPerCompound
                            .OrderBy(r => r.Compound.Code)
                            .ElementAt(j).IntakePortion.Concentration = result[j][i];
                    }
                }
            }
        }

        internal class PatternComparer : IEqualityComparer<IntakePerFood> {
            public bool Equals(IntakePerFood x, IntakePerFood y) {
                return PatternString(x) == PatternString(y);
            }

            public int GetHashCode(IntakePerFood obj) {
                return PatternString(obj).GetChecksum();
            }

            public string PatternString(IntakePerFood record) {
                var positiveSubstanceCodes = record.DetailedIntakesPerCompound
                    .Where(r => r.MeanConcentration > 0)
                    .Select(r => r.Compound.Code)
                    .OrderBy(r => r);
                return string.Join("\a", positiveSubstanceCodes);
            }
        }
    }
}
