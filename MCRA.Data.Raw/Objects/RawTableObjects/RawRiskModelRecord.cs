using MCRA.General;

namespace MCRA.Data.Raw.Objects.RawTableObjects {

    [RawDataSourceTableID(RawDataSourceTableID.RiskModels)]
    public sealed class RawRiskModelRecord : IRawDataTableRecord {
        public string idRiskModel { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string idSubstance { get; set; }
        public string RiskMetric { get; set; }
    }
}
