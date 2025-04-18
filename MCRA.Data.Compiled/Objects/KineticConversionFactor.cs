﻿using MCRA.General;

namespace MCRA.Data.Compiled.Objects {
    public sealed class KineticConversionFactor {
        private ExposureTarget _targetFrom;
        private ExposureTarget _targetTo;

        public string IdKineticConversionFactor { get; set; }
        public Compound SubstanceFrom { get; set; }
        public ExposureRoute ExposureRouteFrom { get; set; } = ExposureRoute.Undefined;
        public BiologicalMatrix BiologicalMatrixFrom { get; set; } = BiologicalMatrix.Undefined;
        public ExposureUnitTriple DoseUnitFrom { get; set; }
        public ExpressionType ExpressionTypeFrom { get; set; } = ExpressionType.None;
        public Compound SubstanceTo { get; set; }
        public ExposureRoute ExposureRouteTo { get; set; } = ExposureRoute.Undefined;
        public BiologicalMatrix BiologicalMatrixTo { get; set; } = BiologicalMatrix.Undefined;
        public ExposureUnitTriple DoseUnitTo { get; set; }
        public ExpressionType ExpressionTypeTo { get; set; } = ExpressionType.None;
        public double ConversionFactor { get; set; }
        public KineticConversionFactorDistributionType Distribution { get; set; } = KineticConversionFactorDistributionType.Unspecified;
        public double? UncertaintyUpper { get; set; }

        public ICollection<KineticConversionFactorSG> KCFSubgroups { get; set; } = [];

        public ExposureTarget TargetFrom {
            get {
                _targetFrom = ExposureRouteFrom != ExposureRoute.Undefined
                    ? new ExposureTarget(ExposureRouteFrom)
                    : new ExposureTarget(BiologicalMatrixFrom, ExpressionTypeFrom);
                return _targetFrom;
            }
        }

        public ExposureTarget TargetTo {
            get {
                _targetTo = ExposureRouteTo != ExposureRoute.Undefined
                    ? new ExposureTarget(ExposureRouteTo)
                    : new ExposureTarget(BiologicalMatrixTo, ExpressionTypeTo);
                return _targetTo;
            }
        }
        public ExposureRoute ExposureRoute => ExposureRouteFrom;

        public static KineticConversionFactor FromDefaultAbsorptionFactor(
            ExposureRoute route,
            Compound substance,
            double factor
        ) {
            var kineticConversionFactor = new KineticConversionFactor() {
                SubstanceFrom = substance,
                ExposureRouteFrom = route,
                DoseUnitFrom = ExposureUnitTriple.FromExposureUnit(ExternalExposureUnit.ugPerKgBWPerDay),
                SubstanceTo = substance,
                DoseUnitTo = ExposureUnitTriple.FromDoseUnit(DoseUnit.ugPerKg),
                ConversionFactor = factor,
            };
            return kineticConversionFactor;
        }
    }
}
