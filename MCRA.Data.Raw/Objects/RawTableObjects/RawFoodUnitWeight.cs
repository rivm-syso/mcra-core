using MCRA.General;

namespace MCRA.Data.Raw.Objects.RawTableObjects {
    [RawDataSourceTableID(RawDataSourceTableID.FoodUnitWeights)]
    public class RawFoodUnitWeight : IRawDataTableRecord {
        public string idFood { get; set; }
        public string Location { get; set; }
        public UnitWeightValueType ValueType { get; set; }
        public ValueQualifier Qualifier { get; set; }
        public double Value { get; set; }
        public string Reference { get; set; }
    }
}
