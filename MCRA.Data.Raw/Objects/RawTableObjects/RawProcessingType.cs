using MCRA.General;

namespace MCRA.Data.Raw.Objects.RawTableObjects {
    [RawDataSourceTableID(RawDataSourceTableID.ProcessingTypes)]
    public class RawProcessingType : IRawDataTableRecord {
        public string idProcessingType { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string DistributionType { get; set; }
        public bool BulkingBlending { get; set; }
    }
}
