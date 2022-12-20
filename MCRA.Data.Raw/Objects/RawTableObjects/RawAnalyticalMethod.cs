using MCRA.General;

namespace MCRA.Data.Raw.Objects.RawObjects {
    [RawDataSourceTableID(RawDataSourceTableID.AnalyticalMethods)]
    public class RawAnalyticalMethod : IRawDataTableRecord {
        public string idAnalyticalMethod { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
