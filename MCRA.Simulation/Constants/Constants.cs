using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.Constants {
    public static class SimulationConstants {
        public static readonly Compound NullSubstance = new();
        public static readonly double MOE_eps = 10E7D;
        public static readonly double DefaultBodyWeight = 70D;

        public static int MinimalPercentileSampleSize(double percentage) {
            if (percentage > 50) {
                return (int)Math.Ceiling(300 / (100 - percentage));
            } else {
                return (int)Math.Ceiling(300 / percentage);
            }
        }
    }
}
