using MCRA.General;

namespace MCRA.Data.Raw.Objects.RawObjects {
    [RawDataSourceTableID(RawDataSourceTableID.Foods)]
    public class RawFood : IRawDataTableRecord {
        public string idFood { get; set; }
        public string Name { get; set; }
        public string AlternativeName { get; set; }
        public string Description { get; set; }
    }
}
