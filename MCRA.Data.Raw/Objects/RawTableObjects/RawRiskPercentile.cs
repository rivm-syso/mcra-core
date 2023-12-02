using MCRA.General;

namespace MCRA.Data.Raw.Objects.RawTableObjects {

    [RawDataSourceTableID(RawDataSourceTableID.RiskPercentiles)]
    public sealed class RawRiskPercentile : IRawDataTableRecord {
        public string idRiskModel { get; set; }
        public double Percentage { get; set; }
        public double Risk { get; set; }
    }
}
