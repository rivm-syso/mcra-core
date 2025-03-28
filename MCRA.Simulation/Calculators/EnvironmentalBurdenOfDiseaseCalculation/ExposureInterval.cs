namespace MCRA.Simulation.Calculators.EnvironmentalBurdenOfDiseaseCalculation {
    public struct ExposureInterval(double lower, double upper) {
        public double Lower { get; set; } = lower;
        public double Upper { get; set; } = upper;

        public override readonly string ToString() {
            var res = double.IsNaN(Lower) || Lower == 0
                ? $"< {Upper:G3}"
                : double.IsNaN(Upper)
                    ? $"> {Lower:G3}"
                    : $"{Lower:G3} - {Upper:G3}";
            return res;
        }
    }
}
