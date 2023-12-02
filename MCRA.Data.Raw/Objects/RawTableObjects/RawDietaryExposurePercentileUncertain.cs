using MCRA.General;

namespace MCRA.Data.Raw.Objects.RawTableObjects {

    [RawDataSourceTableID(RawDataSourceTableID.DietaryExposurePercentilesUncertain)]
    public sealed class RawDietaryExposurePercentileUncertain : IRawDataTableRecord {
        public string idDietaryExposureModel { get; set; }
        public string idUncertaintySet { get; set; }
        public double Percentage { get; set; }
        public double Exposure { get; set; }
    }
}
