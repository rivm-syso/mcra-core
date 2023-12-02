using MCRA.General;

namespace MCRA.Data.Raw.Objects.RawTableObjects {

    [RawDataSourceTableID(RawDataSourceTableID.DietaryExposurePercentiles)]
    public sealed class RawDietaryExposurePercentile : IRawDataTableRecord {
        public string idDietaryExposureModel { get; set; }
        public double Percentage { get; set; }
        public double Exposure { get; set; }
    }
}
