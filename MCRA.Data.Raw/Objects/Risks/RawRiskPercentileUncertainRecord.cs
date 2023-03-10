using MCRA.General;

namespace MCRA.Data.Raw.Objects.Risks {

    [RawDataSourceTableID(RawDataSourceTableID.RiskPercentilesUncertain)]
    public sealed class RawRiskPercentileUncertainRecord : IRawDataTableRecord {
        public string idRiskModel { get; set; }
        public string idUncertaintySet { get; set; }
        public double Percentage { get; set; }
        public double Risk { get; set; }
    }
}
