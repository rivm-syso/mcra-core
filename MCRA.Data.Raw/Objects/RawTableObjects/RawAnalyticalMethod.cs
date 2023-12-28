using MCRA.General;

namespace MCRA.Data.Raw.Objects.RawTableObjects {
    [RawDataSourceTableID(RawDataSourceTableID.AnalyticalMethods)]
    public sealed class RawAnalyticalMethod : IRawDataTableRecord {
        public string idAnalyticalMethod { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
