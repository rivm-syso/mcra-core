using MCRA.General;

namespace MCRA.Data.Compiled.Objects {

    /// <summary>
    /// Baseline burden of disease indicator for EBD calculation.
    /// </summary>
    public sealed class BurdenOfDisease {
        public Population Population { get; set; }
        public Effect Effect { get; set; }
        public BodIndicator BodIndicator { get; set; }
        public double Value { get; set; }
    }
}
