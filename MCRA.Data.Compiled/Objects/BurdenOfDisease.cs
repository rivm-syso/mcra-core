using MCRA.General;

namespace MCRA.Data.Compiled.Objects {

    /// <summary>
    /// Baseline burden of disease indicator for EBD calculation.
    /// </summary>
    public class BurdenOfDisease {
        public Population Population { get; set; }
        public Effect Effect { get; set; }
        public BodIndicator BodIndicator { get; set; }
        public double Value { get; set; }
        public BodIndicatorDistributionType BodUncertaintyDistribution { get; set; } = BodIndicatorDistributionType.Constant;
        public double? BodUncertaintyUpper { get; set; }
        public double? BodUncertaintyLower { get; set; }
    }
}
