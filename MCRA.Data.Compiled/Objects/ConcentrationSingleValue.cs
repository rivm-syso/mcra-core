using MCRA.General;

namespace MCRA.Data.Compiled.Objects {
    public sealed class ConcentrationSingleValue {
        public Compound Substance { get; set; }
        public Food Food { get; set; }

        public double Value { get; set; }
        public string ConcentrationUnitString { get; set; }
        public double? Percentile { get; set; }

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
            set {
                ConcentrationUnitString = value.ToString();
            }
        }

        public ConcentrationValueType ValueType {
            get {
                if (!string.IsNullOrEmpty(ValueTypeString)) {
                    return ConcentrationValueTypeConverter.FromString(ValueTypeString);
                } else {
                    return ConcentrationValueType.MeanConcentration;
                }
            }
            set {
                ValueTypeString = value.ToString();
            }
        }
    }
}
