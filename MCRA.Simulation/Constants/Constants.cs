using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.Constants {
    public static class SimulationConstants {
        public static readonly Compound NullSubstance = new();
        public static readonly double MOE_eps = 10E7D;
        public static readonly double DefaultBodyWeight = 70D;

        /// <summary>
        /// Returns the minimum sample size needed for reporting percentiles
        /// at the given percentage in accordance to the privacy guidelines.
        /// </summary>
        /// <param name="percentage"></param>
        /// <returns></returns>
        public static int MinimalPercentileSampleSize(double percentage) {
            if (percentage > 50) {
                return (int)Math.Ceiling(300 / (100 - percentage));
            } else {
                return (int)Math.Ceiling(300 / percentage);
            }
        }

        /// <summary>
        /// Returns for the specified sample size the maximum allowed percentile
        /// point (rounded to the nearest reasonable value) for reporting in 
        /// accordance with the privacy guidelines.
        /// </summary>
        /// <param name="sampleSize"></param>
        /// <returns></returns>
        public static double MaxUpperPercentage(int sampleSize) {
            var min = 300D / sampleSize;
            if (min < 0.001) {
                min = 0.001;
            } else if (min < 0.01) {
                min = 0.01;
            } else if (min < 0.1) {
                min = 0.1;
            } else if (min < 1) {
                min = 1;
            } else if (min < 2) {
                min = 2;
            } else if (min < 2.5) {
                min = 2.5;
            } else {
                min = Math.Ceiling(min);
            }
            return 100 - min;
        }
    }
}
