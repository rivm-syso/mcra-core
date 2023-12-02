using MCRA.General;

namespace MCRA.Data.Raw.Objects.RawTableObjects {
    [RawDataSourceTableID(RawDataSourceTableID.IndividualPropertyValues)]
    public class RawIndividualPropertyValue : IRawDataTableRecord {
        public string idIndividual { get; set; }
        public string PropertyName { get; set; }
        public string TextValue { get; set; }
        public double? DoubleValue { get; set; }
    }
}
