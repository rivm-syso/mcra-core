using MCRA.General;

namespace MCRA.Data.Raw.Objects.RawTableObjects {

    [RawDataSourceTableID(RawDataSourceTableID.DietaryExposureModels)]
    public sealed class RawDietaryExposureModelRecord : IRawDataTableRecord {
        public string idDietaryExposureModel { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string idSubstance { get; set; }
        public string ExposureUnit { get; set; }
    }
}
