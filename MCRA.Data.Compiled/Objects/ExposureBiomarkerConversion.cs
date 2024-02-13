using MCRA.General;

namespace MCRA.Data.Compiled.Objects {
    public sealed class ExposureBiomarkerConversion {
        public string IdExposureBiomarkerConversion { get; set; }
        public Compound SubstanceFrom { get; set; }
        public Compound SubstanceTo { get; set; }
        public BiologicalMatrix BiologicalMatrix { get; set; } = BiologicalMatrix.Undefined;
        public ExposureUnitTriple UnitFrom { get; set; }
        public ExpressionType ExpressionTypeFrom { get; set; } = ExpressionType.None;
        public ExposureUnitTriple UnitTo { get; set; }
        public ExpressionType ExpressionTypeTo { get; set; } = ExpressionType.None;
        public BiomarkerConversionDistribution Distribution { get; set; }
        public double ConversionFactor { get; set; }
        public double? VariabilityUpper { get; set; }
        public ICollection<ExposureBiomarkerConversionSG> EBCSubgroups { get; set; } = new HashSet<ExposureBiomarkerConversionSG>();
    }
}
