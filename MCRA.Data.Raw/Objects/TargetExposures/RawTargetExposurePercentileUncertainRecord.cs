using MCRA.General;

namespace MCRA.Data.Raw.Objects.TargetExposures {

    [RawDataSourceTableID(RawDataSourceTableID.TargetExposurePercentilesUncertain)]
    public sealed class RawTargetExposurePercentileUncertainRecord : IRawDataTableRecord {
        public string idTargetExposureModel { get; set; }
        public string idUncertaintySet { get; set; }
        public double Percentage { get; set; }
        public double Exposure { get; set; }
    }
}
