using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.Objects {
    public sealed class GroupedScreeningResultRecord {
        public Food FoodAsMeasured { get; set; }
        public Compound Compound { get; set; }
        public List<ScreeningResultRecord> ScreeningRecords { get; set; }
        public double Contribution { get; set; }
        public double CumulativeContributionFraction { get; set; }
    }
}
