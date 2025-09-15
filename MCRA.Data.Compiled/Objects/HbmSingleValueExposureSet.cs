using MCRA.General;

namespace MCRA.Data.Compiled.Objects {
    public sealed class HbmSingleValueExposureSet : StrongEntity {
        public Compound Substance { get; set; }
        public List<HbmSingleValueExposure> HbmSingleValueExposures { get; set; }
        public HbmSingleValueExposureSurvey Survey {  get; set; } 
        public DoseUnit DoseUnit { get; set; } = DoseUnit.ugPerL;
        public ExposureRoute ExposureRoute { get; set; }
        public BiologicalMatrix BiologicalMatrix { get; set; }
        public ExpressionType ExpressionType { get; set; }
    }
}
