using MCRA.General;

namespace MCRA.Data.Raw.Objects.RawTableObjects {
    [RawDataSourceTableID(RawDataSourceTableID.HumanMonitoringTimepoints)]
    public sealed class RawHumanMonitoringTimepoint : IRawDataTableRecord {
        public string idTimepoint { get; set; }
        public string idSurvey { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
