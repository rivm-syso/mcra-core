using MCRA.General;

namespace MCRA.Data.Compiled.Objects {
    public sealed class FoodUnitWeight {
        public Food Food { get; set; }
        public string Location { get; set; }
        public string Reference { get; set; }
        public double Value { get; set; }
        public UnitWeightValueType ValueType { get; set; } = UnitWeightValueType.UnitWeightRac;
        public ValueQualifier Qualifier { get; set; } = ValueQualifier.Equals;

        public QualifiedValue QualifiedValue => new(Value, Qualifier);
    }
}
