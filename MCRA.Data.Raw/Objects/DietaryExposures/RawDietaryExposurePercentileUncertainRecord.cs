using MCRA.General;

namespace MCRA.Data.Raw.Objects.DietaryExposures {

    [RawDataSourceTableID(RawDataSourceTableID.DietaryExposurePercentilesUncertain)]
    public sealed class RawDietaryExposurePercentileUncertainRecord : IRawDataTableRecord {
        public string idDietaryExposureModel { get; set; }
        public string idUncertaintySet { get; set; }
        public double Percentage { get; set; }
        public double Exposure { get; set; }
    }
}
