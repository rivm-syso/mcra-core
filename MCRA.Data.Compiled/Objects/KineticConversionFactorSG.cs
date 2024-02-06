using MCRA.General;

namespace MCRA.Data.Compiled.Objects {
    public sealed  class KineticConversionFactorSG {
        public string IdKineticConversionFactor { get; set; }
        public double ConversionFactor { get; set; }
        public double? UncertaintyUpper { get; set; }
        public double? AgeLower { get; set; }
        public GenderType Gender { get; set; }
    }
}
