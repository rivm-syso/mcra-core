using MCRA.General;

namespace MCRA.Data.Raw.Objects.RawTableObjects {
    [RawDataSourceTableID(RawDataSourceTableID.HumanMonitoringSampleConcentrations)]
    public sealed class RawHumanMonitoringSampleConcentration : IRawDataTableRecord {
        public string idAnalysisSample { get; set; }
        public string idCompound { get; set; }
        public double? Concentration { get; set; }
        public string ResType { get; set; }
    }
}
