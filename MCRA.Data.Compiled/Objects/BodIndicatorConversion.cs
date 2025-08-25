using MCRA.General;

namespace MCRA.Data.Compiled.Objects {

    /// <summary>
    /// Burden of disease indicator conversion.
    /// </summary>
    public sealed class BodIndicatorConversion {
        public BodIndicator FromIndicator { get; set; }
        public string FromUnit { get; set; }
        public BodIndicator ToIndicator { get; set; }
        public string ToUnit { get; set; }
        public double Value { get; set; }
    }
}
