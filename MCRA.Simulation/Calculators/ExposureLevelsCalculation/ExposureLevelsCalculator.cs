using MCRA.General;
using MCRA.Utils;

namespace MCRA.Simulation.Calculators.ExposureLevelsCalculation {
    public sealed class ExposureLevelsCalculator {

        /// <summary>
        /// Computes the exposure levels of the given exposures and reference dose.
        /// </summary>
        /// <param name="exposures"></param>
        /// <param name="exposureMethod"></param>
        /// <param name="exposureLevels"></param>
        /// <returns>The computed exposure levels.</returns>
        public static double[] GetExposureLevels(
            List<double> exposures,
            ExposureMethod exposureMethod,
            double[] exposureLevels
        ) {
            //create a copy of the ExposureLevels array, result array elements are edited below.
            var result = exposureLevels.ToArray();
            if (exposureMethod == ExposureMethod.Automatic) {
                var positiveIntakes = exposures.Where(c => c > 0).ToList();
                if (positiveIntakes.Any()) {
                    result = GriddingFunctions.GetAutomaticLevels(positiveIntakes.Min(), positiveIntakes.Max()).ToArray();
                }
            }
            return result;
        }
    }
}
