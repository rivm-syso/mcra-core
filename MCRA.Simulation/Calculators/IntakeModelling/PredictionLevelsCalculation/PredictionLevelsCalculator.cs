using MCRA.Simulation.Objects;
using MCRA.Simulation.Calculators.DietaryExposureCalculation.IndividualDietaryExposureCalculation;

namespace MCRA.Simulation.Calculators.IntakeModelling.PredictionLevelsCalculation {
    public class PredictionLevelsCalculator {

        /// <summary>
        /// Calculate predictions levels based on available levels and number of intervals
        /// and combine these with user specified prediction levels.
        /// </summary>
        /// <param name="individualDayAmounts"></param>
        /// <param name="intervals"></param>
        /// <param name="userSpecifiedPredictionsLevels"></param>
        /// <returns></returns>
        public static List<double> ComputePredictionLevels(
            ICollection<IIndividualDay> individualDayAmounts,
            double intervals,
            double[] userSpecifiedPredictionsLevels
        ) {
            var predictionLevels = new List<double>();
            if (individualDayAmounts?.Count > 0) {
                var min = individualDayAmounts.Select(c => c.SimulatedIndividual.Covariable).Min();
                var max = individualDayAmounts.Select(c => c.SimulatedIndividual.Covariable).Max();
                var range = ((max - min) / (intervals - 1));
                for (int i = 0; i < intervals; i++) {
                    predictionLevels.Add(Math.Floor(min));
                    min += range;
                }
            }
            if (userSpecifiedPredictionsLevels?.Length > 0) {
                predictionLevels.AddRange(userSpecifiedPredictionsLevels);
            }
            return predictionLevels.Distinct().Order().ToList();
        }

        /// <summary>
        /// Overload of <see cref="ComputePredictionLevels(ICollection{IIndividualDay}, double, double[])" />.
        /// </summary>
        /// <param name="individualDayAmounts"></param>
        /// <param name="intervals"></param>
        /// <param name="userSpecifiedPredictionsLevels"></param>
        /// <returns></returns>
        public static List<double> ComputePredictionLevels(
            ICollection<SimpleIndividualDayIntake> individualDayAmounts,
            double intervals,
            double[] userSpecifiedPredictionsLevels
        ) {
            return ComputePredictionLevels(
                individualDayAmounts?.Cast<IIndividualDay>().ToList(),
                intervals,
                userSpecifiedPredictionsLevels
            );
        }

        /// <summary>
        /// Overload of <see cref="ComputePredictionLevels(ICollection{IIndividualDay}, double, double[])" />.
        /// </summary>
        /// <param name="individualDayAmounts"></param>
        /// <param name="intervals"></param>
        /// <param name="userSpecifiedPredictionsLevels"></param>
        /// <returns></returns>
        public static List<double> ComputePredictionLevels(
            ICollection<DietaryIndividualDayIntake> individualDayAmounts,
            double intervals,
            double[] userSpecifiedPredictionsLevels
        ) {
            return ComputePredictionLevels(
                individualDayAmounts?.Cast<IIndividualDay>().ToList(),
                intervals,
                userSpecifiedPredictionsLevels
            );
        }
    }
}
