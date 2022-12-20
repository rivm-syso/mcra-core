using MCRA.General;

namespace MCRA.Data.Raw.Objects.RawObjects {
    [RawDataSourceTableID(RawDataSourceTableID.HumanMonitoringSampleConcentrations)]
    public class RawHumanMonitoringSampleConcentration : IRawDataTableRecord {
        public string idAnalysisSample { get; set; }
        public string idCompound { get; set; }
        public double? Concentration { get; set; }
        public string ResType { get; set; }
    }
}
