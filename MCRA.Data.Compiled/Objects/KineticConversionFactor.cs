using MCRA.General;

namespace MCRA.Data.Compiled.Objects {
    public sealed class KineticConversionFactor {
        private ExposureTarget _targetFrom;
        private ExposureTarget _targetTo;

        public Compound SubstanceFrom { get; set; }
        public ExposurePathType ExposureRouteFrom { get; set; } = ExposurePathType.Undefined;
        public BiologicalMatrix BiologicalMatrixFrom { get; set; } = BiologicalMatrix.Undefined;
        public ExposureUnitTriple DoseUnitFrom { get; set; }
        public ExpressionType ExpressionTypeFrom { get; set; } = ExpressionType.None;
        public Compound SubstanceTo { get; set; }
        public ExposurePathType ExposureRouteTo { get; set; } = ExposurePathType.Undefined;
        public BiologicalMatrix BiologicalMatrixTo { get; set; } = BiologicalMatrix.Undefined;
        public ExposureUnitTriple DoseUnitTo { get; set; }
        public ExpressionType ExpressionTypeTo { get; set; } = ExpressionType.None;
        public double ConversionFactor { get; set; }

        public ExposureTarget TargetFrom {
            get {
                _targetFrom = ExposureRouteFrom != ExposurePathType.AtTarget
                    && ExposureRouteFrom != ExposurePathType.Undefined
                    ? new ExposureTarget(ExposureRouteFrom)
                    : new ExposureTarget(BiologicalMatrixFrom, ExpressionTypeFrom);
                return _targetFrom;
            }
        }

        public ExposureTarget TargetTo {
            get {
                _targetTo = ExposureRouteTo != ExposurePathType.AtTarget
                    && ExposureRouteTo != ExposurePathType.Undefined
                    ? new ExposureTarget(ExposureRouteTo)
                    : new ExposureTarget(BiologicalMatrixTo, ExpressionTypeTo);
                return _targetTo;
            }
        }
    }
}
