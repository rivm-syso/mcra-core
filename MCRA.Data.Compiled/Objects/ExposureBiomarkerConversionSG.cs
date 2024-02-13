using MCRA.General;

namespace MCRA.Data.Compiled.Objects {
    public sealed  class ExposureBiomarkerConversionSG {
        public string IdExposureBiomarkerConversion { get; set; }
        public double ConversionFactor { get; set; }
        public double? VariabilityUpper { get; set; }
        public double? AgeLower { get; set; }
        public GenderType Gender { get; set; }
    }
}
