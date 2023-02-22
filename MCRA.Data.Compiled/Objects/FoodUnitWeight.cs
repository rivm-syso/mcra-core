using MCRA.General;

namespace MCRA.Data.Compiled.Objects {
    public sealed class FoodUnitWeight {
        public Food Food { get; set; }
        public string Location { get; set; }
        public string ValueTypeString { get; set; }
        public string Reference { get; set; }
        public string QualifierString { get; set; }
        public double Value { get; set; }

        public UnitWeightValueType ValueType {
            get {
                if (!string.IsNullOrEmpty(ValueTypeString)) {
                    return UnitWeightValueTypeConverter.FromString(ValueTypeString);
                }
                return UnitWeightValueType.UnitWeightRac;
            }
            set {
                ValueTypeString = value.ToString();
            }
        }

        public ValueQualifier Qualifier {
            get {
                if (!string.IsNullOrEmpty(QualifierString)) {
                    return ValueQualifierConverter.FromString(QualifierString);
                }
                return ValueQualifier.Equals;
            }
            set {
                QualifierString = value.ToString();
            }
        }

        public QualifiedValue QualifiedValue {
            get {
                return new QualifiedValue(Value, Qualifier);
            }
        }
    }
}
