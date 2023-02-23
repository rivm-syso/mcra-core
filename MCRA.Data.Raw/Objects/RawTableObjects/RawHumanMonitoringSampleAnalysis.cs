using MCRA.General;

namespace MCRA.Data.Raw.Objects.RawObjects {
    [RawDataSourceTableID(RawDataSourceTableID.HumanMonitoringSampleAnalyses)]
    public class RawHumanMonitoringSampleAnalysis : IRawDataTableRecord {
        public string idSampleAnalysis { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string idSample { get; set; }
        public string idAnalyticalMethod { get; set; }
        public DateTime? DateAnalysis { get; set; }
    }
}
