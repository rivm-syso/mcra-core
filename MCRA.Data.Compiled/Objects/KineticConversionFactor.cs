using MCRA.General;

namespace MCRA.Data.Compiled.Objects {
    public sealed class KineticConversionFactor {
        public Compound SubstanceFrom { get; set; }
        public ExposureRouteType ExposureRouteFrom { get; set; }
        public BiologicalMatrix BiologicalMatrixFrom { get; set; }
        public DoseUnit DoseUnitFrom { get; set; }
        public ExpressionType ExpressionTypeFrom { get; set; }
        public Compound SubstanceTo { get; set; }
        public ExposureRouteType ExposureRouteTo { get; set; }
        public BiologicalMatrix BiologicalMatrixTo { get; set; }
        public DoseUnit DoseUnitTo { get; set; }
        public ExpressionType ExpressionTypeTo { get; set; }
        public double ConversionFactor { get; set; }
    }
}
