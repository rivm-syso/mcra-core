using MCRA.General;

namespace MCRA.Data.Raw.Objects.RawTableObjects {

    [RawDataSourceTableID(RawDataSourceTableID.ExposureDeterminants)]
    public sealed class RawExposureDeterminant : IRawDataTableRecord {
        public string idExposureDeterminant { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public IndividualPropertyType Type { get; set; }
    }
}
