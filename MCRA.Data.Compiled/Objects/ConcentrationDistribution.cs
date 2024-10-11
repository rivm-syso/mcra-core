using MCRA.General;

namespace MCRA.Data.Compiled.Objects {
    public sealed class ConcentrationDistribution {
        public Food Food { get; set; }
        public Compound Compound { get; set; }

        public double Mean { get; set; }
        public double? CV { get; set; }
        public double? Percentile { get; set; }
        public double? Percentage { get; set; }
        public double? Limit { get; set; }

        public ConcentrationUnit ConcentrationUnit { get; set; } = ConcentrationUnit.mgPerKg;
    }
}
