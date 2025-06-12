using MCRA.General;
using NCalc;

namespace MCRA.Data.Compiled.Objects {

    /// <summary>
    /// Exposure Response Function for EBD calculation.
    /// </summary>
    public sealed class ExposureResponseFunction : StrongEntity {

        public Compound Substance { get; set; }
        public Effect Effect { get; set; }

        public ExposureTarget ExposureTarget { get; set; }
        public ExposureUnitTriple ExposureUnit { get; set; }

        public EffectMetric EffectMetric { get; set; }
        public ExposureResponseType ExposureResponseType { get; set; }
        public Expression ExposureResponseSpecification { get; set; }
        public Expression ExposureResponseSpecificationLower { get; set; }
        public Expression ExposureResponseSpecificationUpper { get; set; }
        public double CounterfactualValue { get; set; }
        public ICollection<ErfSubgroup> ErfSubgroups { get; set; } = [];
        public PopulationCharacteristicType PopulationCharacteristic { get; set; }
        public double? EffectThresholdLower { get; set; }
        public double? EffectThresholdUpper { get; set; }

        public TargetLevelType TargetLevel => ExposureTarget.TargetLevelType;
        public ExposureRoute ExposureRoute => ExposureTarget.ExposureRoute;
        public BiologicalMatrix BiologicalMatrix => ExposureTarget.BiologicalMatrix;
        public ExpressionType ExpressionType => ExposureTarget.ExpressionType;
        public TargetUnit TargetUnit => new(ExposureTarget, ExposureUnit);

        public bool HasErfSubGroups => ErfSubgroups != null && ErfSubgroups.Count > 0;
    }
}
