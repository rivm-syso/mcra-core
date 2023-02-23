using MCRA.General;

namespace MCRA.Data.Compiled.Objects {
    public sealed class ConcentrationLimit {
        public Compound Compound { get; set; }
        public Food Food { get; set; }

        public double Limit { get; set; }
        public string ConcentrationUnitString { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public string ValueTypeString { get; set; }
        public string Reference { get; set; }

        public ConcentrationUnit ConcentrationUnit {
            get {
                if (!string.IsNullOrEmpty(ConcentrationUnitString)) {
                    return ConcentrationUnitConverter.FromString(ConcentrationUnitString);
                } else {
                    return ConcentrationUnit.mgPerKg;
                }
            }
        }

        public ConcentrationLimitValueType ValueType {
            get {
                if (!string.IsNullOrEmpty(ValueTypeString)) {
                    return ConcentrationLimitValueTypeConverter.FromString(ValueTypeString);
                } else {
                    return ConcentrationLimitValueType.MaximumResidueLimit;
                }
            }
        }
    }
}
