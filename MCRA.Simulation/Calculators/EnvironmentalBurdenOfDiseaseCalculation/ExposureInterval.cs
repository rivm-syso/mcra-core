using MCRA.General;

namespace MCRA.Simulation.Calculators.EnvironmentalBurdenOfDiseaseCalculation {
    public struct ExposureInterval(double lower, double upper, SubgroupIntervalBoundType subgroupIntervalBoundType) {
        public double Lower { get; set; } = lower;
        public double Upper { get; set; } = upper;
        public SubgroupIntervalBoundType SubgroupIntervalBoundType { get; set; } = subgroupIntervalBoundType;

        public readonly double Average =>
            double.IsNaN(Lower) || Lower == 0 ? Upper
            : double.IsNaN(Upper) ? Lower
            : (Upper + Lower) / 2;

        public override readonly string ToString() {
            var isRightInclusive = SubgroupIntervalBoundType == SubgroupIntervalBoundType.RightInclusive;
            var hasNoLower = double.IsNaN(Lower) || Lower == 0;
            var hasNoUpper = double.IsNaN(Upper) || Upper == double.PositiveInfinity;

            var res = hasNoLower
                ? isRightInclusive ? $"<= {Upper:G3}" : $"< {Upper:G3}"
                : hasNoUpper
                    ? isRightInclusive ? $"> {Lower:G3}" : $">= {Lower:G3}"
                    : isRightInclusive ? $"({Lower:G3} - {Upper:G3}]" : $"[{Lower:G3} - {Upper:G3})";

            return res;
        }
    }
}
