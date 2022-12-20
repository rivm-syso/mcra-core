using MCRA.General;

namespace MCRA.Data.Raw.Objects.Risks {

    [RawDataSourceTableID(RawDataSourceTableID.RiskPercentiles)]
    public sealed class RawRiskPercentileRecord : IRawDataTableRecord {
        public string idRiskModel { get; set; }
        public double Percentage { get; set; }
        public double MarginOfExposure { get; set; }
    }
}
