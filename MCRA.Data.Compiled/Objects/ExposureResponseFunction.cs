using MCRA.General;
using NCalc;

namespace MCRA.Data.Compiled.Objects {

    /// <summary>
    /// Exposure Response Function for EBD calculation.
    /// </summary>
    public sealed class ExposureResponseFunction : StrongEntity {

        public Compound Substance { get; set; }
        public Effect Effect { get; set; }
        public TargetLevelType TargetLevel { get; set; }
        public ExposureRoute ExposureRoute { get; set; }
        public BiologicalMatrix BiologicalMatrix { get; set; }
        public DoseUnit DoseUnit { get; set; }
        public ExpressionType ExpressionType { get; set; } = ExpressionType.None;
        public EffectMetric EffectMetric { get; set; }
        public ExposureResponseType ExposureResponseType { get; set; }
        public Expression ExposureResponseSpecification { get; set; }
        public double Baseline { get; set; }
        public ICollection<ErfSubgroup> ErfSubgroups { get; set; } = [];
        public PopulationCharacteristicType PopulationCharacteristic { get; set; }
        public double? EffectThresholdLower { get; set; }
        public double? EffectThresholdUpper { get; set; }
        public ExposureTarget ExposureTarget {
            get {
                return TargetLevel == TargetLevelType.External
                    ? new ExposureTarget(ExposureRoute)
                    : new ExposureTarget(BiologicalMatrix, ExpressionType);
            }
        }
        public TargetUnit TargetUnit {
            get {
                if (TargetLevel == TargetLevelType.External) {
                    return TargetUnit.FromExternalDoseUnit(DoseUnit, ExposureRoute);
                } else {
                    return TargetUnit.FromInternalDoseUnit(DoseUnit, BiologicalMatrix, ExpressionType);
                }
            }
        }
        public bool HasErfSubGroups() {
            return ErfSubgroups != null && ErfSubgroups.Count > 0;
        }
    }
}
