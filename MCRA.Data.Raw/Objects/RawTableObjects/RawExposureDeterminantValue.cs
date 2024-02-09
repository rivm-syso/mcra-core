using MCRA.General;

namespace MCRA.Data.Raw.Objects.RawTableObjects {

    [RawDataSourceTableID(RawDataSourceTableID.ExposureDeterminantValues)]
    public sealed class RawExposureDeterminantValue : IRawDataTableRecord {
        public string idExposureDeterminantCombination { get; set; }
        public string PropertyName { get; set; }
        public string TextValue { get; set; }
        public double? DoubleValue { get; set; }
    }
}
