using MCRA.General;

namespace MCRA.Data.Compiled.Objects {
    public sealed class PopulationConsumptionSingleValue {
        public Population Population { get; set; }
        public Food Food { get; set; }
        public string ValueTypeString { get; set; }
        public double? Percentile { get; set; }
        public double ConsumptionAmount { get; set; }
        public string ConsumptionUnitString { get; set; }
        public string Reference { get; set; }

        public ConsumptionIntakeUnit ConsumptionUnit {
            get {
                if (!string.IsNullOrEmpty(ConsumptionUnitString)) {
                    return ConsumptionIntakeUnitConverter.FromString(ConsumptionUnitString);
                } else {
                    return ConsumptionIntakeUnit.gPerKgBWPerDay;
                }
            }
            set {
                ConsumptionUnitString = value.ToString();
            }
        }

        public ConsumptionValueType ValueType {
            get {
                if (!string.IsNullOrEmpty(ValueTypeString)) {
                    return ConsumptionValueTypeConverter.FromString(ValueTypeString);
                } else {
                    return ConsumptionValueType.Undefined;
                }
            }
            set {
                ValueTypeString = value.ToString();
            }
        }
    }
}
