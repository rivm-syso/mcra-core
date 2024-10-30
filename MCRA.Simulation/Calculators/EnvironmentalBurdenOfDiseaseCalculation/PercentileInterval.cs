namespace MCRA.Simulation.Calculators.EnvironmentalBurdenOfDiseaseCalculation {
    public struct PercentileInterval(double lower, double upper) {
        public double Lower { get; set; } = lower;
        public double Upper { get; set; } = upper;

        public override readonly string ToString() {
            var res = Lower == 0
                ? $"< {Upper}"
                : Upper == 100
                    ? $"> {Lower}"
                    : $"{Lower} - {Upper}";
            return res;
        }
    }
}
