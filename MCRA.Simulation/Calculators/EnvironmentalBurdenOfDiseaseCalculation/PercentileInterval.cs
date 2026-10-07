namespace MCRA.Simulation.Calculators.EnvironmentalBurdenOfDiseaseCalculation {
    public struct PercentileInterval(double lower, double upper) {
        public double Lower { get; set; } = lower;
        public double Upper { get; set; } = upper;

        public readonly double Percentage => (double.IsNaN(Upper) ? 100 : Upper) - (double.IsNaN(Lower) ? 0 : Lower);

        public override readonly string ToString() {
            var res = Lower == 0 || double.IsNaN(Lower)
                ? $"< {Upper:G3}"
                : Upper == 100 || double.IsNaN(Upper)
                    ? $">= {Lower:G3}"
                    : $"{Lower:G3} - {Upper:G3}";
            return res;
        }
    }
}
