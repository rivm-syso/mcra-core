using MCRA.General;

namespace MCRA.Data.Raw.Objects.DietaryExposures {

    [RawDataSourceTableID(RawDataSourceTableID.DietaryExposurePercentiles)]
    public sealed class RawDietaryExposurePercentileRecord : IRawDataTableRecord {
        public string idDietaryExposureModel { get; set; }
        public double Percentage { get; set; }
        public double Exposure { get; set; }
    }
}
